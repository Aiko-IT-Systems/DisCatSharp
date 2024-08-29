using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordDiscordSelectMenu : TransportDiscordMessageComponent
{
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordSelectOption> Options { get; internal set; }

	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string? Placeholder { get; internal set; }

	[JsonProperty("min_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinValues { get; internal set; }

	[JsonProperty("max_values", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxValues { get; internal set; }

	[JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Disabled { get; internal set; }
}
