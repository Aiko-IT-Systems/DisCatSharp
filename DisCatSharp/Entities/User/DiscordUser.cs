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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord user.
/// </summary>
public class DiscordUser : SnowflakeObject, IEquatable<DiscordUser>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordUser"/> class.
	/// </summary>
	internal DiscordUser()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordUser"/> class.
	/// </summary>
	/// <param name="transport">The transport user.</param>
	internal DiscordUser(TransportUser transport)
	{
		this.Id = transport.Id;
		this.Username = transport.Username;
		this.Discriminator = transport.Discriminator;
		this.AvatarHash = transport.AvatarHash;
		this.BannerHash = transport.BannerHash;
		this.BannerColorInternal = transport.BannerColor;
		this.IsBot = transport.IsBot;
		this.MfaEnabled = transport.MfaEnabled;
		this.Verified = transport.Verified;
		this.Email = transport.Email;
		this.PremiumType = transport.PremiumType;
		this.Locale = transport.Locale;
		this.Flags = transport.Flags;
		this.OAuthFlags = transport.OAuthFlags;
		this.Bio = transport.Bio;
		this.Pronouns = transport.Pronouns;
	}

	/// <summary>
	/// Gets this user's username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Username { get; internal set; }

	/// <summary>
	/// Gets this user's username with the discriminator.
	/// Example: Discord#0000
	/// </summary>
	[JsonIgnore]
	public virtual string UsernameWithDiscriminator
		=> $"{this.Username}#{this.Discriminator}";

	/// <summary>
	/// Gets the user's 4-digit discriminator.
	/// </summary>
	[JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Discriminator { get; internal set; }

	/// <summary>
	/// Gets the discriminator integer.
	/// </summary>
	[JsonIgnore]
	internal int DiscriminatorInt
		=> int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

	/// <summary>
	/// Gets the user's banner color, if set. Mutually exclusive with <see cref="BannerHash"/>.
	/// </summary>
	public virtual DiscordColor? BannerColor
		=> !this.BannerColorInternal.HasValue ? null : new DiscordColor(this.BannerColorInternal.Value);

	/// <summary>
	/// Gets the user's banner color integer.
	/// </summary>
	[JsonProperty("accent_color")]
	internal int? BannerColorInternal;

	/// <summary>
	/// Gets the user's banner url
	/// </summary>
	[JsonIgnore]
	public string BannerUrl
		=> string.IsNullOrWhiteSpace(this.BannerHash) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_") ? "gif" : "png")}?size=4096";

	/// <summary>
	/// Gets the user's profile banner hash. Mutually exclusive with <see cref="BannerColor"/>.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string BannerHash { get; internal set; }

	/// <summary>
	/// Gets the users bio.
	/// This is not available to bots tho.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Bio { get; internal set; }

	/// <summary>
	/// Gets the user's avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string AvatarHash { get; internal set; }

	/// <summary>
	/// Returns a uri to this users profile.
	/// </summary>
	[JsonIgnore]
	public Uri ProfileUri => new($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.USERS}/{this.Id}");

	/// <summary>
	/// Returns a string representing the direct URL to this users profile.
	/// </summary>
	/// <returns>The URL of this users profile.</returns>
	[JsonIgnore]
	public string ProfileUrl => this.ProfileUri.AbsoluteUri;

	/// <summary>
	/// Gets the user's avatar URL.s
	/// </summary>
	[JsonIgnore]
	public string AvatarUrl
		=> string.IsNullOrWhiteSpace(this.AvatarHash) ? this.DefaultAvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.{(this.AvatarHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

	/// <summary>
	/// Gets the URL of default avatar for this user.
	/// </summary>
	[JsonIgnore]
	public string DefaultAvatarUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMBED}{Endpoints.AVATARS}/{(this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=1024";

	/// <summary>
	/// Gets whether the user is a bot.
	/// </summary>
	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool IsBot { get; internal set; }

	/// <summary>
	/// Gets whether the user has multi-factor authentication enabled.
	/// </summary>
	[JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool? MfaEnabled { get; internal set; }

	/// <summary>
	/// Gets whether the user is an official Discord system user.
	/// </summary>
	[JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsSystem { get; internal set; }

	/// <summary>
	/// Gets whether the user is verified.
	/// <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool? Verified { get; internal set; }

	/// <summary>
	/// Gets the user's email address.
	/// <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Email { get; internal set; }

	/// <summary>
	/// Gets the user's premium type.
	/// </summary>
	[JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
	public virtual PremiumType? PremiumType { get; internal set; }

	/// <summary>
	/// Gets the user's chosen language
	/// </summary>
	[JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Locale { get; internal set; }

	/// <summary>
	/// Gets the user's flags for OAuth.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public virtual UserFlags? OAuthFlags { get; internal set; }

	/// <summary>
	/// Gets the user's flags.
	/// </summary>
	[JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
	public virtual UserFlags? Flags { get; internal set; }

	/// <summary>
	/// Gets the user's pronouns.
	/// </summary>
	[JsonProperty("pronouns", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Pronouns { get; internal set; }

	/// <summary>
	/// Gets the user's mention string.
	/// </summary>
	[JsonIgnore]
	public string Mention
		=> Formatter.Mention(this, this is DiscordMember);

	/// <summary>
	/// Gets whether this user is the Client which created this object.
	/// </summary>
	[JsonIgnore]
	public bool IsCurrent
		=> this.Id == this.Discord.CurrentUser.Id;

	#region Extension of DiscordUser

	/// <summary>
	/// Whether this member is a <see cref="UserFlags.CertifiedModerator"/>
	/// </summary>
	/// <returns><see cref="bool"/></returns>
	[JsonIgnore]
	public bool IsMod
			=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.CertifiedModerator);

	/// <summary>
	/// Whether this member is a <see cref="UserFlags.Partner"/>
	/// </summary>
	/// <returns><see cref="bool"/></returns>
	[JsonIgnore]
	public bool IsPartner
			=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.Partner);

	/// <summary>
	/// Whether this member is a <see cref="UserFlags.VerifiedBot"/>
	/// </summary>
	/// <returns><see cref="bool"/></returns>
	[JsonIgnore]
	public bool IsVerifiedBot
			=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.VerifiedBot);

	/// <summary>
	/// Whether this member is a <see cref="UserFlags.VerifiedDeveloper"/>
	/// </summary>
	/// <returns><see cref="bool"/></returns>
	[JsonIgnore]
	public bool IsBotDev
			=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.VerifiedDeveloper);

	/// <summary>
	/// Whether this member is a <see cref="UserFlags.Staff"/>
	/// </summary>
	/// <returns><see cref="bool"/></returns>
	[JsonIgnore]
	public bool IsStaff
			=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.Staff);

	#endregion

	/// <summary>
	/// Fetches the user from the API.
	/// </summary>
	/// <returns>The user with fresh data from the API.</returns>
	public async Task<DiscordUser> GetFromApiAsync()
		=> await this.Discord.ApiClient.GetUserAsync(this.Id);

	/// <summary>
	/// Whether this user is in a <see cref="DiscordGuild"/>
	/// </summary>
	/// <example>
	/// <code>
	/// DiscordGuild guild = await Client.GetGuildAsync(806675511555915806);
	/// DiscordUser user = await Client.GetUserAsync(469957180968271873);
	/// Console.WriteLine($"{user.Username} {(user.IsInGuild(guild) ? "is a" : "is not a")} member of {guild.Name}");
	/// </code>
	/// results to <c>J_M_Lutra is a member of Project Nyaw~</c>.
	/// </example>
	/// <param name="guild"><see cref="DiscordGuild"/></param>
	/// <returns><see cref="bool"/></returns>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
	public async Task<bool> IsInGuild(DiscordGuild guild)
	{
		try
		{
			var member = await guild.GetMemberAsync(this.Id);
			return member is not null;

		}
		catch (NotFoundException)
		{
			return false;
		}
	}

	/// <summary>
	/// Whether this user is not in a <see cref="DiscordGuild"/>
	/// </summary>
	/// <param name="guild"><see cref="DiscordGuild"/></param>
	/// <returns><see cref="bool"/></returns>
	public async Task<bool> IsNotInGuild(DiscordGuild guild)
		=> !await this.IsInGuild(guild);

	/// <summary>
	/// Returns the DiscordMember in the specified <see cref="DiscordGuild"/>
	/// </summary>
	/// <param name="guild">The <see cref="DiscordGuild"/> to get this user on.</param>
	/// <returns>The <see cref="DiscordMember"/>.</returns>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the user is not part of the guild.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMember> ConvertToMember(DiscordGuild guild)
		=> await guild.GetMemberAsync(this.Id);

	/// <summary>
	/// Unbans this user from a guild.
	/// </summary>
	/// <param name="guild">Guild to unban this user from.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
	/// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnbanAsync(DiscordGuild guild, string reason = null)
		=> guild.UnbanMemberAsync(this, reason);

	/// <summary>
	/// Gets this user's presence.
	/// </summary>
	[JsonIgnore]
	public DiscordPresence Presence
		=> this.Discord is DiscordClient dc && dc.Presences.TryGetValue(this.Id, out var presence) ? presence : null;

	/// <summary>
	/// Gets the user's avatar URL, in requested format and size.
	/// </summary>
	/// <param name="fmt">Format of the avatar to get.</param>
	/// <param name="size">Maximum size of the avatar. Must be a power of two, minimum 16, maximum 2048.</param>
	/// <returns>URL of the user's avatar.</returns>
	public string GetAvatarUrl(ImageFormat fmt, ushort size = 1024)
	{
		if (fmt == ImageFormat.Unknown)
			throw new ArgumentException("You must specify valid image format.", nameof(fmt));

		if (size < 16 || size > 2048)
			throw new ArgumentOutOfRangeException(nameof(size));

		var log = Math.Log(size, 2);
		if (log < 4 || log > 11 || log % 1 != 0)
			throw new ArgumentOutOfRangeException(nameof(size));

		var sfmt = "";
		sfmt = fmt switch
		{
			ImageFormat.Gif => "gif",
			ImageFormat.Jpeg => "jpg",
			ImageFormat.Png => "png",
			ImageFormat.WebP => "webp",
			ImageFormat.Auto => !string.IsNullOrWhiteSpace(this.AvatarHash) ? this.AvatarHash.StartsWith("a_") ? "gif" : "png" : "png",
			_ => throw new ArgumentOutOfRangeException(nameof(fmt)),
		};
		var ssize = size.ToString(CultureInfo.InvariantCulture);
		if (!string.IsNullOrWhiteSpace(this.AvatarHash))
		{
			var id = this.Id.ToString(CultureInfo.InvariantCulture);
			return $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{id}/{this.AvatarHash}.{sfmt}?size={ssize}";
		}
		else
		{
			var type = (this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture);
			return $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMBED}{Endpoints.AVATARS}/{type}.{sfmt}?size={ssize}";
		}
	}

	/// <summary>
	/// Returns a string representation of this user.
	/// </summary>
	/// <returns>String representation of this user.</returns>
	public override string ToString() => $"User {this.Id}; {this.Username}#{this.Discriminator}";

	/// <summary>
	/// Checks whether this <see cref="DiscordUser"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordUser"/>.</returns>
	public override bool Equals(object obj) => this.Equals(obj as DiscordUser);

	/// <summary>
	/// Checks whether this <see cref="DiscordUser"/> is equal to another <see cref="DiscordUser"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordUser"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordUser"/> is equal to this <see cref="DiscordUser"/>.</returns>
	public bool Equals(DiscordUser e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordUser"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordUser"/>.</returns>
	public override int GetHashCode() => this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordUser"/> objects are equal.
	/// </summary>
	/// <param name="e1">First user to compare.</param>
	/// <param name="e2">Second user to compare.</param>
	/// <returns>Whether the two users are equal.</returns>
	public static bool operator ==(DiscordUser e1, DiscordUser e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordUser"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First user to compare.</param>
	/// <param name="e2">Second user to compare.</param>
	/// <returns>Whether the two users are not equal.</returns>
	public static bool operator !=(DiscordUser e1, DiscordUser e2)
		=> !(e1 == e2);
}

/// <summary>
/// Represents a user comparer.
/// </summary>
internal class DiscordUserComparer : IEqualityComparer<DiscordUser>
{
	/// <summary>
	/// Whether the users are equal.
	/// </summary>
	/// <param name="x">The first user</param>
	/// <param name="y">The second user.</param>
	public bool Equals(DiscordUser x, DiscordUser y) => x.Equals(y);

	/// <summary>
	/// Gets the hash code.
	/// </summary>
	/// <param name="obj">The user.</param>
	public int GetHashCode(DiscordUser obj) => obj.Id.GetHashCode();
}
