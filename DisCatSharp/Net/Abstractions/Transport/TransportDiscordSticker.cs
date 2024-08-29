using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordSticker : NullableSnowflakeObject
{
	[JsonProperty("pack_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? PackId { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
	public string? Tags { get; internal set; }

	[JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
	public string? Asset { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public StickerType Type { get; internal set; }

	[JsonProperty("format_type", NullValueHandling = NullValueHandling.Ignore)]
	public StickerFormatType FormatType { get; internal set; }

	[JsonProperty("available", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Available { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? User { get; internal set; }

	[JsonProperty("sort_value", NullValueHandling = NullValueHandling.Ignore)]
	public int? SortValue { get; internal set; }
}
