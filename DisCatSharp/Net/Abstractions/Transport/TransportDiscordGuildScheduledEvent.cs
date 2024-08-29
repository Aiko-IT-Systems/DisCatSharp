using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordGuildScheduledEvent : SnowflakeObject
{
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? CreatorId { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	[JsonProperty("scheduled_start_time", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ScheduledStartTime { get; internal set; }

	[JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ScheduledEndTime { get; internal set; }

	[JsonProperty("privacy_level", NullValueHandling = NullValueHandling.Ignore)]
	public int PrivacyLevel { get; internal set; }

	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public ScheduledEventStatus Status { get; internal set; }

	[JsonProperty("entity_type", NullValueHandling = NullValueHandling.Ignore)]
	public ScheduledEventEntityType EntityType { get; internal set; }

	[JsonProperty("entity_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EntityId { get; internal set; }

	[JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuildScheduledEventEntityMetadata? EntityMetadata { get; internal set; }

	[JsonProperty("user_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? UserCount { get; internal set; }

	[JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
	public string Image { get; internal set; }
}
