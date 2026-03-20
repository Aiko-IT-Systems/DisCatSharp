using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the Discord <c>options</c> object on an audit log entry.
/// </summary>
/// <remarks>
///     Discord only includes this object for specific action types. Some actions rely on it more heavily than on
///     <see cref="DiscordAuditLogEntry.Changes" />, especially message, overwrite, auto moderation, and internal
///     system entries.
/// </remarks>
public sealed class DiscordAuditLogEntryOptions
{
	/// <summary>
	///     Gets the targeted application id.
	/// </summary>
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	///     Gets the triggered auto moderation rule name.
	/// </summary>
	public string? AutoModerationRuleName { get; internal set; }

	/// <summary>
	///     Gets the triggered auto moderation rule trigger type.
	/// </summary>
	public string? AutoModerationRuleTriggerType { get; internal set; }

	/// <summary>
	///     Gets the targeted channel id.
	/// </summary>
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	///     Gets the targeted entity count.
	/// </summary>
	public int? Count { get; internal set; }

	/// <summary>
	///     Gets the prune delete-member-days value.
	/// </summary>
	public int? DeleteMemberDays { get; internal set; }

	/// <summary>
	///     Gets the event exception id.
	/// </summary>
	public ulong? EventExceptionId { get; internal set; }

	/// <summary>
	///     Gets the overwritten entity id.
	/// </summary>
	public ulong? Id { get; internal set; }

	/// <summary>
	///     Gets the members removed count.
	/// </summary>
	public int? MembersRemoved { get; internal set; }

	/// <summary>
	///     Gets the targeted message id.
	/// </summary>
	public ulong? MessageId { get; internal set; }

	/// <summary>
	///     Gets the overwrite role name.
	/// </summary>
	public string? RoleName { get; internal set; }

	/// <summary>
	///     Gets the voice channel status value.
	/// </summary>
	public string? Status { get; internal set; }

	/// <summary>
	///     Gets the Discord type string.
	/// </summary>
	public string? Type { get; internal set; }

	/// <summary>
	///     Gets the integration type.
	/// </summary>
	public string? IntegrationType { get; internal set; }

	/// <summary>
	///     Gets the raw JSON object.
	/// </summary>
	/// <remarks>
	///     Unknown option fields are preserved here so callers can inspect them even before DisCatSharp adds a typed
	///     property.
	/// </remarks>
	public JObject RawObject { get; internal set; } = [];
}
