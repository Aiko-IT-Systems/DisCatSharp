namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common role audit log change keys.
/// </summary>
public sealed class DiscordRoleAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordRoleAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordRoleAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordRoleAuditLogChanges(DiscordRoleAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the role name change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Name
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "name");

	/// <summary>
	///     Gets the role color change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? Color
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "color");

	/// <summary>
	///     Gets the hoist flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Hoist
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "hoist");

	/// <summary>
	///     Gets the mentionable flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Mentionable
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "mentionable");

	/// <summary>
	///     Gets the permissions bitset change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<DisCatSharp.Enums.Permissions?>? Permissions
		=> DiscordAuditLogChangeSetUtilities.GetPermissionsChange(this._entry, "permissions");
}
