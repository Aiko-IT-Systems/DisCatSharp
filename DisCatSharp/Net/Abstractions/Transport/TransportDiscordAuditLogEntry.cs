using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAuditLogEntry : SnowflakeObject
{
	[JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
	public string TargetId { get; internal set; }

	[JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordAuditLogChange> Changes { get; internal set; }

	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; internal set; }

	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public AuditLogActionType ActionType { get; internal set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordAuditLogEntryOptions Options { get; internal set; }

	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? Reason { get; internal set; }
}
