using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordClientStatus
{
	[JsonProperty("desktop", NullValueHandling = NullValueHandling.Ignore)]
	public string? Desktop { get; internal set; }

	[JsonProperty("mobile", NullValueHandling = NullValueHandling.Ignore)]
	public string? Mobile { get; internal set; }

	[JsonProperty("web", NullValueHandling = NullValueHandling.Ignore)]
	public string? Web { get; internal set; }
}
