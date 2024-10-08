using System;
using System.Linq;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
///     Marks this method as a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class CommandAttribute : Attribute
{
	/// <summary>
	///     Marks this method as a command, using the method's name as command name.
	/// </summary>
	public CommandAttribute()
	{
		this.Name = null;
	}

	/// <summary>
	///     Marks this method as a command with specified name.
	/// </summary>
	/// <param name="name">Name of this command.</param>
	public CommandAttribute(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name), "Command names cannot be null, empty, or all-whitespace.");

		if (name.Any(xc => char.IsWhiteSpace(xc)))
			throw new ArgumentException("Command names cannot contain whitespace characters.", nameof(name));

		this.Name = name;
	}

	/// <summary>
	///     Gets the name of this command.
	/// </summary>
	public string Name { get; }
}

/// <summary>
///     Marks this method as a group command.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GroupCommandAttribute : Attribute
{
	/// <summary>
	///     Marks this method as a group command.
	/// </summary>
	public GroupCommandAttribute()
	{ }
}
