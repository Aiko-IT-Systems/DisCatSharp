using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Websocket;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink track started event.
/// </summary>
public sealed class LavalinkTrackStartedEventArgs : DiscordEventArgs
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
	/// Gets the guild id.
	/// </summary>
	public ulong GuildId { get; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild
		=> this.Discord.GuildsInternal.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkTrackStartedEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="eventArgs">The event args.</param>
	internal LavalinkTrackStartedEventArgs(DiscordClient client, TrackStartEvent eventArgs)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.Track = eventArgs.Track;
		this.GuildId = Convert.ToUInt64(eventArgs.GuildId);
	}
}
