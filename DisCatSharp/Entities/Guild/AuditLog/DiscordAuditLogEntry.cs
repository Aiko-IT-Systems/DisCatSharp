using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.Guild.AuditLog;

/// <summary>
/// Represents a Discord audit log entry.
/// </summary>
public class DiscordAuditLogEntry : SnowflakeObject
{
	[JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
	public SnowflakeObject? TargetId { get; set; }

	[JsonProperty("changes", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAuditLogChangeObject> Changes { get; set; } = new();

	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserId { get; set; }

	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public AuditLogActionType ActionType { get; set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordAuditEntryInfo Options { get; set; }

	[JsonProperty("reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? Reason { get; set; }
}
