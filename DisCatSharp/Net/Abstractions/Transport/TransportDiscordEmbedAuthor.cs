using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbedAuthor
{
	[JsonProperty("name")]
	public string Name { get; internal set; }

	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? Url { get; internal set; }

	[JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? IconUrl { get; internal set; }

	[JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? ProxyIconUrl { get; internal set; }
}
