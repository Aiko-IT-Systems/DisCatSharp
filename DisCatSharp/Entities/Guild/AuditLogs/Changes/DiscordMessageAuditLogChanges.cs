namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common message audit log change keys.
/// </summary>
public sealed class DiscordMessageAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordMessageAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordMessageAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordMessageAuditLogChanges(DiscordMessageAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the raw message flags change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? Flags
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "flags");

	/// <summary>
	///     Gets the pinned state change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Pinned
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "pinned");

	/// <summary>
	///     Gets the message content change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Content
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "content");
}
