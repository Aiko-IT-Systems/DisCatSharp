using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using DisCatSharp.Exceptions;

using Microsoft.Extensions.Logging;

namespace DisCatSharp.Net;

/// <summary>
///     Processes requests for a single rate-limit bucket in strict FIFO order.
///     Workers spawn on demand when a request is enqueued and shut down
///     after an idle grace period with no pending work.
/// </summary>
internal sealed class BucketWorker : IDisposable
{
	/// <summary>
	///     How long the worker loop waits for a new request before shutting down.
	/// </summary>
	private static readonly TimeSpan s_idleGracePeriod = TimeSpan.FromSeconds(30);

	private readonly RateLimitBucket _bucket;
	private readonly RestClient _client;
	private readonly RestAdvancedConfiguration _config;
	private readonly CancellationTokenSource _cts;
	private readonly ILogger _logger;
	private readonly Channel<BaseRestRequest> _queue;
	private readonly Lock _startLock = new();

	private volatile bool _disposed;
	private Task? _loopTask;

	// ── Circuit breaker state ────────────────────────────────────────────────
	private int _consecutiveFailures;
	private DateTimeOffset _circuitOpenSince;

	internal BucketWorker(RestClient client, RateLimitBucket bucket, RestAdvancedConfiguration config, ILogger logger)
	{
		this._client = client;
		this._bucket = bucket;
		this._config = config;
		this._logger = logger;
		this._cts = new();

		// Use bounded channel if MaxQueueDepthPerBucket is configured, otherwise unbounded
		this._queue = config.MaxQueueDepthPerBucket > 0
			? Channel.CreateBounded<BaseRestRequest>(new BoundedChannelOptions(config.MaxQueueDepthPerBucket)
			{
				FullMode = BoundedChannelFullMode.Wait
			})
			: Channel.CreateUnbounded<BaseRestRequest>();
	}

	/// <summary>
	///     Gets the number of requests currently queued.
	/// </summary>
	internal int QueueLength => this._queue.Reader.Count;

