using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPresenceUpdate
{
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser User { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public string Status { get; internal set; }

	[JsonProperty("activities", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordActivity>? Activities { get; internal set; }

	[JsonProperty("client_status", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordClientStatus? ClientStatus { get; internal set; }
}
