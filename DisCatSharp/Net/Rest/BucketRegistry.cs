using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net;

/// <summary>
///     Owns the three-dictionary bucket registry (routes → hashes → buckets → workers)
///     and provides atomic remap synchronization so that <c>UpdateHashCaches</c> never
///     exposes a half-updated state to concurrent <c>GetBucket</c> callers.
/// </summary>
internal sealed class BucketRegistry : IDisposable
{
	/// <summary>
	///     Caches reflected <see cref="PropertyInfo" /> arrays per route-parameter type so that
	///     <see cref="GetBucket" /> (a hot path) does not call reflection on every request.
	/// </summary>
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_propertyCache = new();

	/// <summary>
	///     Maps generic route patterns (e.g. "POST:/channels/:channel_id/messages") to rate-limit hashes.
	/// </summary>
	private readonly ConcurrentDictionary<string, string> _routesToHashes = new();

	/// <summary>
	///     Maps bucket IDs (hash:guild:channel:webhook) to bucket objects.
	/// </summary>
	private readonly ConcurrentDictionary<string, RateLimitBucket> _hashesToBuckets = new();

	/// <summary>
	///     Maps bucket object references to their workers, using reference equality
	///     for stable identity across hash transitions.
	/// </summary>
	private readonly ConcurrentDictionary<RateLimitBucket, BucketWorker> _bucketWorkers = new(ReferenceEqualityComparer.Instance);

	/// <summary>
	///     Remap lock — only held during structural changes (hash remap, bucket migration).
	///     Hot-path lookups (<see cref="GetBucket" />, <see cref="GetOrCreateWorker" />) do NOT acquire this.
	/// </summary>
	private readonly Lock _remapLock = new();

	/// <summary>
	///     The logger for registry operations.
	/// </summary>
	private readonly ILogger _logger;

	/// <summary>
	///     The REST client that owns this registry, needed for worker creation.
	/// </summary>
	private readonly RestClient _client;

	/// <summary>
	///     The advanced REST configuration, passed to workers on creation.
	/// </summary>
	private readonly RestAdvancedConfiguration _config;

	/// <summary>
	///     Whether to use reset-after (relative) ratelimit timing.
	/// </summary>
	private readonly bool _useResetAfter;

	/// <summary>
	///     The bucket cleanup delay.
	/// </summary>
	private readonly TimeSpan _bucketCleanupDelay;

	/// <summary>
	///     Cancellation token source for the bucket cleaner task.
	/// </summary>
	private CancellationTokenSource? _bucketCleanerTokenSource;

	/// <summary>
	///     Whether the bucket cleaner is running.
	/// </summary>
	private int _cleanerRunning;

	/// <summary>
	///     The cleaner task.
	/// </summary>
	private Task? _cleanerTask;

	/// <summary>
	///     Whether the registry has been disposed.
	/// </summary>
	private volatile bool _disposed;

	/// <summary>
	///     Initializes a new instance of the <see cref="BucketRegistry" /> class.
	/// </summary>
	/// <param name="client">The owning REST client (passed to workers for HTTP and global-gate access).</param>
	/// <param name="config">Advanced REST configuration (queue timeout, retries, etc.).</param>
	/// <param name="logger">Logger for diagnostic output.</param>
	/// <param name="useResetAfter">Whether to use relative ratelimit timing.</param>
	/// <param name="bucketCleanupDelay">How often the cleaner runs.</param>
	internal BucketRegistry(
		RestClient client,
		RestAdvancedConfiguration config,
		ILogger logger,
		bool useResetAfter,
		TimeSpan bucketCleanupDelay)
	{
		this._client = client;
		this._config = config;
		this._logger = logger;
		this._useResetAfter = useResetAfter;
		this._bucketCleanupDelay = bucketCleanupDelay;
	}

	// ── Read-only views for diagnostics / cleanup ────────────────────────────

	/// <summary>
	///     Gets the number of tracked buckets.
	/// </summary>
	internal int BucketCount => this._hashesToBuckets.Count;

	/// <summary>
	///     Gets the number of tracked workers.
	/// </summary>
	internal int WorkerCount => this._bucketWorkers.Count;

