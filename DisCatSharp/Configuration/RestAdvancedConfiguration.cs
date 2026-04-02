using System;

namespace DisCatSharp;

/// <summary>
///     Advanced configuration for Discord REST client tuning.
/// </summary>
/// <remarks>
///     Properties in this class control low-level REST behavior such as queue timeouts, retry limits,
///     and rate-limit buffers. Additional knobs will be added here in later phases.
/// </remarks>
public sealed class RestAdvancedConfiguration
{
	/// <summary>
	///     Creates a new REST advanced configuration with default values.
	/// </summary>
	public RestAdvancedConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another REST advanced configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public RestAdvancedConfiguration(RestAdvancedConfiguration other)
	{
		this.QueueTimeout = other.QueueTimeout;
		this.QueueWarningThreshold = other.QueueWarningThreshold;
		this.MaxRetries = other.MaxRetries;
		this.MaxQueueDepthPerBucket = other.MaxQueueDepthPerBucket;
		this.RetryTransientErrors = other.RetryTransientErrors;
		this.CircuitBreakerThreshold = other.CircuitBreakerThreshold;
		this.CircuitBreakerResetTimeout = other.CircuitBreakerResetTimeout;
		this.FailFastMode = other.FailFastMode;
	}

	/// <summary>
	///     Gets or sets the maximum time a request may wait in a bucket queue before being failed
	///     with a <see cref="Exceptions.RestQueueTimeoutException" />.
	/// </summary>
	/// <remarks>
	///     <para>Set to <see cref="TimeSpan.Zero" /> to disable the timeout entirely (wait forever).</para>
	///     <para>Must be non-negative. The default is 5 minutes.</para>
	/// </remarks>
	public TimeSpan QueueTimeout
	{
		get;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "QueueTimeout must be non-negative. Use TimeSpan.Zero to disable.");

			field = value;
		}
	} = TimeSpan.FromMinutes(5);

	/// <summary>
	///     Gets or sets the duration after which a warning log is emitted for a queued request that has not yet started.
	/// </summary>
	/// <remarks>
	///     <para>Set to <see cref="TimeSpan.Zero" /> to disable the warning.</para>
	///     <para>Must be non-negative. Defaults to 2 minutes.</para>
	/// </remarks>
	public TimeSpan QueueWarningThreshold
	{
		get;
		set
		{
			if (value < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "QueueWarningThreshold must be non-negative. Use TimeSpan.Zero to disable.");

			field = value;
		}
	} = TimeSpan.FromMinutes(2);

	/// <summary>
	///     Gets or sets the maximum number of automatic retries for a request that receives a retryable response.
	///     Applies to rate-limit responses (429) and transient server errors (500, 502, 503, 504).
	///     Server errors use exponential backoff (1s, 2s, 4s, …); rate limits use the server-provided Retry-After.
	/// </summary>
	/// <remarks>
	///     Must be non-negative. Defaults to 5. Set to 0 to disable retries entirely.
	/// </remarks>
	public int MaxRetries
	{
		get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MaxRetries must be non-negative.");

			field = value;
		}
	} = 5;

	/// <summary>
	///     Gets or sets the maximum number of requests that can be queued per bucket.
	///     When the limit is reached, new requests are immediately failed with
	///     <see cref="Exceptions.RestQueueFullException" />.
	/// </summary>
	/// <remarks>
	///     <para>Must be non-negative. Set to 0 to disable the limit (unbounded queue). Defaults to 1000.</para>
	///     <para>This protects against OOM from runaway request patterns on a single endpoint.</para>
	/// </remarks>
	public int MaxQueueDepthPerBucket
	{
		get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "MaxQueueDepthPerBucket must be non-negative. Use 0 for unbounded.");

			field = value;
		}
	} = 1000;

	/// <summary>
	///     Gets or sets whether transient HTTP errors (DNS failures, socket errors, connection timeouts)
	///     should be retried with exponential backoff, up to <see cref="MaxRetries" />.
	/// </summary>
	/// <remarks>
	///     <para>Defaults to <see langword="true" />. Set to <see langword="false" /> to fail immediately on network errors.</para>
	///     <para>Only transient errors are retried — permanent errors (e.g. 404, 403) always fail immediately.</para>
	/// </remarks>
	public bool RetryTransientErrors { get; set; } = true;

	/// <summary>
	///     Gets or sets the number of consecutive failures on a single bucket before the circuit breaker opens.
	///     When open, new requests to that bucket are immediately failed without sending HTTP requests.
	/// </summary>
	/// <remarks>
	///     <para>Must be non-negative. Set to 0 to disable the circuit breaker. Defaults to 10.</para>
	///     <para>The circuit breaker resets after <see cref="CircuitBreakerResetTimeout" /> with no new failures.</para>
	/// </remarks>
	public int CircuitBreakerThreshold
	{
		get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "CircuitBreakerThreshold must be non-negative. Use 0 to disable.");

			field = value;
		}
	} = 10;

	/// <summary>
	///     Gets or sets the duration after which a tripped circuit breaker transitions to half-open,
	///     allowing a single probe request through.
	/// </summary>
	/// <remarks>
	///     Must be positive. Defaults to 30 seconds.
	/// </remarks>
	public TimeSpan CircuitBreakerResetTimeout
	{
		get;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "CircuitBreakerResetTimeout must be positive.");

			field = value;
		}
	} = TimeSpan.FromSeconds(30);

	/// <summary>
	///     Enables fail-fast mode: disables all retries and circuit breaker cooldown, and sets a short queue timeout.
	///     When enabled, any REST failure is surfaced immediately without automatic recovery attempts.
	/// </summary>
	/// <remarks>
	///     <para>This overrides <see cref="MaxRetries" />, <see cref="RetryTransientErrors" />,
	///     <see cref="CircuitBreakerThreshold" />, and <see cref="QueueTimeout" /> when set to <see langword="true" />.</para>
	///     <para>Intended for library developer diagnostics. Not part of the public API surface.</para>
	/// </remarks>
	internal bool FailFastMode
	{
		get;
		set
		{
			field = value;

			if (!value)
				return;

			this.MaxRetries = 0;
			this.RetryTransientErrors = false;
			this.CircuitBreakerThreshold = 0;
			this.QueueTimeout = TimeSpan.FromSeconds(10);
		}
	}
}
