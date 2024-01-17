using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a command translator.
/// </summary>
public sealed class CommandTranslator
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
	public Dictionary<string, string>? NameTranslationsDictionary { get; set; }

	/// <summary>
	/// Gets the command name translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameTranslations
		=> this.NameTranslationsDictionary is not null ? new(this.NameTranslationsDictionary) : null;

	/// <summary>
	/// Gets the command description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	public Dictionary<string, string>? DescriptionTranslationsDictionary { get; set; }

	/// <summary>
	/// Gets the command description translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionTranslations
		=> this.DescriptionTranslationsDictionary is not null ? new(this.DescriptionTranslationsDictionary) : null;

	/// <summary>
	/// Gets the option translators, if applicable.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<OptionTranslator>? Options { get; set; }
}
