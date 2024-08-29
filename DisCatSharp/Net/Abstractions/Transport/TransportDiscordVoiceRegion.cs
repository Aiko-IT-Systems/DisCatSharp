using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordVoiceRegion
{
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public string Id { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
	public bool Optimal { get; internal set; }

	[JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
	public bool Deprecated { get; internal set; }

	[JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
	public bool Custom { get; internal set; }
}
