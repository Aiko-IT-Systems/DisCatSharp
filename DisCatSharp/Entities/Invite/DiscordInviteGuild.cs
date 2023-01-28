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

using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild to which the user is invited.
/// </summary>
public class DiscordInviteGuild : SnowflakeObject
{
	/// <summary>
	/// Gets the name of the guild.
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
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.jpg" : null;

	/// <summary>
	/// Gets the hash of guild's invite splash.
	/// </summary>
	[JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
	internal string SplashHash { get; set; }

	/// <summary>
	/// Gets the URL of guild's invite splash.
	/// </summary>
	[JsonIgnore]
	public string SplashUrl
		=> !string.IsNullOrWhiteSpace(this.SplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SPLASHES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.jpg" : null;

	/// <summary>
	/// Gets the guild's banner hash, when applicable.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string Banner { get; internal set; }

	/// <summary>
	/// Gets the guild's banner in url form.
	/// </summary>
	[JsonIgnore]
	public string BannerUrl
		=> !string.IsNullOrWhiteSpace(this.Banner) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.Id}/{this.Banner}" : null;

	/// <summary>
	/// Gets the guild description, when applicable.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets a collection of this guild's features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Features { get; internal set; }

	/// <summary>
	/// Gets the guild's verification level.
	/// </summary>
	[JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
	public VerificationLevel VerificationLevel { get; internal set; }

	/// <summary>
	/// Gets vanity URL code for this guild, when applicable.
	/// </summary>
	[JsonProperty("vanity_url_code")]
	public string VanityUrlCode { get; internal set; }

	/// <summary>
	/// Gets the guild's welcome screen, when applicable.
	/// </summary>
	[JsonProperty("welcome_screen", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordGuildWelcomeScreen WelcomeScreen { get; internal set; }

	/// <summary>
	/// Gets the guild nsfw status.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsNsfw { get; internal set; }

	/// <summary>
	/// Gets the guild nsfw level.
	/// </summary>
	[JsonProperty("nsfw_level", NullValueHandling = NullValueHandling.Ignore)]
	public NsfwLevel NsfwLevel { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordInviteGuild"/> class.
	/// </summary>
	internal DiscordInviteGuild()
	{ }
}
