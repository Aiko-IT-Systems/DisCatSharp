using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordInteractionData : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandType Type { get; internal set; }

	[JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordResolvedData DiscordResolved { get; internal set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordApplicationCommandInteractionDataOption> Options { get; internal set; }

	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	[JsonProperty("component_type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType ComponentType { get; internal set; }

	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Values { get; internal set; }
}
