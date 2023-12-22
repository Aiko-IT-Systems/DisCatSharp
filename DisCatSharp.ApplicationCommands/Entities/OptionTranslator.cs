using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a option translator.
/// </summary>
internal sealed class OptionTranslator
{
	/// <summary>
	/// Gets the option name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the option description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	/// Gets the option type
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType? Type { get; set; }

	/// <summary>
	/// Gets the option name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	internal Dictionary<string, string> NameTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameTranslations
		=> new(this.NameTranslationsDictionary);

	/// <summary>
	/// Gets the option description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	internal Dictionary<string, string> DescriptionTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization DescriptionTranslations
		=> new(this.DescriptionTranslationsDictionary);

	/// <summary>
	/// Gets the choice translators, if applicable.
	/// </summary>
	[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
	public List<ChoiceTranslator> Choices { get; set; }
}
