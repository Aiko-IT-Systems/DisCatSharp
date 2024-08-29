using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordTextInput : TransportDiscordMessageComponent
{
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	[JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
	public TextComponentStyle Style { get; internal set; }

	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	[JsonProperty("min_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MinLength { get; internal set; }

	[JsonProperty("max_length", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxLength { get; internal set; }

	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string Placeholder { get; internal set; }
}
