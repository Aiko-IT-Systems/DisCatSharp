using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordPollMediaObject
{
	[JsonProperty("text")]
	public string Text { get; internal set; }

	[JsonProperty("emoji")]
	public TransportDiscordEmoji? Emoji { get; internal set; }
}
