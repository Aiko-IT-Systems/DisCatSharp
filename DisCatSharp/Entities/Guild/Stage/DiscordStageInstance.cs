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

using System;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Stage instance.
/// </summary>
public class DiscordStageInstance : SnowflakeObject, IEquatable<DiscordStageInstance>
{
	/// <summary>
	/// Gets the guild id of the associated Stage channel.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the guild to which this channel belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

	/// <summary>
	/// Gets id of the associated Stage channel.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the topic of the Stage instance.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; internal set; }

	/// <summary>
	/// Gets the topic of the Stage instance.
	/// </summary>
	[JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Will be static due to the discovery removal."), Obsolete]
	public StagePrivacyLevel PrivacyLevel { get; internal set; } = StagePrivacyLevel.GuildOnly;

	/// <summary>
	/// Gets whether or not stage discovery is disabled.
	/// </summary>
	[JsonProperty("discoverable_disabled", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Will be removed due to the discovery removal."), Obsolete]
	public bool DiscoverableDisabled { get; internal set; }

	/// <summary>
	/// Checks whether this <see cref="DiscordStageInstance"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordStageInstance"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordStageInstance);

	/// <summary>
	/// Checks whether this <see cref="DiscordStageInstance"/> is equal to another <see cref="DiscordStageInstance"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordStageInstance"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordStageInstance"/> is equal to this <see cref="DiscordStageInstance"/>.</returns>
	public bool Equals(DiscordStageInstance e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordStageInstance"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordStageInstance"/>.</returns>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordStageInstance"/> objects are equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are equal.</returns>
	public static bool operator ==(DiscordStageInstance e1, DiscordStageInstance e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordStageInstance"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First channel to compare.</param>
	/// <param name="e2">Second channel to compare.</param>
	/// <returns>Whether the two channels are not equal.</returns>
	public static bool operator !=(DiscordStageInstance e1, DiscordStageInstance e2)
		=> !(e1 == e2);
}
