using System;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// The application commands translation context.
/// </summary>
public sealed class ApplicationCommandsTranslationContext
{
	/// <summary>
	/// Gets the type.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the group translation json.
	/// </summary>
	internal string? GroupTranslations { get; set; }

	/// <summary>
	/// Gets the single translation json.
	/// </summary>
	internal string? SingleTranslations { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandsTranslationContext"/> class.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="name">The name.</param>
	internal ApplicationCommandsTranslationContext(Type type, string name)
	{
		this.Type = type;
		this.Name = name;
	}

	/// <summary>
	/// Adds the group translation.
	/// </summary>
	/// <param name="translationJson"></param>
	public void AddGroupTranslation(string translationJson)
		=> this.GroupTranslations = translationJson;

	/// <summary>
	/// Adds the single translation.
	/// </summary>
	/// <param name="translationJson"></param>
	public void AddSingleTranslation(string translationJson)
		=> this.SingleTranslations = translationJson;
}
