using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a sub group translator.
/// </summary>
internal sealed class SubGroupTranslator
{
	/// <summary>
	/// Gets the sub group name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets the sub group description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	/// Gets the sub group name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	internal Dictionary<string, string> NameTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameTranslations
		=> new(this.NameTranslationsDictionary);

	/// <summary>
	/// Gets the sub group description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	internal Dictionary<string, string> DescriptionTranslationsDictionary { get; set; }

	[JsonIgnore]
	public DiscordApplicationCommandLocalization DescriptionTranslations
		=> new(this.DescriptionTranslationsDictionary);

	/// <summary>
	/// Gets the command translators.
	/// </summary>
	[JsonProperty("commands", NullValueHandling = NullValueHandling.Ignore)]
	public List<CommandTranslator> Commands { get; set; }
}
