using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// Represents a context for a context menu.
/// </summary>
public sealed class ContextMenuContext : BaseContext
{
	/// <summary>
	/// The user this command targets, if applicable.
	/// </summary>
	public DiscordUser TargetUser { get; internal init; }

	/// <summary>
	/// The member this command targets, if applicable.
	/// </summary>
	public DiscordMember TargetMember
		=> this.TargetUser is DiscordMember member ? member : null;

	/// <summary>
	/// The message this command targets, if applicable.
	/// </summary>
	public DiscordMessage TargetMessage { get; internal set; }
}
