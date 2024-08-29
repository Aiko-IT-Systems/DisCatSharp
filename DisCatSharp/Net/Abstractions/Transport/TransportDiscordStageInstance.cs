using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordStageInstance : SnowflakeObject
{
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; internal set; }

	[JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
	public int PrivacyLevel { get; internal set; }

	[JsonProperty("discoverable_disabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool DiscoverableDisabled { get; internal set; }
}
