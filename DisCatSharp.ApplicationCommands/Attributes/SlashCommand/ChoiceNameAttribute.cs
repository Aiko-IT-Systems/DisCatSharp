using System;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Sets the name for this enum choice.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class ChoiceNameAttribute : Attribute
{
	/// <summary>
	///     Sets the name for this enum choice.
	/// </summary>
	/// <param name="name">The name for this enum choice.</param>
	public ChoiceNameAttribute(string name)
	{
		this.Name = name;
	}

	/// <summary>
	///     The name.
	/// </summary>
	public string Name { get; set; }
}
