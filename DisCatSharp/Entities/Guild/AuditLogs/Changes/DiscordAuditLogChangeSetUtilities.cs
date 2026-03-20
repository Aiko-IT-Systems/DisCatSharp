using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Provides shared helper methods for audit log change set wrappers.
/// </summary>
internal static class DiscordAuditLogChangeSetUtilities
{
	/// <summary>
	///     Gets a typed string change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<string>? GetStringChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToValueChange<string>();

	/// <summary>
	///     Gets a typed boolean change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<bool>? GetBooleanChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToValueChange<bool>();

	/// <summary>
	///     Gets a typed integer change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<int?>? GetInt32Change(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToInt32Change();

	/// <summary>
	///     Gets a typed snowflake change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<ulong?>? GetSnowflakeChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToSnowflakeChange();

	/// <summary>
	///     Gets a typed timestamp change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<System.DateTimeOffset?>? GetDateTimeOffsetChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToDateTimeOffsetChange();

	/// <summary>
	///     Gets a typed permissions change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<Permissions?>? GetPermissionsChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToPermissionsChange();

	/// <summary>
	///     Gets a typed verification-level change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogValueChange<VerificationLevel?>? GetVerificationLevelChange(DiscordAuditLogEntry entry, string key)
	{
		var change = entry.GetChange(key);
		return change is null
			? null
			: new(change.HasOldValue, change.GetOldEnum<VerificationLevel>(), change.HasNewValue, change.GetNewEnum<VerificationLevel>());
	}

	/// <summary>
	///     Gets a typed string collection change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogCollectionChange<string>? GetStringCollectionChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToCollectionChange<string>();

	/// <summary>
	///     Gets a typed snowflake collection change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogCollectionChange<ulong>? GetSnowflakeCollectionChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToSnowflakeCollectionChange();

	/// <summary>
	///     Gets a typed partial role collection change for the given Discord key.
	/// </summary>
	/// <param name="entry">The entry to inspect.</param>
	/// <param name="key">The Discord change key.</param>
	/// <returns>The typed change when the key exists; otherwise <see langword="null" />.</returns>
	internal static DiscordAuditLogCollectionChange<DiscordAuditLogPartialRole>? GetPartialRoleCollectionChange(DiscordAuditLogEntry entry, string key)
		=> entry.GetChange(key)?.ToPartialRoleCollectionChange();
}
