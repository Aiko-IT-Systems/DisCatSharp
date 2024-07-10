using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Websocket;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink track stuck events.
/// </summary>
public sealed class LavalinkTrackStuckEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the related lavalink track.
	/// </summary>
	public LavalinkTrack Track { get; }

	/// <summary>
	/// Gets the threshold in milliseconds that was exceeded as <see cref="TimeSpan"/>.
	/// </summary>
	public TimeSpan ThresholdMs => TimeSpan.FromMilliseconds(this.ThresholdMilliseconds);

	/// <summary>
	/// Gets the threshold in milliseconds that was exceeded.
	/// </summary>
	internal long ThresholdMilliseconds { get; set; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild
		=> this.Discord.GuildsInternal.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkTrackStuckEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="eventArgs">The event args.</param>
	internal LavalinkTrackStuckEventArgs(DiscordClient client, TrackStuckEvent eventArgs)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.Track = eventArgs.Track;
		this.ThresholdMilliseconds = eventArgs.ThresholdMs;
		this.GuildId = Convert.ToUInt64(eventArgs.GuildId);
	}
}
