using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net.AuditLogs;

/// <summary>
///     Represents a lightweight user object included in a Discord audit log response.
/// </summary>
/// <remarks>
///     Discord only includes the user fields needed to identify the actor that created the audit log entry.
///     This payload is intentionally narrower than the full user payload returned by other endpoints.
/// </remarks>
internal sealed class RawAuditLogUser
{
	/// <summary>
	///     Gets or sets the username supplied by Discord.
	/// </summary>
	[JsonProperty("username")]
	public string? Username { get; set; }

	/// <summary>
	///     Gets or sets the legacy discriminator supplied by Discord.
	/// </summary>
	[JsonProperty("discriminator")]
	public string? Discriminator { get; set; }

	/// <summary>
	///     Gets or sets the user identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the avatar hash.
	/// </summary>
	[JsonProperty("avatar")]
	public string? AvatarHash { get; set; }

	/// <summary>
	///     Gets or sets the user's global display name.
	/// </summary>
	[JsonProperty("global_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? GlobalName { get; set; }
}

/// <summary>
///     Represents a lightweight webhook object included in a Discord audit log response.
/// </summary>
internal sealed class RawAuditLogWebhook
{
	/// <summary>
	///     Gets or sets the webhook name.
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; set; }

	/// <summary>
	///     Gets or sets the channel identifier associated with the webhook.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the webhook token when Discord includes it.
	/// </summary>
	/// <remarks>
	///     The token is usually omitted for audit log payloads and must be treated as optional.
	/// </remarks>
	[JsonProperty("token")]
	public string? Token { get; set; }

	/// <summary>
	///     Gets or sets the webhook avatar hash.
	/// </summary>
	[JsonProperty("avatar")]
	public string? AvatarHash { get; set; }

	/// <summary>
	///     Gets or sets the guild identifier associated with the webhook.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; set; }

	/// <summary>
	///     Gets or sets the webhook identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }
}

/// <summary>
///     Represents a lightweight thread object included in a Discord audit log response.
/// </summary>
internal sealed class RawAuditLogThread
{
	/// <summary>
	///     Gets or sets the thread identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the guild identifier associated with the thread.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; set; }

	/// <summary>
	///     Gets or sets the parent channel identifier.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ParentId { get; set; }

	/// <summary>
	///     Gets or sets the Discord channel type for the thread.
	/// </summary>
	[JsonProperty("type")]
	public ChannelType Type { get; set; }

	/// <summary>
	///     Gets or sets the thread name.
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; set; }
}

/// <summary>
///     Represents a lightweight guild scheduled event object included in a Discord audit log response.
/// </summary>
internal sealed class RawAuditLogGuildScheduledEvent
{
	/// <summary>
	///     Gets or sets the scheduled event identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the guild identifier associated with the scheduled event.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; set; }

	/// <summary>
	///     Gets or sets the channel identifier associated with the scheduled event.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the creator user identifier.
	/// </summary>
	[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? CreatorId { get; set; }

	/// <summary>
	///     Gets or sets the scheduled event name.
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; set; }

	/// <summary>
	///     Gets or sets the scheduled event description.
	/// </summary>
	[JsonProperty("description")]
	public string? Description { get; set; }
}

/// <summary>
///     Represents a lightweight integration object included in a Discord audit log response.
/// </summary>
internal sealed class RawAuditLogIntegration
{
	/// <summary>
	///     Gets or sets the integration identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the integration name.
	/// </summary>
	[JsonProperty("name")]
	public string? Name { get; set; }

	/// <summary>
	///     Gets or sets the integration type.
	/// </summary>
	[JsonProperty("type")]
	public string? Type { get; set; }
}

/// <summary>
///     Represents a raw audit log change object as returned by Discord.
/// </summary>
internal sealed class RawAuditLogChange
{
	/// <summary>
	///     Gets or sets the old raw value.
	/// </summary>
	[JsonProperty("old_value")]
	public JToken? OldValue { get; set; }

	/// <summary>
	///     Gets or sets the new raw value.
	/// </summary>
	[JsonProperty("new_value")]
	public JToken? NewValue { get; set; }

	/// <summary>
	///     Gets or sets the Discord change key.
	/// </summary>
	[JsonProperty("key")]
	public string Key { get; set; } = string.Empty;
}

/// <summary>
///     Represents the raw <c>options</c> object attached to an audit log entry.
/// </summary>
/// <remarks>
///     Discord serializes several numeric values in audit log options as strings. Those values are normalized later
///     by the parser registry, so the raw model preserves the original wire format.
/// </remarks>
internal sealed class RawAuditLogEntryOptions
{
	/// <summary>
	///     Gets or sets the application identifier.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; set; }

