using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord guild profile.
/// </summary>
public sealed class DiscordGuildProfile : SnowflakeObject
{
    /// <summary>
    ///     Gets the guild name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    ///     Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
    public string? IconHash { get; internal set; }

    /// <summary>
    ///     Gets the guild icon's url.
    /// </summary>
    [JsonIgnore]
    public string? IconUrl
        => !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.{(this.IconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024" : null;

    /// <summary>
    ///     Gets the guild banner's hash.
    /// </summary>
    [JsonProperty("banner", NullValueHandling = NullValueHandling.Include)]
    public string? BannerHash { get; internal set; }

    /// <summary>
    ///     Gets the guild banner's url.
    /// </summary>
    [JsonIgnore]
    public string? BannerUrl
        => !string.IsNullOrWhiteSpace(this.BannerHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}" : null;

    /// <summary>
    ///     Gets the guild custom banner's hash.
    /// </summary>
    [JsonProperty("custom_banner", NullValueHandling = NullValueHandling.Include)]
    public string? CustomBannerHash { get; internal set; }

    /// <summary>
    ///     Gets the guild custom banner's url.
    /// </summary>
    [JsonIgnore]
    public string? CustomBannerUrl
        => !string.IsNullOrWhiteSpace(this.CustomBannerHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.CUSTOM_BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.CustomBannerHash}.{(this.CustomBannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}" : null;

    /// <summary>
    ///     Gets the member count.
    /// </summary>
    [JsonProperty("member_count", NullValueHandling = NullValueHandling.Include)]
    public int? MemberCount { get; internal set; }

    /// <summary>
    ///     Gets the online count.
    /// </summary>
    [JsonProperty("online_count", NullValueHandling = NullValueHandling.Include)]
    public int? OnlineCount { get; internal set; }

    /// <summary>
    ///     Gets the guild description.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
    public string? Description { get; internal set; }

    /// <summary>
    ///     Gets the primary brand color.
    /// </summary>
    [JsonProperty("brand_color_primary", NullValueHandling = NullValueHandling.Include)]
    public DiscordColor? BrandColorPrimary { get; internal set; }

    /// <summary>
    ///     Gets the primary badge color.
    /// </summary>
    [JsonProperty("badge_color_primary", NullValueHandling = NullValueHandling.Include)]
    public DiscordColor? BadgeColorPrimary { get; internal set; }

    /// <summary>
    ///     Gets the secondary badge color.
    /// </summary>
    [JsonProperty("badge_color_secondary", NullValueHandling = NullValueHandling.Include)]
    public DiscordColor? BadgeColorSecondary { get; internal set; }

    /// <summary>
    ///     Gets the badge hash.
    /// </summary>
    [JsonProperty("badge_hash", NullValueHandling = NullValueHandling.Include)]
    public string? BadgeHash { get; internal set; }

    /// <summary>
    ///     Gets the badge url.
    /// </summary>
    [JsonIgnore]
    public string? BadgeUrl
        => !string.IsNullOrWhiteSpace(this.BadgeHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.BADGES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BadgeHash}.png" : null;

    /// <summary>
    ///     Gets the badge.
    /// </summary>
    [JsonProperty("badge", NullValueHandling = NullValueHandling.Ignore)]
    public int Badge { get; internal set; } = 0;

    /// <summary>
    ///     Gets the guild tag.
    /// </summary>
    [JsonProperty("tag", NullValueHandling = NullValueHandling.Include)]
    public string? Tag { get; internal set; }

    /// <summary>
    ///     Gets the game application IDs.
    /// </summary>
    [JsonProperty("game_application_ids", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<ulong> GameApplicationIds { get; internal set; } = [];

    /// <summary>
    ///     Gets the game activities.
    /// </summary>
    [JsonProperty("game_activity", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyDictionary<ulong, DiscordGameActivity> GameActivity { get; internal set; } = new Dictionary<ulong, DiscordGameActivity>();

    /// <summary>
    ///     Gets the traits.
    /// </summary>
    [JsonProperty("traits", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordGuildTrait> Traits { get; internal set; } = [];

    /// <summary>
    ///     Gets the features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Features { get; internal set; } = [];

    /// <summary>
    ///     <para>Gets the visibility.</para>
	///     <para><c>1</c> seems to be the default, for discovery, apply to join and invite only.</para>
	///     <para><c>3</c> seems to be for when you enabled <c>Apply From Profile</c> when having apply to join enabled.</para>
    /// </summary>
    [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
    public int Visibility { get; internal set; }
}
