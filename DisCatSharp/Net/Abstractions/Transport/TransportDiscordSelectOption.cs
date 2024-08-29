using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordSelectOption
{
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmoji Emoji { get; internal set; }

	[JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
	public bool Default { get; internal set; }
}
