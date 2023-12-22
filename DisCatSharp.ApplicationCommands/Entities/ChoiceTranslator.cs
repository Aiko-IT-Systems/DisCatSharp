using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a choice translator.
/// </summary>
internal sealed class ChoiceTranslator
{
	/// <summary>
	/// Gets the choice name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the choice name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	internal Dictionary<string, string> NameTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameTranslations
		=> new(this.NameTranslationsDictionary);
}
