using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessageInteraction : SnowflakeObject
{
	[JsonProperty("type")]
	public InteractionType Type { get; internal set; }

	[JsonProperty("name")]
	public string Name { get; internal set; }

	[JsonProperty("user")]
	public TransportDiscordUser DiscordUser { get; internal set; }
}
