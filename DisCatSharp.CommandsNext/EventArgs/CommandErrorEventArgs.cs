using System;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Represents arguments for <see cref="CommandsNextExtension.CommandErrored"/> event.
/// </summary>
public class CommandErrorEventArgs : CommandEventArgs
{
	/// <summary>
	/// Gets the exception.
	/// </summary>
	public Exception Exception { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandErrorEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public CommandErrorEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
