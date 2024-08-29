using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbedImage
{
	[JsonProperty("url")]
	public string Url { get; internal set; }

	[JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? ProxyUrl { get; internal set; }

	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int? Height { get; internal set; }

	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int? Width { get; internal set; }
}
