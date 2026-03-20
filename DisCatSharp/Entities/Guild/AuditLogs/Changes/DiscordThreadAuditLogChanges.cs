namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common thread audit log change keys.
/// </summary>
public sealed class DiscordThreadAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordThreadAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordThreadAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordThreadAuditLogChanges(DiscordThreadAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the thread name change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Name
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "name");

	/// <summary>
	///     Gets the archived flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Archived
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "archived");

	/// <summary>
	///     Gets the locked flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Locked
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "locked");

	/// <summary>
	///     Gets the auto archive duration change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? AutoArchiveDuration
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "auto_archive_duration");

	/// <summary>
	///     Gets the applied tag id collection change, if present.
	/// </summary>
	/// <remarks>
	///     Discord emits tag ids as arrays of strings in current payloads, so this helper keeps the raw string form.
	/// </remarks>
	public DiscordAuditLogCollectionChange<string>? AppliedTags
		=> DiscordAuditLogChangeSetUtilities.GetStringCollectionChange(this._entry, "applied_tags");

	/// <summary>
	///     Gets the invitable flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Invitable
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "invitable");

	/// <summary>
	///     Gets the per-user slowmode change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? RateLimitPerUser
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "rate_limit_per_user");

	/// <summary>
	///     Gets the raw flags change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? Flags
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "flags");
}