	/// <summary>
	///     Gets whether the bucket collection is empty.
	/// </summary>
	internal bool IsEmpty => this._hashesToBuckets.IsEmpty;

	// ── Hot-path methods (lock-free) ─────────────────────────────────────────

	/// <summary>
	///     Resolves a request's method + route + parameters to a <see cref="RateLimitBucket" />.
	///     This is the hot path — no lock is taken; <see cref="ConcurrentDictionary{TKey,TValue}" /> handles concurrency.
	/// </summary>
	/// <param name="method">The HTTP method.</param>
	/// <param name="route">The route template (with parameter placeholders).</param>
	/// <param name="routeParams">Anonymous object carrying guild_id, channel_id, webhook_id, etc.</param>
	/// <param name="url">Outputs the fully-resolved URL.</param>
	/// <returns>The rate-limit bucket for this request.</returns>
	internal RateLimitBucket GetBucket(RestRequestMethod method, string route, object routeParams, out string url)
	{
		var rparamsProps = s_propertyCache.GetOrAdd(routeParams.GetType(), static t => t.GetTypeInfo().DeclaredProperties.ToArray());
		var rparams = new Dictionary<string, string>();
		foreach (var xp in rparamsProps)
		{
			var val = xp.GetValue(routeParams);
			rparams[xp.Name] = val switch
			{
				string xs => xs,
				DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
				IFormattable xf => xf.ToString(null, CultureInfo.InvariantCulture),
				_ => val.ToString()
			};
		}

		var guildId = rparams.GetValueOrDefault("guild_id", "");
		var channelId = rparams.GetValueOrDefault("channel_id", "");
		var webhookId = rparams.GetValueOrDefault("webhook_id", "");

		var hashKey = RateLimitBucket.GenerateHashKey(method, route);

		var hash = this._routesToHashes.GetOrAdd(hashKey, RateLimitBucket.GenerateUnlimitedHash(method, route));

		var bucketId = RateLimitBucket.GenerateBucketId(hash, guildId, channelId, webhookId);

		var bucket = this._hashesToBuckets.GetOrAdd(bucketId, new RateLimitBucket(hash, guildId, channelId, webhookId));

		bucket.LastAttemptAt = DateTimeOffset.UtcNow;

		if (!Enumerable.Contains(bucket.RouteHashes, bucketId))
			bucket.RouteHashes.Add(bucketId);

		this.EnsureCleanerRunning();

		url = CommonRegEx.HttpRouteRegex().Replace(route, xm => rparams[xm.Groups[1].Value]);
		return bucket;
	}

	/// <summary>
	///     Gets or creates a <see cref="BucketWorker" /> for the given bucket.
	///     Uses the bucket object reference as key to remain stable across hash transitions.
	///     This is a hot path — no lock is taken.
	/// </summary>
	/// <param name="bucket">The rate-limit bucket.</param>
	/// <returns>The worker for this bucket.</returns>
	internal BucketWorker GetOrCreateWorker(RateLimitBucket bucket)
		=> this._bucketWorkers.GetOrAdd(bucket, static (b, ctx) => new(ctx.client, b, ctx.config, ctx.logger), (client: this._client, config: this._config, logger: this._logger));

	/// <summary>
	///     Migrates queued requests from the <paramref name="sourceBucket" />'s worker to the
	///     <paramref name="targetBucket" />'s worker. The source worker's in-flight request (if any)
	///     completes on the source worker; it idles out naturally after migration.
	///     The source worker is removed from the registry but NOT disposed.
	/// </summary>
	/// <param name="sourceBucket">The bucket whose worker should be drained.</param>
	/// <param name="targetBucket">The bucket whose worker should receive the migrated requests.</param>
	/// <returns>The number of requests migrated, or 0 if no migration was needed.</returns>
	internal int MigrateBucketWorker(RateLimitBucket sourceBucket, RateLimitBucket targetBucket)
	{
		if (!this._bucketWorkers.TryGetValue(sourceBucket, out var sourceWorker))
			return 0;

		var targetWorker = this.GetOrCreateWorker(targetBucket);
		var migrated = sourceWorker.MigrateQueueTo(targetWorker);

		// Remove the source worker from the registry — don't dispose; let it drain in-flight work
		this._bucketWorkers.TryRemove(sourceBucket, out _);

		if (migrated > 0)
			this._logger.LogDebug(LoggerEvents.RestHashMover, "Migrated {Count} queued request(s) from {Source} to {Target}", migrated, sourceBucket, targetBucket);

		return migrated;
	}