	/// <summary>
	///     Attempts to dequeue a pending request without executing it.
	///     Used by cancel/flush operations to drain the queue.
	/// </summary>
	/// <param name="request">The dequeued request, if any.</param>
	/// <returns><c>true</c> if a request was dequeued; <c>false</c> if the queue is empty.</returns>
	internal bool TryDequeue([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out BaseRestRequest? request)
		=> this._queue.Reader.TryRead(out request);

	/// <summary>
	///     Gets the total number of requests that completed (success or fault).
	/// </summary>
	internal long Processed;

	/// <summary>
	///     Gets the total number of retry attempts across all requests.
	/// </summary>
	internal long Retried;

	/// <summary>
	///     Gets the total number of requests that timed out in the queue.
	/// </summary>
	internal long TimedOut;

	/// <summary>
	///     Gets the total number of requests cancelled before execution.
	/// </summary>
	internal long Cancelled;

	/// <summary>
	///     Gets whether this worker's loop is still running (not yet idle-shutdown or crashed).
	/// </summary>
	internal bool IsAlive => this._loopTask is { IsCompleted: false };

	/// <summary>
	///     Gets whether this worker's loop faulted (crashed with an unhandled exception).
	///     Distinguishes crash from normal idle shutdown.
	/// </summary>
	internal bool IsFaulted => this._loopTask is { IsFaulted: true };

	/// <summary>
	///     Gets the exception that crashed the worker loop, if any.
	/// </summary>
	internal Exception? FaultException => this._loopTask?.Exception;

	/// <summary>
	///     Gets the total number of requests recovered from a faulted worker.
	/// </summary>
	internal long Recovered;

	/// <summary>
	///     Enqueues a request into this worker's FIFO queue.
	///     Starts the worker loop if it is not already running.
	///     Rejects the request if the circuit breaker is open or the queue is full.
	/// </summary>
	internal void Enqueue(BaseRestRequest request)
	{
		if (this._disposed)
		{
			request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Bucket worker has been disposed."));
			request.CancellationTokenSource.Dispose();
			return;
		}

		// Circuit breaker check
		if (this._config.CircuitBreakerThreshold > 0 && this._consecutiveFailures >= this._config.CircuitBreakerThreshold)
		{
			var elapsed = DateTimeOffset.UtcNow - this._circuitOpenSince;
			if (elapsed < this._config.CircuitBreakerResetTimeout)
			{
				request.TrySetFaulted(new RestCircuitBrokenException(
					request.Route,
					this._bucket.BucketId,
					this._consecutiveFailures,
					this._circuitOpenSince));
				request.CancellationTokenSource.Dispose();
				return;
			}

			// Half-open: allow this one probe request through, reset will happen on success
			this._logger.LogInformation(LoggerEvents.RestError, "Circuit breaker half-open for {Bucket}, allowing probe request", this._bucket);
		}

		if (!this._queue.Writer.TryWrite(request))
		{
			// Queue is full (bounded channel) or writer completed (migration)
			if (this._config.MaxQueueDepthPerBucket > 0)
			{
				request.TrySetFaulted(new RestQueueFullException(
					request.Route,
					this._bucket.BucketId,
					this.QueueLength,
					this._config.MaxQueueDepthPerBucket));
			}
			else
			{
				request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Bucket worker queue is closed."));
			}

			request.CancellationTokenSource.Dispose();
			return;
		}

		this.EnsureRunning();
	}

	/// <summary>
	///     Starts the worker loop if it is not already running.
	///     Guarded against post-disposal restarts.
	/// </summary>
	internal void EnsureRunning()
	{
		if (this._disposed)
			return;

		if (this._loopTask is null or { IsCompleted: true })
			lock (this._startLock)
			{
				if (this._disposed)
					return;

				if (this._loopTask is null or { IsCompleted: true })
				{
					this._loopTask = Task.Run(() => this.RunAsync(this._cts.Token));

					// Observe the task to prevent UnobservedTaskException — the cleaner
					// detects faults via IsFaulted and handles recovery, but the task
					// itself must be observed or .NET fires the global unobserved handler.
					_ = this._loopTask.ContinueWith(
						static (t, state) =>
						{
							if (t.IsFaulted)
								((ILogger)state!).LogError(LoggerEvents.RestError, t.Exception!.InnerException, "Bucket worker loop faulted");
						},
						this._logger,
						TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
				}
			}
	}

	/// <summary>
	///     Enqueues a request directly into the channel, bypassing circuit breaker and queue-full checks.
	///     Used by <see cref="MigrateQueueTo" /> during fault recovery where dropping requests is unacceptable.
	///     Also useful for test scenarios where the worker loop should not auto-start.
	/// </summary>
	/// <param name="request">The request to enqueue.</param>
	/// <param name="startLoop">Whether to start the worker loop. Defaults to <c>false</c> for migration safety.</param>
	/// <returns><c>true</c> if the request was written; <c>false</c> if the channel is closed.</returns>
	internal bool EnqueueDirect(BaseRestRequest request, bool startLoop = false)
	{
		if (this._queue.Writer.TryWrite(request))
		{
			if (startLoop)
				this.EnsureRunning();

			return true;
		}

		return false;
	}

	/// <summary>
	///     Migrates all queued (not in-flight) requests to the <paramref name="target" /> worker.
	///     Completes the writer to stop accepting new work, drains remaining items,
	///     and enqueues them into the target. The in-flight request (if any)
	///     completes on this worker. After migration, this worker idles out naturally.
	/// </summary>
	/// <param name="target">The worker to receive the migrated requests.</param>
	/// <returns>The number of requests migrated.</returns>
	internal int MigrateQueueTo(BucketWorker target)
	{
		// Stop accepting new work — the reader side continues for the in-flight request
		this._queue.Writer.TryComplete();

		// Drain queued items and move to target via direct write (bypasses circuit breaker + bounds)
		var migrated = 0;
		while (this._queue.Reader.TryRead(out var request))
		{
			if (target.EnqueueDirect(request))
				migrated++;
			else
			{
				request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Target worker queue is closed during migration."));
				request.CancellationTokenSource.Dispose();
			}
		}

		return migrated;
	}

	/// <summary>
	///     The main worker loop. Reads requests from the channel and processes them one at a time.
	///     Exits after <see cref="s_idleGracePeriod" /> with no requests, allowing the worker to restart on demand.
	/// </summary>
	private async Task RunAsync(CancellationToken ct)
	{
		this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket worker started for {Bucket}", this._bucket);

		try
		{
			while (!ct.IsCancellationRequested)
			{
				BaseRestRequest request;
				try
				{
					using var idle = CancellationTokenSource.CreateLinkedTokenSource(ct);
					idle.CancelAfter(s_idleGracePeriod);
					request = await this._queue.Reader.ReadAsync(idle.Token);
				}
				catch (OperationCanceledException) when (!ct.IsCancellationRequested)
				{
					this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket worker idle, shutting down for {Bucket}", this._bucket);
					return;
				}

				try
				{
					// Best-effort pre-execution cancellation
					if (request.CancellationTokenSource.IsCancellationRequested)
					{
						request.TrySetFaulted(new OperationCanceledException("Request was cancelled before execution."));
						Interlocked.Increment(ref this.Cancelled);
						continue;
					}

					// Queue timeout check — fail requests that have been waiting too long
					if (this._config.QueueTimeout > TimeSpan.Zero)
					{
						var waited = DateTimeOffset.UtcNow - request.EnqueuedAt;

						if (waited >= this._config.QueueTimeout)
						{
							Interlocked.Increment(ref this.TimedOut);
							var ex = new RestQueueTimeoutException(
								request.Route,
								this._bucket.BucketId,
								waited,
								this.QueueLength,
								this._client.IsGlobalGateBlocked
							);
							this._logger.LogError(LoggerEvents.RestQueueTimeout, ex, "Request to {Url} timed out in queue after {Duration:F1}s", request.Url.AbsoluteUri, waited.TotalSeconds);
							request.TrySetFaulted(ex);
							continue;
						}
					}

					await this.ExecuteAsync(request, ct);
				}
				finally
				{
					// Dispose the per-request linked CTS to release callback registrations
					// from long-lived caller tokens and avoid memory leaks.
					request.CancellationTokenSource.Dispose();
				}
			}
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// Normal disposal — fall through
		}
		catch (ChannelClosedException)
		{
			// Normal shutdown after MigrateQueueTo completes the source writer — not a crash
			this._logger.LogDebug(LoggerEvents.RestCleaner, "Bucket worker for {Bucket} exited after queue migration", this._bucket);
		}
		catch (Exception ex)
		{
			this._logger.LogError(LoggerEvents.RestError, ex, "Bucket worker crashed for {Bucket}", this._bucket);

			// Re-throw so the Task captures the exception — IsFaulted becomes true
			// and the cleaner can detect the crash for fault recovery
			throw;
		}
	}

	/// <summary>
	///     Processes a single request, including rate limit gating, HTTP send, and retry on 429.
	/// </summary>
	private async Task ExecuteAsync(BaseRestRequest request, CancellationToken ct)
	{
		var retries = 0;
		var maxRetries = this._config.MaxRetries;
		var warnEmitted = false;

		// Combine worker shutdown token with request-level cancellation token
		using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, request.CancellationTokenSource.Token);
		var effectiveCt = linked.Token;

		while (true)
		{
			ct.ThrowIfCancellationRequested();

			// Best-effort cancellation between retries
			if (request.CancellationTokenSource.IsCancellationRequested)
			{
				request.TrySetFaulted(new OperationCanceledException("Request was cancelled during processing."));
				Interlocked.Increment(ref this.Cancelled);
				return;
			}

			// Emit warning if request has been queued too long (once per request)
			if (!warnEmitted && this._config.QueueWarningThreshold > TimeSpan.Zero)
			{
				var waited = DateTimeOffset.UtcNow - request.EnqueuedAt;
				if (waited >= this._config.QueueWarningThreshold)
				{
					warnEmitted = true;
					this._logger.LogWarning(LoggerEvents.RestQueuePressure, "Request to {Url} has been waiting {Duration:F1}s in queue (bucket: {Bucket}, queue depth: {QueueLength})", request.Url.AbsoluteUri, waited.TotalSeconds, this._bucket, this.QueueLength);
				}
			}

			try
			{
				// 1. Wait for the global rate limit gate to open (cancellable to avoid deadlock on dispose)
				await this._client.WaitForGlobalGateAsync(effectiveCt);

				// 2. Per-bucket preemptive rate limiting (skip for unprobed buckets)
				if (this._bucket.LimitValid)
				{
					var now = DateTimeOffset.UtcNow;
					await this._bucket.TryResetLimitAsync(now, this._config.PreemptiveRatelimitBuffer.Ticks);

					if (Interlocked.Decrement(ref this._bucket.RemainingInternal) < 0)
					{
						// Restore the counter — we didn't actually consume a slot
						Interlocked.Increment(ref this._bucket.RemainingInternal);

						var delay = this._client.ComputeBucketDelay(this._bucket);

						if (delay < new TimeSpan(-TimeSpan.TicksPerMinute))
						{
							this._logger.LogError(LoggerEvents.RatelimitDiag, "Stale ratelimit for {Bucket} — allowing next request", this._bucket);
							this._bucket.RemainingInternal = 1;
						}
						else
						{
							if (delay < TimeSpan.Zero)
								delay = TimeSpan.FromMilliseconds(100);

							this._logger.LogWarning(LoggerEvents.RatelimitPreemptive, "Preemptive ratelimit for {Bucket} — waiting {Delay:c}", this._bucket, delay);
							await Task.Delay(delay, effectiveCt);
						}

						continue; // Re-check after waiting
					}

					this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Request for {Bucket} is allowed. Url: {Url}", this._bucket, request.Url.AbsoluteUri);
				}
				else
				{
					this._logger.LogDebug(LoggerEvents.RatelimitDiag, "Probe request for {Bucket} is allowed. Url: {Url}", this._bucket, request.Url.AbsoluteUri);
				}

				// 3. Build, send, parse response, update bucket headers
				var isProbe = !this._bucket.LimitValid;
				var result = await this._client.SendAndParseAsync(request, isProbe, effectiveCt);

				// 4. Handle retry on 429, 5xx, or transient network errors
				if (result.ShouldRetry && retries < maxRetries)
				{
					retries++;
					Interlocked.Increment(ref this.Retried);

					if (result.IsGlobalRateLimit)
					{
						this._logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down for {Url}", request.Url.AbsoluteUri);
						await this._client.EnforceGlobalRateLimitAsync(result.RetryDelay, effectiveCt);
					}
					else if (result.IsTransientNetworkError)
					{
						// Exponential backoff for transient network errors (DNS, socket, timeout)
						var backoff = TimeSpan.FromSeconds(Math.Pow(2, retries - 1));
						this._logger.LogWarning(LoggerEvents.RestError, "Transient network error, retrying {Url} after {Delay:F1}s (attempt {Retry}/{Max})",
							request.Url.AbsoluteUri, backoff.TotalSeconds, retries, maxRetries);
						await Task.Delay(backoff, effectiveCt);
					}
					else if (result.IsServerError)
					{
						// Exponential backoff for transient server errors (502, 503, 504)
						var backoff = TimeSpan.FromSeconds(Math.Pow(2, retries - 1));
						this._logger.LogWarning(LoggerEvents.RestError, "Server error ({Status}), retrying {Url} after {Delay:F1}s (attempt {Retry}/{Max})",
							result.Response.ResponseCode, request.Url.AbsoluteUri, backoff.TotalSeconds, retries, maxRetries);
						await Task.Delay(backoff, effectiveCt);
					}
					else
					{
						this._logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, retrying request to {Url}", request.Url.AbsoluteUri);
						await this._client.RaiseRateLimitHitAsync(request, result.Error as RateLimitException);

						var retryWait = result.RetryDelay + this._config.PreemptiveRatelimitBuffer;
						if (retryWait > TimeSpan.Zero)
							await Task.Delay(retryWait, effectiveCt);
					}

					continue; // Retry
				}

				// 5. Complete or fault the request + circuit breaker tracking
				if (result.Error is not null)
				{
					this._client.ReportDiagnostics(request, result.Response, result.Error);
					request.SetFaulted(result.Error);
					this.RecordFailure();
				}
				else
				{
					request.SetCompleted(result.Response);
					this.RecordSuccess();
				}

				Interlocked.Increment(ref this.Processed);
				return;
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
				request.TrySetFaulted(new OperationCanceledException("Bucket worker was disposed during request processing."));
				throw;
			}
			catch (OperationCanceledException) when (request.CancellationTokenSource.IsCancellationRequested)
			{
				request.TrySetFaulted(new OperationCanceledException("Request was cancelled during processing."));
				Interlocked.Increment(ref this.Cancelled);
				return;
			}
			catch (Exception ex)
			{
				this._logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} failed", request.Url.AbsoluteUri);

				if (!request.TrySetFaulted(ex))
					throw;

				Interlocked.Increment(ref this.Processed);
				return;
			}
		}
	}

