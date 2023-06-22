// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
