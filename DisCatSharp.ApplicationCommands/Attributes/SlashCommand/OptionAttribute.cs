using System;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Marks this parameter as an option for a slash command
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute : Attribute
{
	/// <summary>
	/// Gets the name of this option.
	/// </summary>
	public string Name;

	/// <summary>
	/// Gets the description of this option.
	/// </summary>
	public string Description;

	/// <summary>
	/// Whether to autocomplete this option.
	/// </summary>
	public bool Autocomplete;

	/// <summary>
	/// Initializes a new instance of the <see cref="OptionAttribute"/> class.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="autocomplete">If true, autocomplete.</param>
	public OptionAttribute(string name, string description, bool autocomplete = false)
	{
		if (name.Length > 32)
			throw new ArgumentException("Slash command option names cannot go over 32 characters.");
		else if (description.Length > 100)
			throw new ArgumentException("Slash command option descriptions cannot go over 100 characters.");

		this.Name = name.ToLower();
		this.Description = description;
		this.Autocomplete = autocomplete;
	}
}
