using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a guild join request.
/// </summary>
public sealed class DiscordGuildJoinRequest : SnowflakeObject
{
	/// <summary>
	///     Gets the id of the join request.
	/// </summary>
	[JsonProperty("join_request_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong RequestId { get; internal set; }

	/// <summary>
	///     <para>Gets the id of the interview channel.</para>
	///     <para>This will be a group dm channel.</para>
	///     <para>Bots cannot access this, so it's pretty much useless.</para>
	/// </summary>
	[JsonProperty("interview_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? InterviewChannelId { get; internal set; }

	/// <summary>
	///     Gets the id of the user that created the join request.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; internal set; }

	/// <summary>
	///     Gets the user that created the join request.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	internal TransportUser UserInternal { get; set; }

	/// <summary>
	///     Gets the user that created the join request.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User
		=> new(this.UserInternal)
		{
			Discord = this.Discord
		};

	/// <summary>
	///     Gets the id of the guild that the join request is for.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	///     Gets the guild that the join request is for.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds[this.GuildId];

	/// <summary>
	///     Gets the form responses of the join request.
	/// </summary>
	[JsonProperty("form_responses", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordGuildMembershipScreeningFieldResponse> FormResponses { get; internal set; } = [];

	/// <summary>
	///     Gets the application status of the join request.
	/// </summary>
	[JsonProperty("application_status", NullValueHandling = NullValueHandling.Ignore)]
	public JoinRequestStatusType ApplicationStatus { get; internal set; }

	/// <summary>
	///     Gets the user that actioned the join request.
	/// </summary>
	[JsonProperty("actioned_by_user", NullValueHandling = NullValueHandling.Ignore)]
	internal TransportUser? ActionedByUserInternal { get; set; }

	/// <summary>
	///     Gets the user that actioned the join request.
	/// </summary>
	[JsonIgnore]
	public DiscordUser? ActionedByUser
		=> this.ActionedByUserInternal is not null
			? new(this.UserInternal)
			{
				Discord = this.Discord
			}
			: null;

	/// <summary>
	///     Gets the datetime of when the join request was actioned.
	/// </summary>
	[JsonProperty("actioned_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ActionedAt { get; internal set; }

	/// <summary>
	///     Gets the datetime of when the join request was created.
	/// </summary>
	[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset CreatedAt { get; internal set; }

	/// <summary>
	///     <para>Gets the datetime of when the user was last seen.</para>
	///     <para>Seems to always be see <see langword="null" /> tho.</para>
	/// </summary>
	[JsonProperty("last_seen", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? LastSeen { get; internal set; }

	/// <summary>
	///    Gets the rejection reason of the join request.
	/// </summary>
	[JsonProperty("rejection_reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? RejectionReason { get; internal set; }

	/// <summary>
	///     Modifies this join request.
	/// </summary>
	/// <param name="approve">Whether to approve or deny this request.</param>
	/// <param name="rejectionReason">The optional rejection reason.</param>
	[DiscordUnreleased("This feature is not available for bots at the current time"), Obsolete("This feature is not available for bots at the current time", true)]
	public async Task<DiscordGuildJoinRequest> ModifyAsync(bool approve, string? rejectionReason)
		=> await this.Discord.ApiClient.ModifyGuildJoinRequestsAsync(this.GuildId, this.RequestId, approve ? JoinRequestStatusType.Approved : JoinRequestStatusType.Rejected, rejectionReason);
}
