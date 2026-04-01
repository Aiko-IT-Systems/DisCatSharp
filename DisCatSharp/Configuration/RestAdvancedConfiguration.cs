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
	}

	/// <summary>
	///     Gets or sets the maximum time a request may wait in a bucket queue before being failed
	///     with a <see cref="Exceptions.RestQueueTimeoutException" />.
	/// </summary>
	/// <remarks>
	///     <para>Set to <see cref="TimeSpan.Zero" /> to disable the timeout entirely (wait forever).</para>
	///     <para>The default is 5 minutes, which covers normal rate-limit back-off while catching stuck queues.</para>
	/// </remarks>
	public TimeSpan QueueTimeout { get; set; } = TimeSpan.FromMinutes(5);

	/// <summary>
	///     Gets or sets the duration after which a warning log is emitted for a queued request that has not yet started.
	/// </summary>
	/// <remarks>
	///     <para>Set to <see cref="TimeSpan.Zero" /> to disable the warning.</para>
	///     <para>Defaults to 2 minutes. Must be less than <see cref="QueueTimeout" /> when both are non-zero.</para>
	/// </remarks>
	public TimeSpan QueueWarningThreshold { get; set; } = TimeSpan.FromMinutes(2);

	/// <summary>
	///     Gets or sets the maximum number of automatic retries for a request that receives a retryable response (e.g. 429, 502).
	/// </summary>
	/// <remarks>
	///     Defaults to 5. Set to 0 to disable retries entirely.
	/// </remarks>
	public int MaxRetries { get; set; } = 5;
}
