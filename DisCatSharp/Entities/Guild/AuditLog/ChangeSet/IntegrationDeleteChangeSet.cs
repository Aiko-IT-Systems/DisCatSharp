using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for removing an app from the server.
/// </summary>
public sealed class IntegrationDeleteChangeSet : DiscordAuditLogEntry
{
	internal IntegrationDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.IntegrationDelete;
	}

	public string Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string Type => (string?)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public bool? Enabled => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.OldValue;
	public bool? Syncing => (bool?)this.Changes.FirstOrDefault(x => x.Key == "syncing")?.OldValue;

	public ulong? RoleId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "role_id")?.OldValue;
	public DiscordRole? Role => this.Discord.Guilds[this.GuildId].Roles[this.RoleId!.Value];

	public bool? EnableEmoticons => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enable_emoticons")?.OldValue;
	public IntegrationExpireBehavior? ExpireBehavior => (IntegrationExpireBehavior?)this.Changes.FirstOrDefault(x => x.Key == "expire_behavior")?.OldValue;
	public int? ExpireGracePeriod => (int?)this.Changes.FirstOrDefault(x => x.Key == "expire_grace_period")?.OldValue;
	public DiscordUser? Bot => (DiscordUser?)this.Changes.FirstOrDefault(x => x.Key == "user")?.OldValue;
	public DiscordIntegrationAccount? Account => (DiscordIntegrationAccount?)this.Changes.FirstOrDefault(x => x.Key == "account")?.OldValue;
	public DiscordApplication? Application => (DiscordApplication?)this.Changes.FirstOrDefault(x => x.Key == "application")?.OldValue;
	public bool? Revoked => (bool?)this.Changes.FirstOrDefault(x => x.Key == "revoked")?.OldValue;
	public IReadOnlyList<string>? Scopes => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "scopes")?.OldValue;
}
