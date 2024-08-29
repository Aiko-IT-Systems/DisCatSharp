using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordChannel : NullableSnowflakeObject
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType Type { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int? Position { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string? Topic { get; internal set; }

	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Nsfw { get; internal set; }

	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public int? RateLimitPerUser { get; internal set; }

	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ParentId { get; internal set; }

	[JsonProperty("available_tags", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordForumTag> AvailableTags { get; internal set; }

	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> AppliedTags { get; internal set; }

	[JsonProperty("default_reaction_emoji", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordDefaultReactionEmoji? DefaultReactionEmoji { get; internal set; }

	[JsonProperty("default_thread_rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public int? DefaultThreadRateLimitPerUser { get; internal set; }

	[JsonProperty("default_sort_order", NullValueHandling = NullValueHandling.Ignore)]
	public ForumPostSortOrder? DefaultSortOrder { get; internal set; }

	[JsonProperty("default_forum_layout", NullValueHandling = NullValueHandling.Ignore)]
	public ForumLayout? DefaultForumLayout { get; internal set; }
}
