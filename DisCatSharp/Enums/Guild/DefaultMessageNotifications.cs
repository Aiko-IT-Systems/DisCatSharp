namespace DisCatSharp.Enums;

/// <summary>
/// Represents default notification level for a guild.
/// </summary>
public enum DefaultMessageNotifications
{
	/// <summary>
	/// All messages will trigger push notifications.
	/// </summary>
	AllMessages = 0,

	/// <summary>
	/// Only messages that mention the user (or a role he's in) will trigger push notifications.
	/// </summary>
	MentionsOnly = 1
}
