using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommandPermissions : SnowflakeObject
{
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandPermission> Permissions { get; internal set; }
}
