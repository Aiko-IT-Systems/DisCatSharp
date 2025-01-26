using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the settings for a Discord clan.
/// </summary>
public sealed class DiscordClanSettings : ObservableApiObject
{
	/// <summary>
	///     Gets the verification form for the clan.
	/// </summary>
	[JsonProperty("verification_form")]
	public DiscordGuildMembershipScreening VerificationForm { get; internal set; }

	/// <summary>
	///     Gets the tag for the clan.
	/// </summary>
	[JsonProperty("tag")]
	public string Tag { get; internal set; }

	/// <summary>
	///     Gets the description of the clan.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Gets the list of game application IDs associated with the clan.
	/// </summary>
	[JsonProperty("game_application_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> GameApplicationIds { get; internal set; } = [];

	/// <summary>
	///     Gets the search terms associated with the clan.
	/// </summary>
	[JsonProperty("search_terms", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> SearchTerms { get; internal set; } = [];

	/// <summary>
	///     Gets the wildcard descriptors associated with the clan.
	/// </summary>
	[JsonProperty("wildcard_descriptors", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string?> WildcardDescriptors { get; internal set; } = [];

	/// <summary>
	///     Gets the play style of the clan.
	/// </summary>
	[JsonProperty("play_style")]
	public PlayStyle PlayStyle { get; internal set; }

	/// <summary>
	///     Gets the primary badge color of the clan.
	/// </summary>
	[JsonProperty("badge_color_primary")]
	public DiscordColor BadgeColorPrimary { get; internal set; }

	/// <summary>
	///     Gets the secondary badge color of the clan.
	/// </summary>
	[JsonProperty("badge_color_secondary")]
	public DiscordColor BadgeColorSecondary { get; internal set; }

	/// <summary>
	///     Gets the primary brand color of the clan.
	/// </summary>
	[JsonProperty("brand_color_primary")]
	public DiscordColor BrandColorPrimary { get; internal set; }

	/// <summary>
	///     Gets the secondary brand color of the clan.
	/// </summary>
	[JsonProperty("brand_color_secondary")]
	public DiscordColor BrandColorSecondary { get; internal set; }

	/// <summary>
	///     Gets the guild ID associated with the clan.
	/// </summary>
	[JsonIgnore]
	internal ulong GuildId { get; set; }

	/// <summary>
	///     Gets the badge hash of the clan.
	/// </summary>
	[JsonProperty("badge_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string BadgeHash { get; internal set; }

	/// <summary>
	///     Gets the URL of the clan's badge.
	/// </summary>
	[JsonIgnore]
	public string BadgeUrl
		=> string.IsNullOrWhiteSpace(this.BadgeHash) ? null! : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.CLAN_BADGES}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}/{this.BadgeHash}.{(this.BadgeHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=512";

	/// <summary>
	///     Gets the banner hash of the clan.
	/// </summary>
	[JsonProperty("banner_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string BannerHash { get; internal set; }

	/// <summary>
	///     Gets the URL of the clan's banner.
	/// </summary>
	[JsonIgnore]
	public string BannerUrl
		=> string.IsNullOrWhiteSpace(this.BannerHash) ? null! : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.CLAN_BANNERS}/{this.GuildId.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=16";

	/// <summary>
	///     Gets the type of the clan's banner.
	/// </summary>
	[JsonProperty("banner")]
	public BannerType Banner { get; internal set; }

	/// <summary>
	///     Gets the type of the clan's badge.
	/// </summary>
	[JsonProperty("badge")]
	public BadgeType Badge { get; internal set; }

	/// <summary>
	///     Gets the profile banner hash of the clan.
	/// </summary>
	[JsonProperty("profile_banner_hash", NullValueHandling = NullValueHandling.Ignore)]
	public string ProfileBannerHash { get; internal set; }

	/* TODO: We don't know what the cdn link is yet
	[JsonIgnore]
	public string ProfileBannerUrl
		=> null!;
	*/
}
