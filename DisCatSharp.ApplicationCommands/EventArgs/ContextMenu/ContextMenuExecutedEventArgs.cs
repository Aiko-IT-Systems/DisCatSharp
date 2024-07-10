using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents the arguments for a <see cref="ApplicationCommandsExtension.ContextMenuExecuted"/> event
/// </summary>
public sealed class ContextMenuExecutedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The context of the command.
	/// </summary>
	public ContextMenuContext Context { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ContextMenuExecutedEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ContextMenuExecutedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
