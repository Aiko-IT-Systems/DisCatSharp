// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Globalization;

using DisCatSharp.EventArgs;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The lavalink voice server update.
/// </summary>
internal sealed class LavalinkVoiceServerUpdate
{
	/// <summary>
	/// Gets the token.
	/// </summary>
	[JsonProperty("token")]
	public string Token { get; }

	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public string GuildId { get; }

	/// <summary>
	/// Gets the endpoint.
	/// </summary>
	[JsonProperty("endpoint")]
	public string Endpoint { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkVoiceServerUpdate"/> class.
	/// </summary>
	/// <param name="vsu">The vsu.</param>
	internal LavalinkVoiceServerUpdate(VoiceServerUpdateEventArgs vsu)
	{
		this.Token = vsu.VoiceToken;
		this.GuildId = vsu.Guild.Id.ToString(CultureInfo.InvariantCulture);
		this.Endpoint = vsu.Endpoint;
	}
}

/// <summary>
/// The lavalink voice update.
/// </summary>
internal sealed class LavalinkVoiceUpdate : LavalinkPayload
{
	/// <summary>
	/// Gets the session id.
	/// </summary>
	[JsonProperty("sessionId")]
	public string SessionId { get; }

	/// <summary>
	/// Gets the event.
	/// </summary>
	[JsonProperty("event")]
	internal LavalinkVoiceServerUpdate Event { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkVoiceUpdate"/> class.
	/// </summary>
	/// <param name="vstu">The vstu.</param>
	/// <param name="vsrvu">The vsrvu.</param>
	public LavalinkVoiceUpdate(VoiceStateUpdateEventArgs vstu, VoiceServerUpdateEventArgs vsrvu)
		: base("voiceUpdate", vstu.Guild.Id.ToString(CultureInfo.InvariantCulture))
	{
		this.SessionId = vstu.SessionId;
		this.Event = new LavalinkVoiceServerUpdate(vsrvu);
	}
}
