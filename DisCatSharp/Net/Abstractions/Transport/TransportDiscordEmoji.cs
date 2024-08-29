using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmoji : NullableSnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? Roles { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? User { get; internal set; }

	[JsonProperty("require_colons", NullValueHandling = NullValueHandling.Ignore)]
	public bool? RequireColons { get; internal set; }

	[JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Managed { get; internal set; }

	[JsonProperty("animated", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Animated { get; internal set; }

	[JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Available { get; internal set; }
}
