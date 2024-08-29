using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordForumTag : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmojiId { get; internal set; }

	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? EmojiName { get; internal set; }

	[JsonProperty("moderated", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Moderated { get; internal set; }
}
