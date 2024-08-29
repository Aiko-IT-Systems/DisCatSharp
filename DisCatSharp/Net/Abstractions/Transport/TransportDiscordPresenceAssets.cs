using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPresenceAssets
{
	[JsonProperty("large_image")]
	public string LargeImage { get; set; }

	[JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
	public string? LargeImageText { get; internal set; }

	[JsonProperty("small_image")]
	internal string SmallImage { get; set; }

	[JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
	public string? SmallImageText { get; internal set; }
}
