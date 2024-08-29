using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordGuildWidget
{
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }
}
