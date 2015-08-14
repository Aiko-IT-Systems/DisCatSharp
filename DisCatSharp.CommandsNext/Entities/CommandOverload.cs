using System;
using System.Collections.Generic;

namespace DisCatSharp.CommandsNext;

/// <summary>
/// Represents a specific overload of a command.
/// </summary>
public sealed class CommandOverload
{
	/// <summary>
	/// Gets this command overload's arguments.
	/// </summary>
	public IReadOnlyList<CommandArgument> Arguments { get; internal set; }

	/// <summary>
	/// Gets this command overload's priority.
	/// </summary>
	public int Priority { get; internal set; }

	/// <summary>
	/// Gets this command overload's delegate.
	/// </summary>
	internal Delegate Callable { get; set; }

	/// <summary>
	/// Gets or sets the invocation target.
	/// </summary>
	internal object InvocationTarget { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CommandOverload"/> class.
	/// </summary>
	internal CommandOverload()
	{ }
}
