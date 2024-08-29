using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessageReference
{
	[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? MessageId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
	public bool? FailIfNotExists { get; internal set; }
}
