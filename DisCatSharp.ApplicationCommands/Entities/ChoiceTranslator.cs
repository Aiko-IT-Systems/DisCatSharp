using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
///     Represents a choice translator.
/// </summary>
public sealed class ChoiceTranslator
{
	/// <summary>
	///     Gets the choice name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	///     Gets the choice name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	public Dictionary<string, string>? NameTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameTranslations
		=> this.NameTranslationsDictionary is not null ? new(this.NameTranslationsDictionary) : null;

	/// <summary>
	///     Converts a <see cref="DiscordApplicationCommandOptionChoice" /> to a <see cref="ChoiceTranslator" />.
	/// </summary>
	/// <param name="choice">The choice to convert.</param>
	public static ChoiceTranslator FromApplicationCommandChoice(DiscordApplicationCommandOptionChoice choice)
	{
		var translator = new ChoiceTranslator
		{
			Name = choice.Name,
			NameTranslationsDictionary = choice.RawNameLocalizations
		};

		return translator;
	}
}
