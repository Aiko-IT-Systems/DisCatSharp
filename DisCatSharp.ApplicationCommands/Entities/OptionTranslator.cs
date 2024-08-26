using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
///     Represents a option translator.
/// </summary>
public sealed class OptionTranslator
{
	/// <summary>
	///     Gets the option name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	///     Gets the option description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	///     Gets the option type
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; set; }

	/// <summary>
	///     Gets the option name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	public Dictionary<string, string>? NameTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameTranslations
		=> this.NameTranslationsDictionary is not null ? new(this.NameTranslationsDictionary) : null;

	/// <summary>
	///     Gets the option description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	public Dictionary<string, string>? DescriptionTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionTranslations
		=> this.DescriptionTranslationsDictionary is not null ? new(this.DescriptionTranslationsDictionary) : null;

	/// <summary>
	///     Gets the choice translators, if applicable.
	/// </summary>
	[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
	public List<ChoiceTranslator>? Choices { get; set; }

	/// <summary>
	///     Converts a <see cref="DiscordApplicationCommandOption" /> to a <see cref="OptionTranslator" />.
	/// </summary>
	/// <param name="option">The option to convert.</param>
	public static OptionTranslator FromApplicationCommandOption(DiscordApplicationCommandOption option)
	{
		var optionTranslator = new OptionTranslator
		{
			Name = option.Name,
			Description = option.Description,
			Type = option.Type,
			NameTranslationsDictionary = option.RawNameLocalizations,
			DescriptionTranslationsDictionary = option.RawDescriptionLocalizations
		};

		if (option.Choices is not null)
			optionTranslator.Choices = option.Choices.Select(ChoiceTranslator.FromApplicationCommandChoice).ToList();

		return optionTranslator;
	}
}
