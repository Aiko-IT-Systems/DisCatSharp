using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbedProvider
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? Url { get; internal set; }
}
