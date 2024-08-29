using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommandInteractionDataOption
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandOptionType Type { get; internal set; }

	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public object Value { get; internal set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandInteractionDataOption> Options { get; internal set; }

	[JsonProperty("focused", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Focused { get; internal set; }
}
