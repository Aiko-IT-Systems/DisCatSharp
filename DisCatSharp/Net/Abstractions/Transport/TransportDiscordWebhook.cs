using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordWebhook : SnowflakeObject
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public int Type { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? User { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
	public string? Avatar { get; internal set; }

	[JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
	public string? Token { get; internal set; }

	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }
}
