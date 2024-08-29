using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAutoModerationRule : SnowflakeObject
{
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong CreatorId { get; internal set; }

	[JsonProperty("event_type", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodEventType EventType { get; internal set; }

	[JsonProperty("trigger_type", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodTriggerType TriggerType { get; internal set; }

	[JsonProperty("trigger_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordAutoModerationTriggerMetadata TriggerMetadata { get; internal set; }

	[JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordAutoModerationAction> Actions { get; internal set; }

	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	[JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> ExemptRoles { get; internal set; }

	[JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> ExemptChannels { get; internal set; }
}
