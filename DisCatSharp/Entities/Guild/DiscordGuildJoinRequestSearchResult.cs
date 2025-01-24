using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a guild join request search result.
/// </summary>
public sealed class DiscordGuildJoinRequestSearchResult : ObservableApiObject
{
	/// <summary>
	///     Gets the limit for results.
	/// </summary>
	[JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
	public int Limit { get; internal set; }

	/// <summary>
	///     Gets the total count of results.
	/// </summary>
	[JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
	public int? Total { get; internal set; }

	/// <summary>
	///     Gets the join requests.
	/// </summary>
	[JsonProperty("guild_join_requests", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordGuildJoinRequest> JoinRequests { get; internal set; } = [];
}
