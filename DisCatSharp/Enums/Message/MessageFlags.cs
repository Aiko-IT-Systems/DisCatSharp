using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a message flag extensions.
/// </summary>
public static class MessageFlagExtensions
{
	/// <summary>
	/// Calculates whether these message flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasMessageFlag(this MessageFlags baseFlags, MessageFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents additional features of a message.
/// </summary>
[Flags]
public enum MessageFlags
{
	/// <summary>
	/// This message has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// Whether this message is the original message that was published from a news channel to subscriber channels.
	/// </summary>
	Crossposted = 1 << 0,

	/// <summary>
	/// Whether this message is crossposted (automatically posted in a subscriber channel).
	/// </summary>
	IsCrosspost = 1 << 1,

	/// <summary>
	/// Whether any embeds in the message are hidden.
	/// </summary>
	SuppressedEmbeds = 1 << 2,

	/// <summary>
	/// The source message for this crosspost has been deleted.
	/// </summary>
	SourceMessageDelete = 1 << 3,

	/// <summary>
	/// The message came from the urgent message system.
	/// </summary>
	Urgent = 1 << 4,

	/// <summary>
	/// The message has an associated thread, with the same id as the message.
	/// </summary>
	HasThread = 1 << 5,

	/// <summary>
	/// The message is only visible to the user who invoked the interaction.
	/// </summary>
	Ephemeral = 1 << 6,

	/// <summary>
	/// The message is an interaction response and the bot is "thinking".
	/// </summary>
	Loading = 1 << 7,

	/// <summary>
	/// The message is warning that some roles failed to mention in thread.
	/// </summary>
	FailedToMentionSomeRolesInThread = 1 << 8,

	/// <summary>
	/// The message contains a link marked as potential dangerous or absusive.
	/// </summary>
	ShouldShowLinkNotDiscordWarning = 1 << 10,

	/// <summary>
	/// The message suppresses channel notifications.
	/// Aka. new message indicator.
	/// </summary>
	SuppressNotifications = 1 << 12,

	/// <summary>
	/// The message is a voice message.
	/// </summary>
	IsVoiceMessage = 1 << 13
}
