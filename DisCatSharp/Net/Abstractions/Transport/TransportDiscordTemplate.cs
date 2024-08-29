using System;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordTemplate : SnowflakeObject
{
	[JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
	public string Code { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("usage_count", NullValueHandling = NullValueHandling.Ignore)]
	public int UsageCount { get; internal set; }

	[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong CreatorId { get; internal set; }

	[JsonProperty("creator", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? Creator { get; internal set; }

	[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset CreatedAt { get; internal set; }

	[JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset UpdatedAt { get; internal set; }

	[JsonProperty("source_guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SourceGuildId { get; internal set; }

	[JsonProperty("serialized_source_guild", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuild? SerializedSourceGuild { get; internal set; }

	[JsonProperty("is_dirty", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsDirty { get; internal set; }
}
