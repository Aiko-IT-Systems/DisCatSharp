namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common member audit log change keys.
/// </summary>
public sealed class DiscordMemberAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordMemberAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMemberAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordMemberAuditLogChanges(DiscordMemberAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the nickname change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Nickname
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "nick");

	/// <summary>
	///     Gets the timeout-until change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<System.DateTimeOffset?>? CommunicationDisabledUntil
		=> DiscordAuditLogChangeSetUtilities.GetDateTimeOffsetChange(this._entry, "communication_disabled_until");

	/// <summary>
	///     Gets the raw member flags change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? Flags
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "flags");

	/// <summary>
	///     Gets the bypasses-verification flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? BypassesVerification
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "bypasses_verification");

	/// <summary>
	///     Gets the typed partial role collection added to the member, if present.
	/// </summary>
	public DiscordAuditLogCollectionChange<DiscordAuditLogPartialRole>? AddedRoles
		=> DiscordAuditLogChangeSetUtilities.GetPartialRoleCollectionChange(this._entry, "$add");

	/// <summary>
	///     Gets the typed partial role collection removed from the member, if present.
	/// </summary>
	public DiscordAuditLogCollectionChange<DiscordAuditLogPartialRole>? RemovedRoles
		=> DiscordAuditLogChangeSetUtilities.GetPartialRoleCollectionChange(this._entry, "$remove");
}
