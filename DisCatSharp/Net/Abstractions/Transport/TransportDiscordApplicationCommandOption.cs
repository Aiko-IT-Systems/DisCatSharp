using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommandOption
{
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> NameLocalizations { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> DescriptionLocalizations { get; set; }

	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Required { get; set; }

	[JsonProperty("choices", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandOptionChoice> Choices { get; set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandOption> SubOptions { get; set; }

	[JsonProperty("channel_types", NullValueHandling = NullValueHandling.Ignore)]
	public List<ChannelType> ChannelTypes { get; set; }

	[JsonProperty("min_value", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinValue { get; set; }

	[JsonProperty("max_value", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxValue { get; set; }

	[JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinLength { get; set; }

	[JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxLength { get; set; }

	[JsonProperty("autocomplete", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Autocomplete { get; set; }
}
