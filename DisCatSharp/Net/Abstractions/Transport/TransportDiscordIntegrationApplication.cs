using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordIntegrationApplication : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string Icon { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	[JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
	public string? Summary { get; internal set; }

	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser Bot { get; internal set; }
}
