using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a transport user.
/// </summary>
internal class TransportUser : ObservableApiObject
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
	/// Gets this user's global name.
	/// </summary>
	[JsonProperty("global_name", NullValueHandling = NullValueHandling.Ignore), DiscordInExperiment]
	public string GlobalName { get; internal set; }

	/// <summary>
	/// Gets or sets the discriminator.
	/// </summary>
	[JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
	internal string Discriminator { get; set; } = "0";

	/// <summary>
	/// Gets the username with discriminator.
	/// </summary>
	[JsonIgnore, DiscordDeprecated]
	internal string UsernameWithDiscriminator
		=> $"{this.Username}#{this.Discriminator}";

	/// <summary>
	/// Gets the username with the global name.
	/// </summary>
	[JsonIgnore, DiscordInExperiment]
	internal string UsernameWithGlobalName
		=> $"{this.Username} ({this.GlobalName})";

	/// <summary>
	/// Gets the avatar hash.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public string AvatarHash { get; internal set; }

	/// <summary>
	/// Gets the user's avatar decoration data.
	/// </summary>
	[JsonProperty("avatar_decoration_data", NullValueHandling = NullValueHandling.Ignore)]
	public AvatarDecorationData AvatarDecorationData { get; internal set; }

	/// <summary>
	/// Gets the banner hash.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string BannerHash { get; internal set; }

	/// <summary>
	/// Gets the banner color.
	/// </summary>
	[JsonProperty("accent_color", NullValueHandling = NullValueHandling.Ignore)]
	public int? BannerColor { get; internal set; }

	/// <summary>
	/// Gets the users theme colors.
	/// </summary>
	[JsonProperty("theme_colors", NullValueHandling = NullValueHandling.Ignore)]
	public int[]? ThemeColors { get; internal set; }

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
	internal TransportUser()
		: base(["display_name", "linked_users", "banner_color"])
	{ }

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
