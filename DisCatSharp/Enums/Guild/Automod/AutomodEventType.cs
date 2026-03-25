namespace DisCatSharp.Enums;

/// <summary>
///     Represent's a rule's flags.
/// </summary>
public enum AutomodEventType
{
	/// <summary>
	///     Indicates that this rule should be checked when a member sends or edits a message in the guild.
	/// </summary>
	MessageSend = 1,

	/// <summary>
	///     Indicates that this rule should be checked when a member joins or updates their profile.
	/// </summary>
	GuildMemberEvent = 2
}
