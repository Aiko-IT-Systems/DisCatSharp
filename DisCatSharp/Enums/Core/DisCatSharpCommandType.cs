namespace DisCatSharp.Enums.Core;

/// <summary>
/// Represents the type of a command.
/// </summary>
public enum DisCatSharpCommandType
{
	/// <summary>
	/// A text command.
	/// </summary>
	TextCommand,

	/// <summary>
	/// A slash command.
	/// </summary>
	SlashCommand,

	/// <summary>
	/// A user context menu command.
	/// </summary>
	UserCommand,

	/// <summary>
	/// A message context menu command.
	/// </summary>
	MessageCommand,

	/// <summary>
	/// A special type.
	/// </summary>
	Special
}
