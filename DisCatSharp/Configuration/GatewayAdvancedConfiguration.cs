using System;

using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
///     Advanced configuration for Discord gateway transport tuning.
/// </summary>
/// <remarks>
///     Properties in this class control low-level transport behavior and should rarely need adjustment.
/// </remarks>
public sealed class GatewayAdvancedConfiguration
{
	/// <summary>
	///     Creates a new gateway advanced configuration with default values.
	/// </summary>
	public GatewayAdvancedConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another gateway advanced configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public GatewayAdvancedConfiguration(GatewayAdvancedConfiguration other)
	{
		this.DispatchMode = other.DispatchMode;
		this.DispatchQueueCapacity = other.DispatchQueueCapacity;
		this.SocketLockTimeout = other.SocketLockTimeout;
		this.ReconnectDelay = other.ReconnectDelay;
		this.HeartbeatZombieThreshold = other.HeartbeatZombieThreshold;
	}

	/// <summary>
	///     <para>Controls how user event handlers are dispatched after ordered internal processing.</para>
	///     <para>
	///         Internal cache and state mutations are always processed in FIFO order per shard, regardless of this setting.
	///         This only affects when and how user-facing event handlers are invoked.
	///     </para>
	///     <para>Defaults to <see cref="GatewayDispatchMode.ConcurrentHandlers" />.</para>
	/// </summary>
	public GatewayDispatchMode DispatchMode { internal get; set; } = GatewayDispatchMode.ConcurrentHandlers;

	/// <summary>
	///     <para>Sets the bounded capacity of the internal dispatch queue per shard.</para>
	///     <para>
	///         When the queue is full, back-pressure will be applied to the WebSocket reader, which may cause
	///         the gateway to close the connection if heartbeat ACKs are delayed. Increase this value if your
	///         bot processes events slowly and receives high event volume.
	///     </para>
	///     <para>Defaults to 10,000. Set to 0 for an unbounded queue.</para>
	/// </summary>
	public int DispatchQueueCapacity
	{
		internal get;
		set
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value, "DispatchQueueCapacity must be 0 (unbounded) or a positive integer.");

			field = value;
		}
	} = 10_000;

	/// <summary>
	///     <para>Maximum time the socket lock will wait before auto-unlocking after an IDENTIFY payload is sent.</para>
	///     <para>
	///         If the gateway does not respond to an IDENTIFY within this duration, the lock is released so that
	///         other shards can proceed. Increase this value on very slow connections.
	///     </para>
	///     <para>Must be positive. Defaults to 30 seconds.</para>
	/// </summary>
	public TimeSpan SocketLockTimeout
	{
		internal get;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "SocketLockTimeout must be positive.");

			field = value;
		}
	} = TimeSpan.FromSeconds(30);

	/// <summary>
	///     <para>Delay before sending a RESUME after receiving an INVALID_SESSION with <c>d: true</c>.</para>
	///     <para>
	///         Discord recommends waiting a few seconds before resuming a session to avoid rate-limit issues.
	///         Adjust this if you experience frequent resume failures or want faster reconnection.
	///     </para>
	///     <para>Must be positive. Defaults to 6 seconds.</para>
	/// </summary>
	public TimeSpan ReconnectDelay
	{
		internal get;
		set
		{
			if (value <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(value), value, "ReconnectDelay must be positive.");

			field = value;
		}
	} = TimeSpan.FromSeconds(6);

	/// <summary>
	///     <para>Number of consecutive missed heartbeat ACKs before the connection is declared a zombie.</para>
	///     <para>
	///         When the threshold is exceeded after guild download has completed, the client will trigger a
	///         <see cref="DiscordClient.Zombied" /> event and force a reconnect. During guild download the event
	///         fires as a warning only.
	///     </para>
	///     <para>Defaults to 5.</para>
	/// </summary>
	public int HeartbeatZombieThreshold
	{
		internal get;
		set
		{
			if (value < 1)
				throw new ArgumentOutOfRangeException(nameof(value), value, "HeartbeatZombieThreshold must be at least 1.");

			field = value;
		}
	} = 5;
}
