namespace DisCatSharp.Enums.Core;

/// <summary>
/// Represents the grouping type of a command.
/// </summary>
public enum DisCatSharpCommandGroupingType
{
	/// <summary>
	/// This is a special type and not a command.
	/// </summary>
	None,

	/// <summary>
	/// The command is not part of a group.
	/// </summary>
	Command,

	/// <summary>
	/// The command is a sub command.
	/// </summary>
	SubCommand,

	/// <summary>
	/// The command is a sub group command.
	/// </summary>
	SubGroupCommand
}
