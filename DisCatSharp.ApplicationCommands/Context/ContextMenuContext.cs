using DisCatSharp.Entities;
using DisCatSharp.Enums.Core;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// Represents a context for a context menu.
/// </summary>
public sealed class ContextMenuContext(DisCatSharpCommandType type) : BaseContext(type)
{
	/// <summary>
	/// The user this command targets, if applicable.
	/// </summary>
	public DiscordUser TargetUser { get; internal init; }

	/// <summary>
	/// The member this command targets, if applicable.
	/// </summary>
	public DiscordMember? TargetMember
		=> this.TargetUser is DiscordMember member ? member : null;

	/// <summary>
	/// The message this command targets, if applicable.
	/// </summary>
	public DiscordMessage TargetMessage { get; internal set; }
}
