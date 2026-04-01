using System;
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

	internal BucketWorker(RestClient client, RateLimitBucket bucket, RestAdvancedConfiguration config, ILogger logger)
	{
		this._client = client;
		this._bucket = bucket;
		this._config = config;
		this._logger = logger;
		this._cts = new();
		this._queue = Channel.CreateUnbounded<BaseRestRequest>(new()
		{
			SingleReader = true
		});
	}

	/// <summary>
	///     Gets the number of requests currently queued.
	/// </summary>
	internal int QueueLength => this._queue.Reader.Count;

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
	///     Enqueues a request into this worker's FIFO queue.
	///     Starts the worker loop if it is not already running.
	/// </summary>
	internal void Enqueue(BaseRestRequest request)
	{
		if (this._disposed || !this._queue.Writer.TryWrite(request))
		{
			request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Bucket worker has been disposed."));
			return;
		}

		this.EnsureRunning();
	}

	/// <summary>
	///     Starts the worker loop if it is not already running.
	/// </summary>
	private void EnsureRunning()
	{
		if (this._loopTask is null or { IsCompleted: true })
			lock (this._startLock)
				if (this._loopTask is null or { IsCompleted: true })
					this._loopTask = Task.Run(() => this.RunAsync(this._cts.Token));
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
						this._logger.LogError(LoggerEvents.RestError, ex, "Request to {Url} timed out in queue after {Duration:F1}s", request.Url.AbsoluteUri, waited.TotalSeconds);
						request.TrySetFaulted(ex);
						continue;
					}
				}

				await this.ExecuteAsync(request, ct);
			}
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// Normal disposal — fall through
		}
		catch (Exception ex)
		{
			this._logger.LogError(LoggerEvents.RestError, ex, "Bucket worker crashed for {Bucket}", this._bucket);
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
					this._logger.LogWarning(LoggerEvents.RestError, "Request to {Url} has been waiting {Duration:F1}s in queue (bucket: {Bucket}, queue depth: {QueueLength})", request.Url.AbsoluteUri, waited.TotalSeconds, this._bucket, this.QueueLength);
				}
			}

			try
			{
				// 1. Wait for the global rate limit gate to open
				await this._client.WaitForGlobalGateAsync();

				// 2. Per-bucket preemptive rate limiting (skip for unprobed buckets)
				if (this._bucket.LimitValid)
				{
					var now = DateTimeOffset.UtcNow;
					await this._bucket.TryResetLimitAsync(now);

					if (Interlocked.Decrement(ref this._bucket.RemainingInternal) < 0)
					{
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
							await Task.Delay(delay, ct);
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
				var result = await this._client.SendAndParseAsync(request, this._bucket, isProbe);

				// 4. Handle retry on 429
				if (result.ShouldRetry && retries < maxRetries)
				{
					retries++;
					Interlocked.Increment(ref this.Retried);

					if (result.IsGlobalRateLimit)
					{
						this._logger.LogError(LoggerEvents.RatelimitHit, "Global ratelimit hit, cooling down for {Url}", request.Url.AbsoluteUri);
						await this._client.EnforceGlobalRateLimitAsync(result.RetryDelay);
					}
					else
					{
						this._logger.LogError(LoggerEvents.RatelimitHit, "Ratelimit hit, retrying request to {Url}", request.Url.AbsoluteUri);
						await this._client.RaiseRateLimitHitAsync(request, result.Error as RateLimitException);

						if (result.RetryDelay > TimeSpan.Zero)
							await Task.Delay(result.RetryDelay, ct);
					}

					continue; // Retry
				}

				// 5. Complete or fault the request
				if (result.Error is not null)
				{
					this._client.ReportDiagnostics(request, result.Response, result.Error);
					request.SetFaulted(result.Error);
				}
				else
				{
					request.SetCompleted(result.Response);
				}

				Interlocked.Increment(ref this.Processed);
				return;
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
				request.TrySetFaulted(new OperationCanceledException("Bucket worker was disposed during request processing."));
				throw;
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
			request.TrySetFaulted(new ObjectDisposedException(nameof(BucketWorker), "Bucket worker was disposed with pending requests."));

		this._cts.Dispose();
	}
}