	// ── Circuit breaker tracking ─────────────────────────────────────────────

	/// <summary>
	///     Gets the number of consecutive failures on this bucket.
	/// </summary>
	internal int ConsecutiveFailures => this._consecutiveFailures;

	/// <summary>
	///     Records a successful request — resets the circuit breaker.
	/// </summary>
	private void RecordSuccess()
	{
		if (this._consecutiveFailures > 0)
		{
			Interlocked.Exchange(ref this._consecutiveFailures, 0);
			this._circuitOpenSince = default;
		}
	}

	/// <summary>
	///     Records a failed request — increments the consecutive failure counter.
	///     When the threshold is reached, the circuit breaker opens.
	/// </summary>
	private void RecordFailure()
	{
		var failures = Interlocked.Increment(ref this._consecutiveFailures);

		if (this._config.CircuitBreakerThreshold > 0 && failures >= this._config.CircuitBreakerThreshold)
		{
			this._circuitOpenSince = DateTimeOffset.UtcNow;

			if (failures == this._config.CircuitBreakerThreshold)
				this._logger.LogWarning(LoggerEvents.RestError, "Circuit breaker opened for {Bucket} after {Failures} consecutive failures", this._bucket, failures);
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (this._disposed)
			return;

		this._disposed = true;
		this._cts.Cancel();
		this._queue.Writer.TryComplete();

		// Drain remaining queued requests and fail them explicitly — no silent loss
		while (this._queue.Reader.TryRead(out var request))
		{
			request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Bucket worker was disposed with pending requests."));
			request.CancellationTokenSource.Dispose();
		}

		this._cts.Dispose();
	}
}
