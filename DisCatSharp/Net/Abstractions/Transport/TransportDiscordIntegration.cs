using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordIntegration : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; internal set; }

	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool Enabled { get; internal set; }

	[JsonProperty("syncing", NullValueHandling = NullValueHandling.Ignore)]
	public bool Syncing { get; internal set; }

	[JsonProperty("role_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? RoleId { get; internal set; }

	[JsonProperty("enable_emoticons", NullValueHandling = NullValueHandling.Ignore)]
	public bool? EnableEmoticons { get; internal set; }

	[JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
	public int ExpireBehavior { get; internal set; }

	[JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
	public int ExpireGracePeriod { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? User { get; internal set; }

	[JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordIntegrationAccount? Account { get; internal set; }

	[JsonProperty("synced_at", NullValueHandling = NullValueHandling.Ignore)]
	public string SyncedAt { get; internal set; }

	[JsonProperty("subscriber_count", NullValueHandling = NullValueHandling.Ignore)]
	public int SubscriberCount { get; internal set; }

	[JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
	public bool Revoked { get; internal set; }

	[JsonProperty("application", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordIntegrationApplication? Application { get; internal set; }
}