	// ── Atomic remap (acquires _remapLock) ───────────────────────────────────

	/// <summary>
	///     Updates the route → hash and bucket-ID → bucket caches when Discord reveals
	///     a real rate-limit hash. Acquires <see cref="_remapLock" /> to make the multi-dict
	///     update atomic, preventing concurrent <see cref="GetBucket" /> callers from seeing
	///     a half-updated state.
	/// </summary>
	/// <param name="request">The request whose route is being remapped.</param>
	/// <param name="bucket">The bucket to remap.</param>
	/// <param name="newHash">The new hash from Discord's response headers, or <c>null</c> to remove an unlimited bucket.</param>
	internal void UpdateHashCaches(BaseRestRequest request, RateLimitBucket bucket, string? newHash = null)
	{
		var hashKey = RateLimitBucket.GenerateHashKey(request.Method, request.Route);

		if (!this._routesToHashes.TryGetValue(hashKey, out var oldHash))
			return;

		// Unlimited bucket with no new hash — just remove it and any stale alias
		if (newHash is null)
		{
			lock (this._remapLock)
			{
				_ = this._routesToHashes.TryRemove(hashKey, out _);

				// Remove the bucket by its ID
				if (bucket.BucketId is not null)
					_ = this._hashesToBuckets.TryRemove(bucket.BucketId, out _);

				// Also remove the old hash alias to prevent stale resolution
				_ = this._hashesToBuckets.TryRemove(oldHash, out _);
			}

			return;
		}

		// Only update the hash once per route, due to a Discord API quirk.
		if (!bucket.IsUnlimited || newHash == oldHash)
			return;

		lock (this._remapLock)
		{
			this._logger.LogDebug(LoggerEvents.RestHashMover, "Updating hash in {0}: \"{1}\" -> \"{2}\"", hashKey, oldHash, newHash);
			var bucketId = RateLimitBucket.GenerateBucketId(newHash, bucket.GuildId, bucket.ChannelId, bucket.WebhookId);

			// Check for merge case: another route already registered a different bucket for this hash+params.
			// If so, migrate queued work from our worker to the existing bucket's worker.
			RateLimitBucket? mergeBucket = null;
			if (this._hashesToBuckets.TryGetValue(bucketId, out var existingBucket) && !ReferenceEquals(existingBucket, bucket))
				mergeBucket = existingBucket;

			_ = this._routesToHashes.AddOrUpdate(hashKey, newHash, (_, _) =>
			{
				bucket.Hash = newHash;

				if (mergeBucket is not null)
				{
					// Merge case: migrate queued work from our worker to the existing bucket's worker
					this._logger.LogDebug(LoggerEvents.RestHashMover, "Bucket merge detected for {0}: migrating work to existing bucket", bucketId);
					this.MigrateBucketWorker(bucket, mergeBucket);
				}
				else
				{
					// Keep the old entry in _hashesToBuckets so concurrent lock-free GetBucket
					// callers that still see the old hash can resolve to the same bucket object.
					// The cleaner will remove the stale entry once the bucket expires.
					_ = this._hashesToBuckets.AddOrUpdate(bucketId, bucket, (_, _) => bucket);
				}

				return newHash;
			});
		}
	}

	// ── Cleanup ──────────────────────────────────────────────────────────────

	/// <summary>
	///     Ensures the bucket cleaner task is running.
	/// </summary>
	private void EnsureCleanerRunning()
	{
		if (Interlocked.CompareExchange(ref this._cleanerRunning, 1, 0) != 0)
			return;

		this._bucketCleanerTokenSource = new();
		this._cleanerTask = Task.Run(this.CleanupBucketsAsync, this._bucketCleanerTokenSource.Token);
		this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task started.");
	}

