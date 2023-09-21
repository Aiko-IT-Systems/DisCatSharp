using System;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents arguments for player update event.
/// </summary>
public sealed class PlayerUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the timestamp at which this event was emitted.
	/// </summary>
	public DateTimeOffset Timestamp { get; }

	/// <summary>
	/// Gets the position in the playback stream.
	/// </summary>
	public TimeSpan Position { get; }

	/// <summary>
	/// Gets the player that emitted this event.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Gets whether the player is connected to the voice gateway.
	/// </summary>
	public bool IsConnected { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PlayerUpdateEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="timestamp">The timestamp.</param>
	/// <param name="position">The position.</param>
	/// <param name="connected">Whether the player is connected.</param>
	internal PlayerUpdateEventArgs(LavalinkGuildConnection lvl, DateTimeOffset timestamp, TimeSpan position, bool connected)
		: base(lvl.Node.Discord.ServiceProvider)
	{
		this.Player = lvl;
		this.Timestamp = timestamp;
		this.Position = position;
		this.IsConnected = connected;
	}
}
