// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a transport user.
/// </summary>
internal class TransportUser
{
	/// <summary>
	/// Gets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; internal set; }

	/// <summary>
	/// Gets the username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; internal set; }

	/// <summary>
	/// Gets or sets the discriminator.
	/// </summary>
	[JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
	internal string Discriminator { get; set; }

	/// <summary>
	/// Gets the username with discriminator.
	/// </summary>
	internal string UsernameWithDiscriminator
		=> $"{this.Username}#{this.Discriminator}";

	/// <summary>
	/// Gets the avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public string AvatarHash { get; internal set; }

	/// <summary>
	/// Gets the banner hash.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string BannerHash { get; internal set; }

	/// <summary>
	/// Gets the banner color.
	/// </summary>
	[JsonProperty("accent_color")]
	public int? BannerColor { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether is bot.
	/// </summary>
	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsBot { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether mfa enabled.
	/// </summary>
	[JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? MfaEnabled { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether verified.
	/// </summary>
	[JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Verified { get; internal set; }

	/// <summary>
	/// Gets the email.
	/// </summary>
	[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
	public string Email { get; internal set; }

	/// <summary>
	/// Gets the premium type.
	/// </summary>
	[JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
	public PremiumType? PremiumType { get; internal set; }

	/// <summary>
	/// Gets the locale.
	/// </summary>
	[JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
	public string Locale { get; internal set; }

	/// <summary>
	/// Gets the OAuth flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public UserFlags? OAuthFlags { get; internal set; }

	/// <summary>
	/// Gets the flags.
	/// </summary>
	[JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
	public UserFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets the users bio.
	/// This is not available to bots tho.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public string Bio { get; internal set; }

	/// <summary>
	/// Gets the users pronouns.
	/// </summary>
	[JsonProperty("pronouns", NullValueHandling = NullValueHandling.Ignore)]
	public string Pronouns { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportUser"/> class.
	/// </summary>
	internal TransportUser() { }

	/// <summary>
	/// Initializes a new instance of the <see cref="TransportUser"/> class from an existing <see cref="TransportUser"/>.
	/// </summary>
	/// <param name="other">The other transport user.</param>
	internal TransportUser(TransportUser other)
	{
		this.Id = other.Id;
		this.Username = other.Username;
		this.Discriminator = other.Discriminator;
		this.AvatarHash = other.AvatarHash;
		this.BannerHash = other.BannerHash;
		this.BannerColor = other.BannerColor;
		this.IsBot = other.IsBot;
		this.MfaEnabled = other.MfaEnabled;
		this.Verified = other.Verified;
		this.Email = other.Email;
		this.PremiumType = other.PremiumType;
		this.Locale = other.Locale;
		this.Flags = other.Flags;
		this.OAuthFlags = other.OAuthFlags;
		this.Bio = other.Bio;
		this.Pronouns = other.Pronouns;
	}
}
