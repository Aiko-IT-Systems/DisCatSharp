using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>Predefined game statistics supported by Discord's game-stat widgets.</summary>
public sealed class DiscordGamePrimaryStats
{
	/// <summary>Gets or sets the current season.</summary>
	[JsonProperty("season", NullValueHandling = NullValueHandling.Ignore)] public string? Season { get; set; }
	/// <summary>Gets or sets the current rank.</summary>
	[JsonProperty("rank_name", NullValueHandling = NullValueHandling.Ignore)] public string? RankName { get; set; }
	/// <summary>Gets or sets the current rank image.</summary>
	[JsonProperty("rank_image", NullValueHandling = NullValueHandling.Ignore)] public DiscordGameProfileMedia? RankImage { get; set; }
	/// <summary>Gets or sets the highest rank.</summary>
	[JsonProperty("highest_rank", NullValueHandling = NullValueHandling.Ignore)] public string? HighestRank { get; set; }
	/// <summary>Gets or sets the highest rank image.</summary>
	[JsonProperty("highest_rank_image", NullValueHandling = NullValueHandling.Ignore)] public DiscordGameProfileMedia? HighestRankImage { get; set; }
	/// <summary>Gets or sets the featured character name.</summary>
	[JsonProperty("featured_played_character", NullValueHandling = NullValueHandling.Ignore)] public string? FeaturedPlayedCharacter { get; set; }
	/// <summary>Gets or sets the featured character image.</summary>
	[JsonProperty("featured_played_character_image", NullValueHandling = NullValueHandling.Ignore)] public DiscordGameProfileMedia? FeaturedPlayedCharacterImage { get; set; }
	/// <summary>Gets or sets the total playtime in hours, including fractional hours.</summary>
	[JsonProperty("playtime_hours", NullValueHandling = NullValueHandling.Ignore)] public double? PlaytimeHours { get; set; }
	/// <summary>Gets or sets the total wins.</summary>
	[JsonProperty("total_wins", NullValueHandling = NullValueHandling.Ignore)] public long? TotalWins { get; set; }
	/// <summary>Gets or sets wins in the current period.</summary>
	[JsonProperty("current_period_wins", NullValueHandling = NullValueHandling.Ignore)] public long? CurrentPeriodWins { get; set; }
	/// <summary>Gets or sets the total games played.</summary>
	[JsonProperty("total_games", NullValueHandling = NullValueHandling.Ignore)] public long? TotalGames { get; set; }
	/// <summary>Gets or sets games played in the current period.</summary>
	[JsonProperty("current_period_games", NullValueHandling = NullValueHandling.Ignore)] public long? CurrentPeriodGames { get; set; }
	/// <summary>Gets or sets the total kills.</summary>
	[JsonProperty("total_kills", NullValueHandling = NullValueHandling.Ignore)] public long? TotalKills { get; set; }
	/// <summary>Gets or sets kills in the current period.</summary>
	[JsonProperty("current_period_kills", NullValueHandling = NullValueHandling.Ignore)] public long? CurrentPeriodKills { get; set; }
	/// <summary>Gets or sets the total assists.</summary>
	[JsonProperty("total_assists", NullValueHandling = NullValueHandling.Ignore)] public long? TotalAssists { get; set; }
	/// <summary>Gets or sets assists in the current period.</summary>
	[JsonProperty("current_period_assists", NullValueHandling = NullValueHandling.Ignore)] public long? CurrentPeriodAssists { get; set; }
	/// <summary>Gets or sets the total deaths.</summary>
	[JsonProperty("total_deaths", NullValueHandling = NullValueHandling.Ignore)] public long? TotalDeaths { get; set; }
	/// <summary>Gets or sets deaths in the current period.</summary>
	[JsonProperty("current_period_deaths", NullValueHandling = NullValueHandling.Ignore)] public long? CurrentPeriodDeaths { get; set; }
	/// <summary>Preserves unrecognized primary stat fields.</summary>
	[JsonExtensionData] public IDictionary<string, JToken>? AdditionalData { get; set; }
}
