using System;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
///     Gives this command, group, or argument a description, which is used when listing help.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter)]
public sealed class DescriptionAttribute : Attribute
{
	/// <summary>
	///     Gives this command, group, or argument a description, which is used when listing help.
	/// </summary>
	/// <param name="description"></param>
	public DescriptionAttribute(string description)
	{
		this.Description = description;
	}

	/// <summary>
	///     Gets the description for this command, group, or argument.
	/// </summary>
	public string Description { get; }
}
