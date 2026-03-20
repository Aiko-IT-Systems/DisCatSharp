namespace DisCatSharp.Entities;

/// <summary>
///     Provides typed accessors for common channel audit log change keys.
/// </summary>
public sealed class DiscordChannelAuditLogChanges
{
	/// <summary>
	///    The owning audit log entry.
	/// </summary>
	private readonly DiscordChannelAuditLogEntry _entry;

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordChannelAuditLogChanges" /> class.
	/// </summary>
	/// <param name="entry">The owning audit log entry.</param>
	internal DiscordChannelAuditLogChanges(DiscordChannelAuditLogEntry entry)
	{
		this._entry = entry;
	}

	/// <summary>
	///     Gets the channel name change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Name
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "name");

	/// <summary>
	///     Gets the channel topic change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? Topic
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "topic");

	/// <summary>
	///     Gets the channel bitrate change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? Bitrate
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "bitrate");

	/// <summary>
	///     Gets the NSFW flag change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<bool>? Nsfw
		=> DiscordAuditLogChangeSetUtilities.GetBooleanChange(this._entry, "nsfw");

	/// <summary>
	///     Gets the per-user slowmode change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? RateLimitPerUser
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "rate_limit_per_user");

	/// <summary>
	///     Gets the default thread auto archive duration change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<int?>? DefaultAutoArchiveDuration
		=> DiscordAuditLogChangeSetUtilities.GetInt32Change(this._entry, "default_auto_archive_duration");

	/// <summary>
	///     Gets the RTC region change, if present.
	/// </summary>
	public DiscordAuditLogValueChange<string>? RtcRegion
		=> DiscordAuditLogChangeSetUtilities.GetStringChange(this._entry, "rtc_region");
}
