using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Websocket;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink track exceptions.
/// </summary>
public sealed class LavalinkTrackExceptionEventArgs : DiscordEventArgs
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
	/// Gets the exception.
	/// </summary>
	public LavalinkException Exception { get; }

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
	/// Initializes a new instance of the <see cref="LavalinkTrackExceptionEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="eventArgs">The event args.</param>
	internal LavalinkTrackExceptionEventArgs(DiscordClient client, TrackExceptionEvent eventArgs)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.Track = eventArgs.Track;
		this.Exception = new()
		{
			Message = eventArgs.Exception.Message,
			Severity = eventArgs.Exception.Severity,
			Cause = eventArgs.Exception.Cause
		};
		this.GuildId = Convert.ToUInt64(eventArgs.GuildId);
	}
}
