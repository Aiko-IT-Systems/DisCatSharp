using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Represents a sub group translator.
/// </summary>
public sealed class SubGroupTranslator
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
	public Dictionary<string, string>? NameTranslationsDictionary { get; set; }

	/// <summary>
	/// Gets the sub group name translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameTranslations
		=> this.NameTranslationsDictionary is not null ? new(this.NameTranslationsDictionary) : null;

	/// <summary>
	/// Gets the sub group description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	public Dictionary<string, string>? DescriptionTranslationsDictionary { get; set; }

	/// <summary>
	/// Gets the sub group description translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionTranslations
		=> this.DescriptionTranslationsDictionary is not null ? new(this.DescriptionTranslationsDictionary) : null;

	/// <summary>
	/// Gets the command translators.
	/// </summary>
	[JsonProperty("commands", NullValueHandling = NullValueHandling.Ignore)]
	public List<CommandTranslator> Commands { get; set; }
}
