using System;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Represents arguments for <see cref="CommandsNextExtension.CommandExecuted"/> event.
/// </summary>
public class CommandExecutionEventArgs : CommandEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CommandExecutionEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public CommandExecutionEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
