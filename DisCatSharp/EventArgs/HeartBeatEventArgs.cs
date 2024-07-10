using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.Heartbeated"/> event.
/// </summary>
public class HeartbeatEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the round-trip time of the heartbeat.
	/// </summary>
	public int Ping { get; internal set; }

	/// <summary>
	/// Gets the timestamp of the heartbeat.
	/// </summary>
	public DateTimeOffset Timestamp { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="HeartbeatEventArgs"/> class.
	/// </summary>
	internal HeartbeatEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
