using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
///     Represents a group translator.
/// </summary>
public sealed class GroupTranslator
{
	/// <summary>
	///     Gets the group name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	///     Gets the group description.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; set; }

	/// <summary>
	///     Gets the application command type.
	///     Used to determine whether it is an translator for context menu or not.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandType? Type { get; set; }

	/// <summary>
	///     Gets the group name translations.
	/// </summary>
	[JsonProperty("name_translations")]
	public Dictionary<string, string>? NameTranslationsDictionary { get; set; }

	/// <summary>
	///     Gets the group name translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameTranslations
		=> this.NameTranslationsDictionary is not null ? new(this.NameTranslationsDictionary) : null;

	/// <summary>
	///     Gets the group description translations.
	/// </summary>
	[JsonProperty("description_translations")]
	public Dictionary<string, string>? DescriptionTranslationsDictionary { get; set; }

	/// <summary>
	///     Gets the group description translations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionTranslations
		=> this.DescriptionTranslationsDictionary is not null ? new(this.DescriptionTranslationsDictionary) : null;

	/// <summary>
	///     Gets the sub group translators, if applicable.
	/// </summary>
	[JsonProperty("groups", NullValueHandling = NullValueHandling.Ignore)]
	public List<SubGroupTranslator> SubGroups { get; set; }

	/// <summary>
	///     Gets the command translators, if applicable.
	/// </summary>
	[JsonProperty("commands", NullValueHandling = NullValueHandling.Ignore)]
	public List<CommandTranslator> Commands { get; set; }
}
