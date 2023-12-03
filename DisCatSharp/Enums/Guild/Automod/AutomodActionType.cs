namespace DisCatSharp.Enums;

/// <summary>
/// Represent's a rule's action type.
/// </summary>
public enum AutomodActionType
{
	/// <summary>
	/// Blocks the content of a message according to the rule.
	/// </summary>
	BlockMessage = 1,

	/// <summary>
	/// Logs user to a specified channel.
	/// </summary>
	SendAlertMessage = 2,

	/// <summary>
	/// Timeout user for a specified duration.
	/// Only valid for Keyword and MentionSpam rules
	/// </summary>
	Timeout = 3
}