	/// <summary>
	///     Periodically cleans up dead workers and expired buckets.
	///     Also performs fault recovery: detects crashed workers, requeues their pending work
	///     into a fresh replacement worker, and disposes the faulted one.
	/// </summary>
	private async Task CleanupBucketsAsync()
	{
		while (this._bucketCleanerTokenSource is { IsCancellationRequested: false })
		{
			try
			{
				await Task.Delay(this._bucketCleanupDelay, this._bucketCleanerTokenSource.Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{ }

			ObjectDisposedException.ThrowIf(this._disposed, this);

			// ── Fault recovery and dead worker cleanup ────────────────────────
			foreach (var key in this._bucketWorkers.Keys.ToList())
			{
				if (!this._bucketWorkers.TryGetValue(key, out var worker))
					continue;

				if (worker.IsAlive)
					continue;

				if (worker.IsFaulted)
				{
					// Worker crashed — recover queued work into a replacement
					this._logger.LogWarning(
						LoggerEvents.RestCleaner,
						worker.FaultException,
						"Bucket worker faulted for {Bucket} — recovering {QueueLength} queued request(s)",
						key,
						worker.QueueLength);

					if (worker.QueueLength > 0)
					{
						// Create a fresh replacement worker for this bucket
						var replacement = new BucketWorker(this._client, key, this._config, this._logger);

						// Drain the faulted worker's remaining queue into the replacement
						var recovered = worker.MigrateQueueTo(replacement);

						if (recovered > 0)
						{
							Interlocked.Add(ref replacement.Recovered, recovered);
							this._logger.LogInformation(
								LoggerEvents.RestCleaner,
								"Recovered {Count} request(s) from faulted worker for {Bucket}",
								recovered,
								key);
						}

						// Swap the worker in the registry — if the key was removed concurrently,
						// try to re-add it. If that also fails, drain the migrated requests
						// back to avoid ObjectDisposedException on dispose.
						if (!this._bucketWorkers.TryUpdate(key, replacement, worker))
						{
							if (!this._bucketWorkers.TryAdd(key, replacement))
							{
								this._logger.LogWarning(LoggerEvents.RestCleaner,
									"Failed to register replacement worker for {Bucket} — draining {Count} request(s) back to caller as faulted", key, replacement.QueueLength);

								// Drain remaining requests and fail them explicitly — better than silent ObjectDisposedException
								while (replacement.TryDequeue(out var orphaned))
								{
									orphaned.TrySetFaulted(new InvalidOperationException($"Fault recovery failed for bucket {key}: worker could not be registered."));
									orphaned.CancellationTokenSource.Dispose();
								}

								replacement.Dispose();
								continue;
							}
						}

						// Start the replacement's processing loop so migrated requests get executed
						if (recovered > 0)
							replacement.EnsureRunning();
					}
					else
					{
						// No queued work — just remove the dead worker
						if (this._bucketWorkers.TryRemove(key, out _))
							this._logger.LogDebug(LoggerEvents.RestCleaner, "Removed faulted worker with empty queue for {Bucket}", key);
					}

					worker.Dispose();
				}
				else if (worker.Processed > 0)
				{
					// Normal idle shutdown — just clean up
					if (this._bucketWorkers.TryRemove(key, out var removed))
						removed.Dispose();
				}
			}

			var removedBuckets = 0;
			StringBuilder? bucketIdStrBuilder = default;

			foreach (var (key, value) in this._hashesToBuckets)
			{
				bucketIdStrBuilder ??= new();

				if (string.IsNullOrEmpty(value.BucketId))
					continue;

				if (this._bucketWorkers.TryGetValue(value, out var activeWorker) && activeWorker.IsAlive && !value.IsUnlimited)
					continue;

				var resetOffset = this._useResetAfter ? value.ResetAfterOffset : value.Reset;

				if (!value.IsUnlimited && (resetOffset > DateTimeOffset.UtcNow || DateTimeOffset.UtcNow - resetOffset < this._bucketCleanupDelay))
					continue;

				_ = this._hashesToBuckets.TryRemove(key, out _);
				removedBuckets++;
				bucketIdStrBuilder.Append(value.BucketId).Append(", ");
			}

			if (removedBuckets > 0)
				this._logger.LogDebug(LoggerEvents.RestCleaner, "Removed {0} unused bucket{1}: [{2}]", removedBuckets, removedBuckets > 1 ? "s" : string.Empty, bucketIdStrBuilder.ToString().TrimEnd(',', ' '));

			if (this._hashesToBuckets.Count > 10_000)
				this._logger.LogWarning(LoggerEvents.RestCleaner, "Bucket accumulation warning: {Count} rate-limit buckets are currently tracked. Cleanup may not be keeping up; consider reviewing route cardinality.", this._hashesToBuckets.Count);

			if (this._hashesToBuckets.IsEmpty)
				break;
		}

		if (this._bucketCleanerTokenSource is { IsCancellationRequested: false })
			this._bucketCleanerTokenSource?.Cancel();

		Interlocked.Exchange(ref this._cleanerRunning, 0);
		this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
	}

	// ── Dispose ──────────────────────────────────────────────────────────────

	/// <summary>
	///     Disposes all workers, clears all dictionaries, and stops the cleaner task.
	/// </summary>
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;

		// Dispose all bucket workers first — cancels their CTS so they exit cleanly
		foreach (var (_, worker) in this._bucketWorkers)
			worker.Dispose();

		this._bucketWorkers.Clear();

		if (this._bucketCleanerTokenSource is { IsCancellationRequested: false })
		{
			this._bucketCleanerTokenSource?.Cancel();
			this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket cleaner task stopped.");
		}

		try
		{
			this._cleanerTask?.Dispose();
			this._bucketCleanerTokenSource?.Dispose();
		}
		catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
		{ }

		this._routesToHashes.Clear();
		this._hashesToBuckets.Clear();
	}

	// ── Diagnostics helpers ─────────────────────────────────────────────────

	/// <summary>
	///     Gets the number of active bucket workers.
	/// </summary>
	internal int GetActiveWorkerCount()
		=> this._bucketWorkers.Count;

	/// <summary>
	///     Gets the total number of queued requests across all workers.
	/// </summary>
	internal int GetTotalQueuedRequests()
	{
		var total = 0;
		foreach (var kvp in this._bucketWorkers)
			total += kvp.Value.QueueLength;

		return total;
	}

	/// <summary>
	///     Gets a diagnostic snapshot of all bucket workers.
	/// </summary>
	internal List<BucketDiagnostics> GetBucketSnapshots()
	{
		var snapshots = new List<BucketDiagnostics>(this._bucketWorkers.Count);
		foreach (var kvp in this._bucketWorkers)
		{
			var worker = kvp.Value;
			snapshots.Add(new(
				BucketId: kvp.Key.ToString(),
				QueueLength: worker.QueueLength,
				Processed: Interlocked.Read(ref worker.Processed),
				Retried: Interlocked.Read(ref worker.Retried),
				TimedOut: Interlocked.Read(ref worker.TimedOut),
				Cancelled: Interlocked.Read(ref worker.Cancelled),
				ConsecutiveFailures: worker.ConsecutiveFailures,
				IsAlive: worker.IsAlive,
				IsFaulted: worker.IsFaulted));
		}

		return snapshots;
	}

	/// <summary>
	///     Drains all bucket worker queues and faults each pending request with <see cref="OperationCanceledException" />.
	/// </summary>
	/// <param name="reason">The cancellation reason.</param>
	/// <returns>The total number of requests cancelled.</returns>
	internal int CancelAllPendingRequests(string reason)
	{
		var cancelled = 0;
		var ex = new OperationCanceledException(reason);

		foreach (var kvp in this._bucketWorkers)
		{
			var worker = kvp.Value;
			while (worker.TryDequeue(out var request))
			{
				request.TrySetFaulted(ex);
				request.CancellationTokenSource.Dispose();
				cancelled++;
			}
		}

		return cancelled;
	}
}
