using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
/// Represents a member filter.
/// </summary>
public sealed class DiscordMemberFilter
{
	/// <summary>
	/// Query to match member IDs against.
	/// Possible query types: <see cref="DiscordQuery.OrQuery"/> or <see cref="DiscordQuery.RangeQuery"/>.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? UserId { get; set; }

	/// <summary>
	/// Query to match display name(s), username(s), and nickname(s) against.
	/// Possible query types: <see cref="DiscordQuery.OrQuery"/>.
	/// </summary>
	[JsonProperty("usernames", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? Usernames { get; set; }

	/// <summary>
	/// IDs of roles to match members against.
	/// Possible query types: <see cref="DiscordQuery.OrQuery"/> or <see cref="DiscordQuery.AndQuery"/>.
	/// </summary>
	[JsonProperty("role_ids", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? RoleIds { get; set; }

	/// <summary>
	/// When the user joined the guild.
	/// Possible query types: <see cref="DiscordQuery.RangeQuery"/>.
	/// </summary>
	[JsonProperty("guild_joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? GuildJoinedAt { get; set; }

	/// <summary>
	/// Safety signals to match members against.
	/// </summary>
	[JsonProperty("safety_signals", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordSafetySignals? SafetySignals { get; set; }

	/// <summary>
	/// Whether the member has not yet passed the guild's member verification requirements.
	/// </summary>
	[JsonProperty("is_pending", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsPending { get; set; }

	/// <summary>
	/// Whether the member left and rejoined the guild.
	/// </summary>
	[JsonProperty("did_rejoin", NullValueHandling = NullValueHandling.Ignore)]
	public bool? DidRejoin { get; set; }

	/// <summary>
	/// How the user joined the guild.
	/// Possible query types: <see cref="DiscordQuery.OrQuery"/>.
	/// </summary>
	[JsonProperty("join_source_type", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? JoinSourceType { get; set; }

	/// <summary>
	/// The invite code or vanity used to join the guild.
	/// Possible query types: <see cref="DiscordQuery.OrQuery"/>.
	/// </summary>
	[JsonProperty("source_invite_code", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? SourceInviteCode { get; set; }
}
