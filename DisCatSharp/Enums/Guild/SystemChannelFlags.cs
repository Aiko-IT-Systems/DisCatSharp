using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a system channel flags extension.
/// </summary>
public static class SystemChannelFlagsExtension
{
	/// <summary>
	///     Calculates whether these system channel flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flag to search for.</param>
	/// <returns></returns>
	public static bool HasSystemChannelFlag(this SystemChannelFlags baseFlags, SystemChannelFlags flag)
		=> (baseFlags & flag) == flag;

	/// <inheritdoc cref="HasSystemChannelFlag(SystemChannelFlags, SystemChannelFlags)" />
	public static bool HasSystemChannelFlag(this SystemChannelFlags? baseFlags, SystemChannelFlags flag)
		=> baseFlags.HasValue && (baseFlags & flag) == flag;
}

/// <summary>
///     Represents settings for a guild's system channel.
/// </summary>
[Flags]
public enum SystemChannelFlags : long
{
	/// <summary>
	///     Member join messages are disabled.
	/// </summary>
	SuppressJoinNotifications = 1L << 0,

	/// <summary>
	///     Server boost messages are disabled.
	/// </summary>
	SuppressPremiumSubscriptions = 1L << 1,

	/// <summary>
	///     Server setup tips are disabled.
	/// </summary>
	SuppressGuildReminderNotifications = 1L << 2,

	/// <summary>
	///     Suppress member join sticker replies.
	/// </summary>
	SuppressJoinNotificationReplies = 1L << 3,

	/// <summary>
	///     Role subscription purchase messages are disabled.
	/// </summary>
	SuppressRoleSubbscriptionPurchaseNotification = 1L << 4,

	/// <summary>
	///     Suppress role subscription purchase sticker replies.
	/// </summary>
	SuppressRoleSubbscriptionPurchaseNotificationReplies = 1L << 5,

	/// <summary>
	///     Suppress deadchat channel prompts.
	/// </summary>
	SuppressChannelPromptDeadchat = 1L << 7,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
