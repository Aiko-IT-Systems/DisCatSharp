using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessageActivity
{
	[JsonProperty("type")]
	public int Type { get; internal set; }

	[JsonProperty("party_id", NullValueHandling = NullValueHandling.Ignore)]
	public string? PartyId { get; internal set; }
}
