using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a search guild members response.
/// </summary>
public sealed class DiscordSearchGuildMembersResponse : ObservableApiObject
{
	/// <summary>
	/// The ID of the guild searched.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// The resulting members.
	/// </summary>
	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordSupplementalGuildMember> Members { get; internal set; } = [];

	/// <summary>
	/// The number of results returned.
	/// </summary>
	[JsonProperty("page_result_count")]
	public int PageResultCount { get; internal set; }

	/// <summary>
	/// The total number of results found.
	/// </summary>
	[JsonProperty("total_result_count")]
	public int TotalResultCount { get; internal set; }
}
