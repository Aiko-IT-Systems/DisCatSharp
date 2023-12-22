using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a command translator.
/// </summary>
internal sealed class CommandTranslator
{
	/// <summary>
	/// Gets the command name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the command description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets the application command type.
	/// Used to determine whether it is an translator for context menu or not.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandType? Type { get; set; }

	/// <summary>
	/// Gets the command name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	internal Dictionary<string, string> NameTranslationDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameTranslations
		=> new(this.NameTranslationDictionary);

	/// <summary>
	/// Gets the command description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	internal Dictionary<string, string> DescriptionTranslationDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization DescriptionTranslations
		=> new(this.DescriptionTranslationDictionary);

	/// <summary>
	/// Gets the option translators, if applicable.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<OptionTranslator> Options { get; set; }
}
