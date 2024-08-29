using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordInteraction : SnowflakeObject
{
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionType Type { get; internal set; }

	[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordInteractionData Data { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuildMember Member { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser User { get; internal set; }

	[JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
	public string Token { get; internal set; }

	[JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
	public int Version { get; internal set; }

	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordMessage Message { get; internal set; }
}
