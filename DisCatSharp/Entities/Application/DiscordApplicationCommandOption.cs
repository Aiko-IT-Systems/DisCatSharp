using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a parameter for a <see cref="DiscordApplicationCommand"/>.
/// </summary>
public class DiscordApplicationCommandOption
{
	/// <summary>
	/// Gets the type of this command parameter.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; internal set; }

	/// <summary>
	/// Gets the name of this command parameter.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Sets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string>? RawNameLocalizations { get; set; }

	/// <summary>
	/// Gets the name localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? NameLocalizations
		=> this.RawNameLocalizations is not null ? new(this.RawNameLocalizations) : null;

	/// <summary>
	/// Gets the description of this command parameter.
	/// </summary>
	[JsonProperty("description")]
	public string Description { get; internal set; }

	/// <summary>
	/// Sets the description localizations.
	/// </summary>
	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string>? RawDescriptionLocalizations { get; set; }

	/// <summary>
	/// Gets the description localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization? DescriptionLocalizations
		=> this.RawDescriptionLocalizations is not null ? new(this.RawDescriptionLocalizations) : null;

	/// <summary>
	/// Gets whether this command parameter is required.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; } = false;

	/// <summary>
	/// Gets the optional choices for this command parameter.
	/// Not applicable for auto-complete options.
	/// </summary>
	[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordApplicationCommandOptionChoice>? Choices { get; internal set; } = null;

	/// <summary>
	/// Gets the optional subcommand parameters for this parameter.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordApplicationCommandOption>? Options { get; internal set; } = null;

	/// <summary>
	/// Gets the optional allowed channel types.
	/// </summary>
	[JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
	public List<ChannelType>? ChannelTypes { get; internal set; } = null;

	/// <summary>
	/// Gets whether this option provides autocompletion.
	/// </summary>
	[JsonProperty("autocomplete", NullValueHandling = NullValueHandling.Ignore)]
	public bool AutoComplete { get; internal set; } = false;

	/// <summary>
	/// Gets the minimum value for this slash command parameter.
	/// </summary>
	[JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
	public object? MinimumValue { get; internal set; }

	/// <summary>
	/// Gets the maximum value for this slash command parameter.
	/// </summary>
	[JsonProperty("max_value", NullValueHandling = NullValueHandling.Ignore)]
	public object? MaximumValue { get; internal set; }

	/// <summary>
	/// Gets the maximum length for this slash command parameter.
	/// </summary>
	[JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinimumLength { get; internal set; }

	/// <summary>
	/// Gets the minimum length for this slash command parameter.
	/// </summary>
	[JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaximumLength { get; internal set; }

	/// <summary>
	/// Creates a new instance of a <see cref="DiscordApplicationCommandOption"/>.
	/// </summary>
	/// <param name="name">The name of this parameter.</param>
	/// <param name="description">The description of the parameter.</param>
	/// <param name="type">The type of this parameter.</param>
	/// <param name="required">Whether the parameter is required.</param>
	/// <param name="choices">The optional choice selection for this parameter.</param>
	/// <param name="options">The optional subcommands for this parameter.</param>
	/// <param name="channelTypes">If the option is a channel type, the channels shown will be restricted to these types.</param>
	/// <param name="autocomplete">Whether this option provides autocompletion.</param>
	/// <param name="minimumValue">The minimum value for this parameter. Only valid for types <see cref="ApplicationCommandOptionType.Integer"/> or <see cref="ApplicationCommandOptionType.Number"/>.</param>
	/// <param name="maximumValue">The maximum value for this parameter. Only valid for types <see cref="ApplicationCommandOptionType.Integer"/> or <see cref="ApplicationCommandOptionType.Number"/>.</param>
	/// <param name="nameLocalizations">The localizations of the parameter name.</param>
	/// <param name="descriptionLocalizations">The localizations of the parameter description.</param>
	/// <param name="minimumLength">The minimum allowed length of the string. (Min 0)</param>
	/// <param name="maximumLength">The maximum allowed length of the string. (Min 1)</param>
	public DiscordApplicationCommandOption(
		string name,
		string description,
		ApplicationCommandOptionType type,
		bool required = false,
		IEnumerable<DiscordApplicationCommandOptionChoice>? choices = null,
		IEnumerable<DiscordApplicationCommandOption>? options = null,
		IEnumerable<ChannelType>? channelTypes = null,
		bool autocomplete = false,
		object? minimumValue = null,
		object? maximumValue = null,
		DiscordApplicationCommandLocalization? nameLocalizations = null,
		DiscordApplicationCommandLocalization? descriptionLocalizations = null,
		int? minimumLength = null,
		int? maximumLength = null
	)
	{
		if (!Utilities.IsValidSlashCommandName(name))
			throw new ArgumentException("Invalid application command option name specified. It must be below 32 characters and not contain any whitespace.", nameof(name));
		if (name.Any(char.IsUpper))
			throw new ArgumentException("Application command option name cannot have any upper case characters.", nameof(name));
		if (description.Length > 100)
			throw new ArgumentException("Application command option description cannot exceed 100 characters.", nameof(description));
		if (autocomplete && (choices?.Any() ?? false))
			throw new InvalidOperationException("Auto-complete slash command options cannot provide choices.");

		if (type is ApplicationCommandOptionType.SubCommand or ApplicationCommandOptionType.SubCommandGroup)
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Slash commands need a description.", nameof(description));

		this.Name = name;
		this.Description = description;
		this.Type = type;
		this.Required = required;
		this.Choices = choices is not null && choices.Any() ? choices.ToList() : null;
		this.Options = options is not null && options.Any() ? options.ToList() : null;
		this.ChannelTypes = channelTypes is not null && channelTypes.Any() ? channelTypes.ToList() : null;
		this.AutoComplete = autocomplete;
		this.MinimumValue = minimumValue;
		this.MaximumValue = maximumValue;
		this.MinimumLength = minimumLength;
		this.MaximumLength = maximumLength;
		this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
		this.RawDescriptionLocalizations = descriptionLocalizations?.GetKeyValuePairs();
	}

	/// <summary>
	/// Creates a new empty DiscordApplicationCommandOption.
	/// </summary>
	internal DiscordApplicationCommandOption()
	{ }
}
