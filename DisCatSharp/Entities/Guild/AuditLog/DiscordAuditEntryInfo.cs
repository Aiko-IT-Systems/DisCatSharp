using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents additional information about the <see cref="DiscordAuditLogEntry"/>.
/// </summary>
public sealed class DiscordAuditEntryInfo : ObservableApiObject
{
	/// <summary>
	/// Gets the guild in which the entities were targeted.
	/// </summary>
	[JsonIgnore]
	internal DiscordGuild Guild
		=> (this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null!)!;

	/// <summary>
	/// Gets the ID of the guild in which the entities were targeted.
	/// </summary>
	[JsonIgnore]
	internal ulong GuildId { get; set; }

	/// <summary>
	/// Gets the ID of the app whose permissions were targeted.
	/// Event Type: <see cref="AuditLogActionType.ApplicationCommandPermissionUpdate"/>.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; } = null;

	/// <summary>
	/// Gets the name of the Auto Moderation rule that was triggered.
	/// Event Types: <see cref="AuditLogActionType.AutoModerationBlockMessage"/>, <see cref="AuditLogActionType.AutoModerationFlagToChannel"/>, <see cref="AuditLogActionType.AutoModerationUserCommunicationDisabled"/>
	/// </summary>
	[JsonProperty("auto_moderation_rule_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? AutoModerationRuleName { get; internal set; } = null;

	/// <summary>
	/// Gets the trigger type of the Auto Moderation rule that was triggered.
	/// Event Types: <see cref="AuditLogActionType.AutoModerationBlockMessage"/>, <see cref="AuditLogActionType.AutoModerationFlagToChannel"/>, <see cref="AuditLogActionType.AutoModerationUserCommunicationDisabled"/>
	/// </summary>
	[JsonProperty("auto_moderation_rule_trigger_type", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodTriggerType? AutoModerationRuleTriggerType { get; internal set; } = null;

	/// <summary>
	/// Gets the ID of the channel in which the entities were targeted.
	/// Event Types: <see cref="AuditLogActionType.MemberMove"/>, <see cref="AuditLogActionType.MessagePin"/>, <see cref="AuditLogActionType.MessageUnpin"/>, <see cref="AuditLogActionType.MessageDelete"/>, <see cref="AuditLogActionType.StageInstanceCreate"/>, <see cref="AuditLogActionType.StageInstanceUpdate"/>, <see cref="AuditLogActionType.StageInstanceDelete"/>, <see cref="AuditLogActionType.AutoModerationBlockMessage"/>, <see cref="AuditLogActionType.AutoModerationFlagToChannel"/>, <see cref="AuditLogActionType.AutoModerationUserCommunicationDisabled"/>
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; } = null;

	/// <summary>
	/// Gets the channel in which the entities were targeted.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel? Channel
		=> this.ChannelId.HasValue ? this.Guild.GetChannel(this.ChannelId.Value) : null;

	/// <summary>
	/// Gets the number of entities that were targeted.
	/// Event Types: <see cref="AuditLogActionType.MessageDelete"/>, <see cref="AuditLogActionType.MessageBulkDelete"/>, <see cref="AuditLogActionType.MemberDisconnect"/>, <see cref="AuditLogActionType.MemberMove"/>
	/// </summary>
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public int? Count { get; internal set; } = null;

	/// <summary>
	/// Gets the number of days after which inactive members were kicked.
	/// Event Type: <see cref="AuditLogActionType.MemberPrune"/>
	/// </summary>
	[JsonProperty("delete_member_days", NullValueHandling = NullValueHandling.Ignore)]
	public int? DeleteMemberDays { get; internal set; } = null;

	/// <summary>
	/// Gets the ID of the overwritten entity.
	/// Event Types: <see cref="AuditLogActionType.ChannelOverwriteCreate"/>, <see cref="AuditLogActionType.ChannelOverwriteUpdate"/>, <see cref="AuditLogActionType.ChannelOverwriteDelete"/>"/>
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OverwrittenEntityId { get; internal set; } = null;

	/// <summary>
	/// Gets the number of members removed by the prune.
	/// Event Type: <see cref="AuditLogActionType.MemberPrune"/>
	/// </summary>
	[JsonProperty("members_removed", NullValueHandling = NullValueHandling.Ignore)]
	public int? MembersRemoved { get; internal set; } = null;

	/// <summary>
	/// Gets the ID of the message that was targeted.
	/// Event Types: <see cref="AuditLogActionType.MessagePin"/>, <see cref="AuditLogActionType.MessageUnpin"/>
	/// </summary>
	[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? MessageId { get; internal set; } = null;

	/// <summary>
	/// Gets the name of the role if the type is "0" (not present if the type is "1").
	/// Event Types: <see cref="AuditLogActionType.ChannelOverwriteCreate"/>, <see cref="AuditLogActionType.ChannelOverwriteUpdate"/>, <see cref="AuditLogActionType.ChannelOverwriteDelete"/>
	/// </summary>
	[JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? RoleName { get; internal set; } = null;

	/// <summary>
	/// Gets the type of the overwritten entity - role ("0") or member ("1").
	/// Event Types: <see cref="AuditLogActionType.ChannelOverwriteCreate"/>, <see cref="AuditLogActionType.ChannelOverwriteUpdate"/>, <see cref="AuditLogActionType.ChannelOverwriteDelete"/>
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public OverwriteType? OverwrittenEntityType { get; internal set; } = null;

	/// <summary>
	/// Gets the type of integration which performed the action.
	/// Event Types: <see cref="AuditLogActionType.MemberKick"/>, <see cref="AuditLogActionType.MemberRoleUpdate"/>
	/// </summary>
	[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? IntegrationType { get; internal set; } = null;

	/// <summary>
	/// Gets the voice channel status.
	/// Event Types: <see cref="AuditLogActionType.VoiceChannelStatusUpdate"/>
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public string? Status { get; internal set; }
}
