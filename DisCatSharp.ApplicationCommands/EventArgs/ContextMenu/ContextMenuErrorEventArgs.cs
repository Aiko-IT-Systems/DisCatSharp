using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.ContextMenuErrored"/>
/// </summary>
public class ContextMenuErrorEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The context of the command.
	/// </summary>
	public ContextMenuContext Context { get; internal set; }

	/// <summary>
	/// The exception thrown.
	/// </summary>
	public Exception Exception { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ContextMenuErrorEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ContextMenuErrorEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
