using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordGuildScheduledEventEntityMetadata
{
	[JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
	public string? Location { get; internal set; }
}
