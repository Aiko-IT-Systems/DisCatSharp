using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAuditLogEntryOptions
{
	[JsonProperty("delete_member_days", NullValueHandling = NullValueHandling.Ignore)]
	public string? DeleteMemberDays { get; internal set; }

	[JsonProperty("members_removed", NullValueHandling = NullValueHandling.Ignore)]
	public string? MembersRemoved { get; internal set; }

	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	[JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? MessageId { get; internal set; }

	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public string? Count { get; internal set; }

	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Id { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string? Type { get; internal set; }

	[JsonProperty("role_name", NullValueHandling = NullValueHandling.Ignore)]
	public string? RoleName { get; internal set; }
}
