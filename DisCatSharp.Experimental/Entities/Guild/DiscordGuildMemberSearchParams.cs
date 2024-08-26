using DisCatSharp.Experimental.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Represents guild member search params.
/// </summary>
public sealed class DiscordGuildMemberSearchParams
{
	/// <summary>
	///     Max number of members to return (<c>1</c>-<c>1000</c>, default <c>25</c>).
	/// </summary>
	[JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? Limit { get; set; } = 25;

	/// <summary>
	///     The sorting algorithm to use (default <see cref="MemberSortType.JoinedAtDesc" />).
	/// </summary>
	[JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
	public MemberSortType? Sort { get; set; } = MemberSortType.JoinedAtDesc;

	/// <summary>
	///     The filter criteria to match against members using OR logic.
	/// </summary>
	[JsonProperty("or_query", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMemberFilter? OrQuery { get; set; }

	/// <summary>
	///     The filter criteria to match against members using AND logic.
	/// </summary>
	[JsonProperty("and_query", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMemberFilter? AndQuery { get; set; }

	/// <summary>
	///     Get members before this member.
	/// </summary>
	[JsonProperty("before", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMemberPaginationFilter? Before { get; set; }

	/// <summary>
	///     Get members after this member.
	/// </summary>
	[JsonProperty("after", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMemberPaginationFilter? After { get; set; }
}
