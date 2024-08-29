using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAuditLog
{
	[JsonProperty("webhooks", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordWebhook> Webhooks { get; internal set; }

	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordUser> Users { get; internal set; }

	[JsonProperty("audit_log_entries", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordAuditLogEntry> AuditLogEntries { get; internal set; }

	[JsonProperty("integrations", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordIntegration> Integrations { get; internal set; }

	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordThreadChannel> Threads { get; internal set; }

	[JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordGuildScheduledEvent> GuildScheduledEvents { get; internal set; }
}
