using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAutoModerationActionMetadata
{
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("duration_seconds", NullValueHandling = NullValueHandling.Ignore)]
	public int? DurationSeconds { get; internal set; }
}
