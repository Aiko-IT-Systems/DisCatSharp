using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordDefaultReactionEmoji
{
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmojiId { get; internal set; }

	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? EmojiName { get; internal set; }
}
