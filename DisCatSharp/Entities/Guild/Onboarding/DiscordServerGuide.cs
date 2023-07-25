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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild's server guide.
/// </summary>
public sealed class DiscordServerGuide : ObservableApiObject
{
	/// <summary>
	/// Gets the guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Gets whether the server guide is enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	/// <summary>
	/// Gets the welcome message.
	/// </summary>
	[JsonProperty("welcome_message", NullValueHandling = NullValueHandling.Ignore)]
	public WelcomeMessage WelcomeMessage { get; internal set; }

	/// <summary>
	/// Gets the new member actions.
	/// </summary>
	[JsonProperty("new_member_actions", NullValueHandling = NullValueHandling.Ignore)]
	public List<NewMemberAction> NewMemberActions { get; internal set; } = new();

	/// <summary>
	/// Gets the resource channels.
	/// </summary>
	[JsonProperty("resource_channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<ResourceChannel> ResourceChannels { get; internal set; } = new();
}
