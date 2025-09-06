using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Represents a member pagination filter.
/// </summary>
public sealed class DiscordMemberPaginationFilter
{
	/// <summary>
	///     The ID of the user to paginate past.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	/// <summary>
	///     When the user to paginate past joined the guild.
	/// </summary>
	[JsonProperty("guild_joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public long GuildJoinedAt { get; set; } = 0;
}
