using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbedField
{
	[JsonProperty("name")]
	public string Name { get; internal set; }

	[JsonProperty("value")]
	public string Value { get; internal set; }

	[JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Inline { get; internal set; }
}
