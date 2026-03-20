namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common guild audit log change keys.
/// </summary>
public sealed class DiscordGuildAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordGuildAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuildAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordGuildAuditLogChanges(DiscordGuildAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the guild name change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Name
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "name");

	/// <summary>
	///     Gets the guild description change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Description
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "description");

	/// <summary>
	///     Gets the owner id change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<ulong?>? OwnerId
		=> DiscordAuditLogChangeSetUtilities.GetSnowflakeChange(this._entry, "owner_id");

	/// <summary>
	///     Gets the verification level change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<DisCatSharp.Enums.VerificationLevel?>? VerificationLevel
		=> DiscordAuditLogChangeSetUtilities.GetVerificationLevelChange(this._entry, "verification_level");

	/// <summary>
	///     Gets the preferred locale change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? PreferredLocale
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "preferred_locale");

	/// <summary>
	///     Gets the vanity invite code change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? VanityUrlCode
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "vanity_url_code");

	/// <summary>
	///     Gets the rules channel id change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<ulong?>? RulesChannelId
		=> DiscordAuditLogChangeSetUtilities.GetSnowflakeChange(this._entry, "rules_channel_id");

	/// <summary>
	///     Gets the public updates channel id change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<ulong?>? PublicUpdatesChannelId
		=> DiscordAuditLogChangeSetUtilities.GetSnowflakeChange(this._entry, "public_updates_channel_id");
}
