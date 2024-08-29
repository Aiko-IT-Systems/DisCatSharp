using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordIntegrationAccount
{
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }
}