	/// <summary>
	///     Gets or sets the auto moderation rule name.
	/// </summary>
	[JsonProperty("auto_moderation_rule_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? AutoModerationRuleName { get; set; }

	/// <summary>
	///     Gets or sets the auto moderation rule trigger type.
	/// </summary>
	[JsonProperty("auto_moderation_rule_trigger_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? AutoModerationRuleTriggerType { get; set; }

	/// <summary>
	///     Gets or sets the channel identifier.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the raw count value.
	/// </summary>
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public string? Count { get; set; }

	/// <summary>
	///     Gets or sets the raw deleted member day count.
	/// </summary>
	[JsonProperty("delete_member_days", NullValueHandling = NullValueHandling.Ignore)]
	public string? DeleteMemberDays { get; set; }

	/// <summary>
	///     Gets or sets the scheduled event exception identifier.
	/// </summary>
	[JsonProperty("event_exception_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EventExceptionId { get; set; }

	/// <summary>
	///     Gets or sets the generic identifier included in some option payloads.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Id { get; set; }

	/// <summary>
	///     Gets or sets the raw removed member count.
	/// </summary>
	[JsonProperty("members_removed", NullValueHandling = NullValueHandling.Ignore)]
	public string? MembersRemoved { get; set; }

	/// <summary>
	///     Gets or sets the message identifier.
	/// </summary>
	[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? MessageId { get; set; }

	/// <summary>
	///     Gets or sets the role name included with overwrite entries.
	/// </summary>
	[JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? RoleName { get; set; }

	/// <summary>
	///     Gets or sets the status value supplied by Discord.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public string? Status { get; set; }

	/// <summary>
	///     Gets or sets the entry type discriminator included by Discord for some option payloads.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string? Type { get; set; }

	/// <summary>
	///     Gets or sets the integration type.
	/// </summary>
	[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? IntegrationType { get; set; }

	/// <summary>
	///     Gets or sets any additional option fields not modeled explicitly by the raw transport type.
	/// </summary>
	/// <remarks>
	///     Preserving unknown option fields allows the parser to expose forward-compatible raw data even when the
	///     library has not added a dedicated property yet.
	/// </remarks>
	[JsonExtensionData]
	public IDictionary<string, JToken>? AdditionalData { get; set; }
}

/// <summary>
///     Represents a raw audit log entry as returned by Discord.
/// </summary>
internal sealed class RawAuditLogEntry
{
	/// <summary>
	///     Gets or sets the raw target identifier.
	/// </summary>
	/// <remarks>
	///     Discord serializes this field as a string and may omit it entirely for some action types.
	/// </remarks>
	[JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? TargetId { get; set; }

	/// <summary>
	///     Gets or sets the raw change collection.
	/// </summary>
	[JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogChange>? Changes { get; set; }

	/// <summary>
	///     Gets or sets the user identifier for the actor that created the entry.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	/// <summary>
	///     Gets or sets the audit log entry identifier.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the action type.
	/// </summary>
	[JsonProperty("action_type")]
	public AuditLogActionType ActionType { get; set; }

	/// <summary>
	///     Gets or sets the raw options object.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public RawAuditLogEntryOptions? Options { get; set; }

	/// <summary>
	///     Gets or sets the audit log reason.
	/// </summary>
	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? Reason { get; set; }
}

/// <summary>
///     Represents the raw payload returned by Discord's audit log REST endpoint.
/// </summary>
/// <remarks>
///     This type preserves both documented side collections and currently untyped extension data so the parser layer
///     can produce strongly typed entry families without losing forward compatibility.
/// </remarks>
internal sealed class RawAuditLog : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RawAuditLog"/> class.
	/// </summary>
	/// <remarks>
	///     The ignored properties list matches payload sections currently preserved as raw side collections rather than
	///     being observed through the older abstraction helpers.
	/// </remarks>
	internal RawAuditLog()
		: base(["application_commands", "auto_moderation_rules"])
	{ }

	/// <summary>
	///     Gets or sets the application command side collection.
	/// </summary>
	[JsonProperty("application_commands", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<JObject>? ApplicationCommands { get; set; }

	/// <summary>
	///     Gets or sets the auto moderation rule side collection.
	/// </summary>
	[JsonProperty("auto_moderation_rules", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<JObject>? AutoModerationRules { get; set; }

	/// <summary>
	///     Gets or sets the guild scheduled event side collection.
	/// </summary>
	[JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogGuildScheduledEvent>? GuildScheduledEvents { get; set; }

	/// <summary>
	///     Gets or sets the integration side collection.
	/// </summary>
	[JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogIntegration>? Integrations { get; set; }

	/// <summary>
	///     Gets or sets the thread side collection.
	/// </summary>
	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogThread>? Threads { get; set; }

	/// <summary>
	///     Gets or sets the user side collection.
	/// </summary>
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogUser>? Users { get; set; }

	/// <summary>
	///     Gets or sets the webhook side collection.
	/// </summary>
	[JsonProperty("webhooks", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<RawAuditLogWebhook>? Webhooks { get; set; }

	/// <summary>
	///     Gets or sets the audit log entries returned by Discord.
	/// </summary>
	[JsonProperty("audit_log_entries")]
	public IReadOnlyList<RawAuditLogEntry> Entries { get; set; } = [];
}
