using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.Guild;

/// <summary>
/// Represents a Discord audit log.
/// </summary>
public sealed class DiscordAuditLog
{
	/// <summary>
	/// List of application commands referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("application_commands", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordApplicationCommand> ReferencedApplicationCommands { get; internal set; } = new();

	/// <summary>
	/// List of <see cref="DiscordAuditLogEntry"/>, sorted from most to least recent.
	/// </summary>
	[JsonProperty("audit_log_entries", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordAuditLogEntry> Entries { get; internal set; } = new();

	/// <summary>
	/// List of auto moderation rules referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("auto_moderation_rules", NullValueHandling = NullValueHandling.Ignore)]
	public List<AutomodRule> ReferencedAutomodRules { get; internal set; } = new();

	/// <summary>
	/// List of guild scheduled events referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordScheduledEvent> ReferencedScheduledEvents { get; internal set; } = new();

	/// <summary>
	/// List of partial integration objects referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordIntegration> ReferencedIntegrations { get; internal set; } = new();

	/// <summary>
	/// List of threads referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordThreadChannel> ReferencedThreads { get; internal set; } = new();

	/// <summary>
	/// List of users referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordUser> ReferencedUsers { get; internal set; } = new();

	/// <summary>
	/// List of webhooks referenced in the <see cref="DiscordAuditLog"/>.
	/// </summary>
	[JsonProperty("webhooks", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordWebhook> ReferencedWebhooks { get; internal set; } = new();
}
