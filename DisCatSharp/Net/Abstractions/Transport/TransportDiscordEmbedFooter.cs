using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbedFooter
{
	[JsonProperty("text")]
	public string Text { get; internal set; }

	[JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? IconUrl { get; internal set; }

	[JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? ProxyIconUrl { get; internal set; }
}
