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

using DisCatSharp.Lavalink.Entities.Filters;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink player.
/// </summary>
public sealed class LavalinkPlayer
{
	/// <summary>
	/// Gets the guild id this player belongs to.
	/// </summary>
	[JsonProperty("guildId")]
	public string GuildId { get; internal set; }

	/// <summary>
	/// Gets the currently loaded track.
	/// </summary>
	[JsonProperty("track", NullValueHandling = NullValueHandling.Include)]
	public LavalinkTrack? Track { get; internal set; }

	/// <summary>
	/// Gets the volume of this player.
	/// </summary>
	[JsonProperty("volume")]
	public int Volume { get; internal set; }

	/// <summary>
	/// Gets whether this player is paused.
	/// </summary>
	[JsonProperty("paused")]
	public bool Paused { get; internal set; }

	/// <summary>
	/// Gets the player state.
	/// </summary>
	[JsonProperty("state")]
	public LavalinkPlayerState PlayerState { get; internal set; }

	/// <summary>
	/// Gets the player voice state.
	/// </summary>
	[JsonProperty("voice")]
	internal LavalinkVoiceState VoiceState { get; set; }

	/// <summary>
	/// Gets the player filters.
	/// </summary>
	[JsonProperty("filters")]
	public LavalinkFilters Filters { get; internal set; }
}
