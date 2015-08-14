using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities.Websocket;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink websocket closed events.
/// </summary>
public sealed class LavalinkWebsocketClosedEventArgs : SocketCloseEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

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
	/// Whether the websocket was closed by discord.
	/// </summary>
	public bool ByRemote { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkWebsocketClosedEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="eventArgs">The event args.</param>
	internal LavalinkWebsocketClosedEventArgs(DiscordClient client, WebSocketClosedEvent eventArgs)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.GuildId = Convert.ToUInt64(eventArgs.GuildId);
		this.ByRemote = eventArgs.ByRemote;
		this.CloseCode = eventArgs.Code;
		this.CloseMessage = eventArgs.Reason;
	}
}
