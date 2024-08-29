using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordThreadChannel
{
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Id { get; internal set; }

	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ChannelType Type { get; internal set; }

	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OwnerId { get; internal set; }

	[JsonProperty("message_count", NullValueHandling = NullValueHandling.Ignore)]
	public int MessageCount { get; internal set; }

	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int MemberCount { get; internal set; }

	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public int? RateLimitPerUser { get; internal set; }

	[JsonProperty("thread_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordThreadMetadata? ThreadMetadata { get; internal set; }

	[JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordThreadMember? Member { get; internal set; }

	[JsonProperty("applied_tags", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong>? AppliedTags { get; internal set; }
}
