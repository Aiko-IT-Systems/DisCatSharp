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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the guild preview.
/// </summary>
public class DiscordGuildPreview : SnowflakeObject
{
	/// <summary>
	/// Gets the guild name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the guild icon's hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string IconHash { get; internal set; }

	/// <summary>
	/// Gets the guild icon's url.
	/// </summary>
	[JsonIgnore]
	public string IconUrl
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.{(this.IconHash.StartsWith("a_") ? "gif" : "png")}?size=1024" : null;

	/// <summary>
	/// Gets the guild splash's hash.
	/// </summary>
	[JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
	public string SplashHash { get; internal set; }

	/// <summary>
	/// Gets the guild splash's url.
	/// </summary>
	[JsonIgnore]
	public string SplashUrl
	 => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SPLASHES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.png?size=1024" : null;

	/// <summary>
	/// Gets the guild discovery splash's hash.
	/// </summary>
	[JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
	public string DiscoverySplashHash { get; internal set; }

	/// <summary>
	/// Gets the guild discovery splash's url.
	/// </summary>
	[JsonIgnore]
	public string DiscoverySplashUrl
		=> !string.IsNullOrWhiteSpace(this.DiscoverySplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILD_DISCOVERY_SPLASHES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.DiscoverySplashHash}.png?size=1024" : null;

	/// <summary>
	/// Gets a collection of this guild's emojis.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this.EmojisInternal);

	[JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
	[JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordEmoji> EmojisInternal;

	/// <summary>
	/// Gets a collection of this guild's stickers.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordSticker> Stickers => new ReadOnlyConcurrentDictionary<ulong, DiscordSticker>(this.StickersInternal);

	[JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
	[JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordSticker> StickersInternal;


	/// <summary>
	/// Gets a collection of this guild's features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Features { get; internal set; }

	/// <summary>
	/// Gets the approximate member count.
	/// </summary>
	[JsonProperty("approximate_member_count")]
	public int ApproximateMemberCount { get; internal set; }

	/// <summary>
	/// Gets the approximate presence count.
	/// </summary>
	[JsonProperty("approximate_presence_count")]
	public int ApproximatePresenceCount { get; internal set; }

	/// <summary>
	/// Gets the description for the guild, if the guild is discoverable.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the system channel flags for the guild.
	/// </summary>
	[JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
	public SystemChannelFlags SystemChannelFlags { get; internal set; }

	/// <summary>
	/// Gets this hub type for the guild, if the guild is a hub.
	/// </summary>
	[JsonProperty("hub_type", NullValueHandling = NullValueHandling.Ignore)]
	public HubType HubType { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildPreview"/> class.
	/// </summary>
	internal DiscordGuildPreview()
	{ }
}
