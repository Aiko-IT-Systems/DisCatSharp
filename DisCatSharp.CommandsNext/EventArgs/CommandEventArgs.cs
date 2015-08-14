using System;

using DisCatSharp.EventArgs;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Base class for all CNext-related events.
/// </summary>
public class CommandEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the context in which the command was executed.
	/// </summary>
	public CommandContext Context { get; internal set; }

	/// <summary>
	/// Gets the command that was executed.
	/// </summary>
	public Command Command
		=> this.Context.Command;

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public CommandEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
