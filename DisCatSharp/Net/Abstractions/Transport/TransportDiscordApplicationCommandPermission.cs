using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommandPermission : SnowflakeObject
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandPermissionType Type { get; internal set; }

	[JsonProperty("permission", NullValueHandling = NullValueHandling.Ignore)]
	public bool Permission { get; internal set; }
}
