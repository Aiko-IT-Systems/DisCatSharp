using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordReaction
{
	[JsonProperty("count")]
	public int Count { get; internal set; }

	[JsonProperty("me")]
	public bool Me { get; internal set; }

	[JsonProperty("emoji")]
	public TransportDiscordEmoji Emoji { get; internal set; }
}
