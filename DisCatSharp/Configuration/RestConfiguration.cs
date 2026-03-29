using System;
using System.Net;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord REST client settings.
/// </summary>
public sealed class RestConfiguration
{
	/// <summary>
	///     Creates a new REST configuration with default values.
	/// </summary>
	public RestConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another REST configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public RestConfiguration(RestConfiguration other)
	{
		this.RequestTimeout = other.RequestTimeout;
		this.UseRelativeRatelimit = other.UseRelativeRatelimit;
		this.Proxy = other.Proxy;
		this.Advanced = new(other.Advanced);
	}

	/// <summary>
	///     <para>Sets the timeout for HTTP requests.</para>
	///     <para>Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan" /> to disable timeouts.</para>
	///     <para>Defaults to 20 seconds.</para>
	/// </summary>
	public TimeSpan RequestTimeout { internal get; set; } = TimeSpan.FromSeconds(20);

	/// <summary>
	///     <para>
	///         Sets whether to rely on Discord for NTP (Network Time Protocol) synchronization with the
	///         "X-Ratelimit-Reset-After" header.
	///     </para>
	///     <para>
	///         If the system clock is unsynced, setting this to true will ensure ratelimits are synced with Discord and
	///         reduce the risk of hitting one.
	///     </para>
	///     <para>This should only be set to false if the system clock is synced with NTP.</para>
	///     <para>Defaults to <see langword="true" />.</para>
	/// </summary>
	public bool UseRelativeRatelimit { internal get; set; } = true;

	/// <summary>
	///     <para>Sets the proxy to use for HTTP and WebSocket connections to Discord.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public IWebProxy? Proxy { internal get; set; } = null;

	/// <summary>
	///     <para>Advanced REST tuning options.</para>
	///     <para>Most applications should not need to modify these settings.</para>
	/// </summary>
	public RestAdvancedConfiguration Advanced { internal get; set; } = new();
}
