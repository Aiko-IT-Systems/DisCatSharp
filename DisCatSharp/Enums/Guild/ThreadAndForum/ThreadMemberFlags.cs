namespace DisCatSharp.Enums;

/// <summary>
/// Represents notification settings for a thread.
/// </summary>
public enum ThreadMemberFlags
{
	/// <summary>
	/// Indicates that the notification setting is set to has interacted.
	/// </summary>
	HasInteracted = 1,

	/// <summary>
	/// Indicates that the notification setting is set to all messages.
	/// </summary>
	AllMessages = 2,

	/// <summary>
	/// Indicates that the notification setting is set to only mentions.
	/// </summary>
	OnlyMentions = 4,

	/// <summary>
	/// Indicates that the notification setting is set to none.
	/// </summary>
	None = 8
}
