using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord user.
/// </summary>
public class DiscordUser : SnowflakeObject, IEquatable<DiscordUser>
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordUser" /> class.
	/// </summary>
	internal DiscordUser()
		: base(["display_name", "linked_users", "banner_color", "authenticator_types", "clan"])
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordUser" /> class.
	/// </summary>
	/// <param name="transport">The transport user.</param>
	internal DiscordUser(TransportUser transport)
	{
		this.Id = transport.Id;
		this.Username = transport.Username;
		this.Discriminator = transport.Discriminator;
		this.AvatarHash = transport.AvatarHash;
		this.AvatarDecorationData = transport.AvatarDecorationData;
		this.Collectibles = transport.Collectibles;
		this.BannerHash = transport.BannerHash;
		this.BannerColorInternal = transport.BannerColor;
		this.ThemeColorsInternal = [.. transport.ThemeColors ?? []];
		this.IsBot = transport.IsBot;
		this.IsSystem = transport.IsSystem;
		this.MfaEnabled = transport.MfaEnabled;
		this.Verified = transport.Verified;
		this.Email = transport.Email;
		this.PremiumType = transport.PremiumType;
		this.Locale = transport.Locale;
		this.Flags = transport.Flags;
		this.OAuthFlags = transport.OAuthFlags;
		this.Bio = transport.Bio;
		this.Pronouns = transport.Pronouns;
		this.GlobalName = transport.GlobalName;
		this.PrimaryGuild = transport.PrimaryGuild;
	}

	/// <summary>
	///     Gets the user's banner color integer.
	/// </summary>
	[JsonProperty("accent_color")]
	internal int? BannerColorInternal { get; set; }

	/// <summary>
	///     Gets the user's theme color integers.
	/// </summary>
	[JsonProperty("theme_colors", NullValueHandling = NullValueHandling.Ignore)]
	internal List<int>? ThemeColorsInternal { get; set; }

	/// <summary>
	///     Gets this user's username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Username { get; internal set; }

	/// <summary>
	///     Gets this user's username with the discriminator.
	///     Example: Discord#0000
	/// </summary>
	[JsonIgnore]
	public virtual string UsernameWithDiscriminator
		=> this.IsMigrated ? this.UsernameWithGlobalName : $"{this.Username}#{this.Discriminator}";

	/// <summary>
	///     Gets the username with the global name.
	///     Example: @lulalaby (Lala Sabathil)
	/// </summary>
	[JsonIgnore]
	public virtual string UsernameWithGlobalName
		=> this.GlobalName != null ? $"{this.Username} ({this.GlobalName})" : this.Username;

	/// <summary>
	///     Gets this user's global name.
	///     Only applicable if <see cref="IsMigrated" /> is <see langword="true" />.
	/// </summary>
	[JsonProperty("global_name", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string? GlobalName { get; internal set; }

	/// <summary>
	///     <para>Whether this user account is migrated to the new username system.</para>
	///     <para>Learn more at <see href="https://dis.gd/usernames">dis.gd/usernames</see>.</para>
	/// </summary>
	[JsonIgnore]
	public virtual bool IsMigrated
		=> this.Discriminator == "0";

	/// <summary>
	///     Gets the user's 4-digit discriminator.
	/// </summary>
	[JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Discriminator { get; internal set; }

	/// <summary>
	///     Gets the discriminator integer.
	/// </summary>
	[JsonIgnore, Deprecated("Users are being migrated currently. Bots still have discrims")]
	internal int DiscriminatorInt
		=> int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

	/// <summary>
	///     Gets the user's banner color, if set. Mutually exclusive with <see cref="BannerHash" />.
	/// </summary>
	[JsonIgnore]
	public virtual DiscordColor? BannerColor
		=> !this.BannerColorInternal.HasValue ? null : new DiscordColor(this.BannerColorInternal.Value);

	/// <summary>
	///     Gets the user's theme colors, if set.
	/// </summary>
	[JsonIgnore]
	public virtual IReadOnlyList<DiscordColor>? ThemeColors => !(this.ThemeColorsInternal is not null && this.ThemeColorsInternal.Count != 0) ? null : this.ThemeColorsInternal.Select(x => new DiscordColor(x)).ToList();

	/// <summary>
	///     Gets the user's banner url
	/// </summary>
	[JsonIgnore]
	public string? BannerUrl
		=> string.IsNullOrWhiteSpace(this.BannerHash) ? null : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=4096";

	/// <summary>
	///     Gets the user's banner hash. Mutually exclusive with <see cref="BannerColor" />.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string BannerHash { get; internal set; }

	/// <summary>
	///     Gets the users bio.
	///     This is not available to bots tho.
	/// </summary>
	[JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Bio { get; internal set; }

	/// <summary>
	///     Gets the users primary guild.
	/// </summary>
	[JsonProperty("primary_guild", NullValueHandling = NullValueHandling.Ignore)]
	public virtual DiscordPrimaryGuild? PrimaryGuild { get; internal set; }

	/// <summary>
	///     Gets the user's avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string AvatarHash { get; internal set; }

	/// <summary>
	///     Gets the user's avatar decoration data.
	/// </summary>
	[JsonProperty("avatar_decoration_data", NullValueHandling = NullValueHandling.Ignore)]
	public virtual AvatarDecorationData? AvatarDecorationData { get; internal set; }

	/// <summary>
	///     Gets the user's collectibles.
	/// </summary>
	[JsonProperty("collectibles", NullValueHandling = NullValueHandling.Ignore)]
	public virtual DiscordCollectibles? Collectibles { get; internal set; }

	/// <summary>
	///     Returns a uri to this users profile.
	/// </summary>
	[JsonIgnore]
	public Uri ProfileUri
		=> new($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.USERS}/{this.Id}");

	/// <summary>
	///     Returns a string representing the direct URL to this users profile.
	/// </summary>
	/// <returns>The URL of this users profile.</returns>
	[JsonIgnore]
	public string ProfileUrl
		=> this.ProfileUri.AbsoluteUri;

	/// <summary>
	///     Gets the user's avatar url.
	/// </summary>
	[JsonIgnore]
	public string AvatarUrl
		=> string.IsNullOrWhiteSpace(this.AvatarHash) ? this.DefaultAvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.{(this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024";

	/// <summary>
	///     Gets the user's avatar decoration url.
	/// </summary>
	[JsonIgnore]
	public string? AvatarDecorationUrl => this.AvatarDecorationData?.AssetUrl;

	/// <summary>
	///     Gets the URL of default avatar for this user.
	/// </summary>
	[JsonIgnore]
	public string DefaultAvatarUrl
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMBED}{Endpoints.AVATARS}/{(this.IsMigrated ? (this.Id >> 22) % 6 : Convert.ToUInt64(this.DiscriminatorInt) % 5).ToString(CultureInfo.InvariantCulture)}.png?size=1024";

	/// <summary>
	///     Gets whether the user is a bot.
	/// </summary>
	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool IsBot { get; internal set; }

	/// <summary>
	///     Gets whether the user has multifactor authentication enabled.
	/// </summary>
	[JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool? MfaEnabled { get; internal set; }

	/// <summary>
	///     Gets whether the user is an official Discord system user.
	/// </summary>
	[JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool? IsSystem { get; internal set; }

	/// <summary>
	///     Gets whether the user is verified.
	///     <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
	public virtual bool? Verified { get; internal set; }

	/// <summary>
	///     Gets the user's email address.
	///     <para>This is only present in OAuth.</para>
	/// </summary>
	[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string? Email { get; internal set; }

	/// <summary>
	///     Gets the user's premium type.
	/// </summary>
	[JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
	public virtual PremiumType? PremiumType { get; internal set; }

	/// <summary>
	///     Gets the user's chosen language
	/// </summary>
	[JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string Locale { get; internal set; }

	/// <summary>
	///     Gets the user's flags for OAuth.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public virtual UserFlags? OAuthFlags { get; internal set; }

	/// <summary>
	///     Gets the user's flags.
	/// </summary>
	[JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
	public virtual UserFlags? Flags { get; internal set; }

	/// <summary>
	///     Gets the user's pronouns.
	/// </summary>
	[JsonProperty("pronouns", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string? Pronouns { get; internal set; }

	/// <summary>
	///     Gets the user's mention string.
	/// </summary>
	[JsonIgnore]
	public string Mention
		=> this.Mention(this is DiscordMember);

	/// <summary>
	///     Gets whether this user is the Client which created this object.
	/// </summary>
	[JsonIgnore]
	public bool IsCurrent
		=> this.Id == this.Discord.CurrentUser.Id;

	/// <summary>
	///     Gets the user's access token.
	///     <para>Can be used in combination with <see cref="DiscordOAuth2Client" />.</para>
	///     <para>You can generate a token object from json with <see cref="DiscordAccessToken.FromJson" />, if needed.</para>
	///     <para>
	///         As alternative you can construct the object via
	///         <code>new DiscordAccessToken(string accessToken, string tokenType, int expiresIn, string refreshToken, string scope)</code>
	///         .
	///     </para>
	/// </summary>
	[JsonIgnore]
	public DiscordAccessToken? AccessToken { get; set; }

	/// <summary>
	///     Gets this user's presence.
	/// </summary>
	[JsonIgnore]
	public DiscordPresence? Presence
		=> this.Discord is DiscordClient dc && dc.Presences.TryGetValue(this.Id, out var presence) ? presence : null;

	/// <summary>
	///     Checks whether this <see cref="DiscordUser" /> is equal to another <see cref="DiscordUser" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordUser" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordUser" /> is equal to this <see cref="DiscordUser" />.</returns>
	public bool Equals(DiscordUser e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	///     Fetches the user from the API.
	/// </summary>
	/// <returns>The user with fresh data from the API.</returns>
	public async Task<DiscordUser> GetFromApiAsync()
		=> await this.Discord.ApiClient.GetUserAsync(this.Id).ConfigureAwait(false);

	/// <summary>
	///     Gets additional information about an application if the user is an bot.
	/// </summary>
	/// <returns>The rpc info or <see langword="null" /></returns>
	/// <exception cref="NotFoundException">Thrown when the application does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordRpcApplication?> GetRpcInfoAsync()
		=> this.IsBot ? await this.Discord.ApiClient.GetApplicationRpcInfoAsync(this.Id).ConfigureAwait(false) : await Task.FromResult<DiscordRpcApplication?>(null).ConfigureAwait(false);

	/// <summary>
	///     Whether this user is in a <see cref="DiscordGuild" />
	/// </summary>
	/// <example>
	///     <code>
	/// DiscordGuild guild = await Client.GetGuildAsync(806675511555915806);
	/// DiscordUser user = await Client.GetUserAsync(469957180968271873);
	/// Console.WriteLine($"{user.Username} {(user.IsInGuild(guild) ? "is a" : "is not a")} member of {guild.Name}");
	/// </code>
	///     results to <c>J_M_Lutra is a member of Project Nyaw~</c>.
	/// </example>
	/// <param name="guild">
	///     <see cref="DiscordGuild" />
	/// </param>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	public async Task<bool> IsInGuild(DiscordGuild guild)
	{
		try
		{
			var member = await guild.GetMemberAsync(this.Id).ConfigureAwait(false);
			return member is not null;
		}
		catch (NotFoundException)
		{
			return false;
		}
	}

	/// <summary>
	///     Whether this user is not in a <see cref="DiscordGuild" />
	/// </summary>
	/// <param name="guild">
	///     <see cref="DiscordGuild" />
	/// </param>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	public async Task<bool> IsNotInGuild(DiscordGuild guild)
		=> !await this.IsInGuild(guild).ConfigureAwait(false);

	/// <summary>
	///     Returns the DiscordMember in the specified <see cref="DiscordGuild" />
	/// </summary>
	/// <param name="guild">The <see cref="DiscordGuild" /> to get this user on.</param>
	/// <returns>The <see cref="DiscordMember" />.</returns>
	/// <exception cref="NotFoundException">Thrown when the user is not part of the guild.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMember> ConvertToMember(DiscordGuild guild)
		=> await guild.GetMemberAsync(this.Id).ConfigureAwait(false);

	/// <summary>
	///     Unbans this user from a guild.
	/// </summary>
	/// <param name="guild">Guild to unban this user from.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnbanAsync(DiscordGuild guild, string? reason = null)
		=> guild.UnbanMemberAsync(this, reason);

	/// <summary>
	///     Gets the user's avatar URL, in requested format and size.
	/// </summary>
	/// <param name="fmt">Format of the avatar to get.</param>
	/// <param name="size">Maximum size of the avatar. Must be a power of two, minimum 16, maximum 2048.</param>
	/// <returns>URL of the user's avatar.</returns>
	public string GetAvatarUrl(MediaFormat fmt, ushort size = 1024)
	{
		if (fmt is MediaFormat.Unknown)
			throw new ArgumentException("You must specify valid image format.", nameof(fmt));

		if (size is < 16 or > 2048)
			throw new ArgumentOutOfRangeException(nameof(size));

		var log = Math.Log(size, 2);
		if (log < 4 || log > 11 || log % 1 is not 0)
			throw new ArgumentOutOfRangeException(nameof(size));

		var sfmt = fmt switch
		{
			MediaFormat.Gif => "gif",
			MediaFormat.Jpeg => "jpg",
			MediaFormat.Png => "png",
			MediaFormat.WebP => "webp",
			MediaFormat.Auto => !string.IsNullOrWhiteSpace(this.AvatarHash) ? this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png" : "png",
			_ => throw new ArgumentOutOfRangeException(nameof(fmt))
		};
		var ssize = size.ToString(CultureInfo.InvariantCulture);
		if (!string.IsNullOrWhiteSpace(this.AvatarHash))
		{
			var id = this.Id.ToString(CultureInfo.InvariantCulture);
			return $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.AVATARS}/{id}/{this.AvatarHash}.{sfmt}?size={ssize}";
		}

		var type = (this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture);
		return $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.EMBED}{Endpoints.AVATARS}/{type}.{sfmt}?size={ssize}";
	}

	/// <summary>
	///     Creates a direct message channel to this user.
	/// </summary>
	/// <returns>Direct message channel to this user.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the user has the bot blocked, the member shares no guild with the
	///     bot, or if the member has Allow DM from server members off.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordDmChannel> CreateDmChannelAsync()
		=> this.Discord.ApiClient.CreateDmAsync(this.Id);

	/// <summary>
	///     Sends a direct message to this user. Creates a direct message channel if one does not exist already.
	/// </summary>
	/// <param name="content">Content of the message to send.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the user has the bot blocked, the member shares no guild with the
	///     bot, or if the member has Allow DM from server members off.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> SendMessageAsync(string content)
	{
		if (this.IsBot && this.Discord.CurrentUser.IsBot)
			throw new ArgumentException("Bots cannot DM each other.");

		var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
		return await chn.SendMessageAsync(content).ConfigureAwait(false);
	}

	/// <summary>
	///     Sends a direct message to this user. Creates a direct message channel if one does not exist already.
	/// </summary>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the user has the bot blocked, the member shares no guild with the
	///     bot, or if the member has Allow DM from server members off.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed)
	{
		if (this.IsBot && this.Discord.CurrentUser.IsBot)
			throw new ArgumentException("Bots cannot DM each other.");

		var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
		return await chn.SendMessageAsync(embed).ConfigureAwait(false);
	}

	/// <summary>
	///     Sends a direct message to this user. Creates a direct message channel if one does not exist already.
	/// </summary>
	/// <param name="content">Content of the message to send.</param>
	/// <param name="embed">Embed to attach to the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the user has the bot blocked, the member shares no guild with the
	///     bot, or if the member has Allow DM from server members off.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed)
	{
		if (this.IsBot && this.Discord.CurrentUser.IsBot)
			throw new ArgumentException("Bots cannot DM each other.");

		var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
		return await chn.SendMessageAsync(content, embed).ConfigureAwait(false);
	}

	/// <summary>
	///     Sends a direct message to this user. Creates a direct message channel if one does not exist already.
	/// </summary>
	/// <param name="message">Builder to with the message.</param>
	/// <returns>The sent message.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the user has the bot blocked, the member shares no guild with the
	///     bot, or if the member has Allow DM from server members off.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder message)
	{
		if (this.IsBot && this.Discord.CurrentUser.IsBot)
			throw new ArgumentException("Bots cannot DM each other.");

		var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
		return await chn.SendMessageAsync(message).ConfigureAwait(false);
	}

	/// <summary>
	///     Returns a string representation of this user.
	/// </summary>
	/// <returns>String representation of this user.</returns>
	public override string ToString()
		=> this.IsMigrated ? $"User {this.Id}; {this.UsernameWithGlobalName}" : $"User {this.Id}; {this.UsernameWithDiscriminator}";

	/// <summary>
	///     Checks whether this <see cref="DiscordUser" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordUser" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordUser);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordUser" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordUser" />.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	///     Gets whether the two <see cref="DiscordUser" /> objects are equal.
	/// </summary>
	/// <param name="e1">First user to compare.</param>
	/// <param name="e2">Second user to compare.</param>
	/// <returns>Whether the two users are equal.</returns>
	public static bool operator ==(DiscordUser? e1, DiscordUser? e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 is null && o2 is null) || (o1 is not null && o2 is not null && e1.Id == e2.Id);
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordUser" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First user to compare.</param>
	/// <param name="e2">Second user to compare.</param>
	/// <returns>Whether the two users are not equal.</returns>
	public static bool operator !=(DiscordUser? e1, DiscordUser? e2)
		=> !(e1 == e2);

#region Extension of DiscordUser

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.CertifiedModerator" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsMod
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.CertifiedModerator);

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.Partner" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsPartner
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.Partner);

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.VerifiedBot" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsVerifiedBot
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.VerifiedBot);

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.VerifiedDeveloper" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsBotDev
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.VerifiedDeveloper);

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.ActiveDeveloper" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsActiveDeveloper
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.ActiveDeveloper);

	/// <summary>
	///     Whether this member is a <see cref="UserFlags.Staff" />
	/// </summary>
	/// <returns>
	///     <see cref="bool" />
	/// </returns>
	[JsonIgnore]
	public bool IsStaff
		=> this.Flags.HasValue && this.Flags.Value.HasFlag(UserFlags.Staff);

#endregion

#region OAuth2 Methods

	/// <summary>
	///     Gets the current user's connections.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<IReadOnlyList<DiscordConnection>> OAuth2GetConnectionsAsync(DiscordOAuth2Client oauth2Client)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserConnectionsAsync(this.AccessToken) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's guilds.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<IReadOnlyList<DiscordGuild>> OAuth2GetGuildAsync(DiscordOAuth2Client oauth2Client)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserGuildsAsync(this.AccessToken) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's member object for given <paramref name="guild" />.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <param name="guild">The guild to get the member object for.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordMember> OAuth2GetGuildMemberAsync(DiscordOAuth2Client oauth2Client, DiscordGuild guild)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserGuildMemberAsync(this.AccessToken, guild.Id) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's member object for given <paramref name="guildId" />.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <param name="guildId">The guild id to get the member object for.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordMember> OAuth2GetGuildMemberAsync(DiscordOAuth2Client oauth2Client, ulong guildId)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserGuildMemberAsync(this.AccessToken, guildId) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     <para>Adds the user to the given <paramref name="guildId" />.</para>
	///     <para>
	///         Some parameters might need additional permissions for the bot on the target guild. See
	///         https://discord.com/developers/docs/resources/guild#add-guild-member for details.
	///     </para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <param name="guildId">The guild id to add the member to.</param>
	/// <param name="nickname">The new nickname.</param>
	/// <param name="roles">The new roles.</param>
	/// <param name="muted">Whether this user has to be muted.</param>
	/// <param name="deafened">Whether this user has to be deafened.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordMember> OAuth2AddToGuildAsync(DiscordOAuth2Client oauth2Client, ulong guildId, string? nickname = null, IEnumerable<DiscordRole>? roles = null, bool? muted = null, bool? deafened = null)
		=> this.AccessToken is not null ? await oauth2Client.AddCurrentUserToGuildAsync(this.AccessToken, this.Id, guildId, nickname, roles, muted, deafened) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     <para>Adds the user to the given <paramref name="guild" />.</para>
	///     <para>
	///         Some parameters might need additional permissions for the bot on the target guild. See
	///         https://discord.com/developers/docs/resources/guild#add-guild-member for details.
	///     </para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <param name="guild">The guild to add the member to.</param>
	/// <param name="nickname">The new nickname.</param>
	/// <param name="roles">The new roles.</param>
	/// <param name="muted">Whether this user has to be muted.</param>
	/// <param name="deafened">Whether this user has to be deafened.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordMember> OAuth2AddToGuildAsync(DiscordOAuth2Client oauth2Client, DiscordGuild guild, string? nickname = null, IEnumerable<DiscordRole>? roles = null, bool? muted = null, bool? deafened = null)
		=> this.AccessToken is not null ? await oauth2Client.AddCurrentUserToGuildAsync(this.AccessToken, this.Id, guild.Id, nickname, roles, muted, deafened) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's oauth2 object.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordUser> OAuth2GetAsync(DiscordOAuth2Client oauth2Client)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserAsync(this.AccessToken) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's authorization info.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordAuthorizationInformation> OAuth2GetAuthorizationInfoAsync(DiscordOAuth2Client oauth2Client)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentAuthorizationInformationAsync(this.AccessToken) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Gets the current user's application role connection.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordApplicationRoleConnection> OAuth2GetApplicationRoleConnectionAsync(DiscordOAuth2Client oauth2Client)
		=> this.AccessToken is not null ? await oauth2Client.GetCurrentUserApplicationRoleConnectionAsync(this.AccessToken) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

	/// <summary>
	///     Updates the current user's application role connection.
	///     <para>Requires a <see cref="DiscordAccessToken" /> set in <see cref="AccessToken" />.</para>
	/// </summary>
	/// <param name="oauth2Client">The oauth2 client.</param>
	/// <param name="platformName">The platform name.</param>
	/// <param name="platformUsername">The platform username.</param>
	/// <param name="metadata">The metadata.</param>
	/// <exception cref="NullReferenceException">Thrown when <see cref="AccessToken" /> is not present.</exception>
	public async Task<DiscordApplicationRoleConnection> OAuth2UpdateApplicationRoleConnectionAsync(DiscordOAuth2Client oauth2Client, string platformName, string platformUsername, ApplicationRoleConnectionMetadata metadata)
		=> this.AccessToken is not null ? await oauth2Client.UpdateCurrentUserApplicationRoleConnectionAsync(this.AccessToken, platformName, platformUsername, metadata) : throw new NullReferenceException("You need to specify the AccessToken on this DiscordUser entity.");

#endregion
}

/// <summary>
///     Represents a user comparer.
/// </summary>
internal sealed class DiscordUserComparer : IEqualityComparer<DiscordUser>
{
	/// <summary>
	///     Whether the users are equal.
	/// </summary>
	/// <param name="x">The first user</param>
	/// <param name="y">The second user.</param>
	public bool Equals(DiscordUser x, DiscordUser y)
		=> x.Equals(y);

	/// <summary>
	///     Gets the hash code.
	/// </summary>
	/// <param name="obj">The user.</param>
	public int GetHashCode(DiscordUser obj)
		=> obj.Id.GetHashCode();
}
