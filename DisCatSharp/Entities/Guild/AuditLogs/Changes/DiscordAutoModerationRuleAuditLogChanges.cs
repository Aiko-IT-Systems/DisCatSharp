namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common auto moderation audit log change keys.
/// </summary>
public sealed class DiscordAutoModerationRuleAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordAutoModerationRuleAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordAutoModerationRuleAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordAutoModerationRuleAuditLogChanges(DiscordAutoModerationRuleAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the rule name change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Name
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "name");

	/// <summary>
	///     Gets the enabled flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Enabled
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "enabled");

	/// <summary>
	///     Gets the event type change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? EventType
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "event_type");

	/// <summary>
	///     Gets the trigger type change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? TriggerType
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "trigger_type");

	/// <summary>
	///     Gets the exempt role id collection change, if present.
	/// </summary>
	public DiscordAuditLogCollectionChange<ulong>? ExemptRoles
		=> DiscordAuditLogChangeSetUtilities.GetSnowflakeCollectionChange(this._entry, "exempt_roles");

	/// <summary>
	///     Gets the exempt channel id collection change, if present.
	/// </summary>
	public DiscordAuditLogCollectionChange<ulong>? ExemptChannels
		=> DiscordAuditLogChangeSetUtilities.GetSnowflakeCollectionChange(this._entry, "exempt_channels");
}
