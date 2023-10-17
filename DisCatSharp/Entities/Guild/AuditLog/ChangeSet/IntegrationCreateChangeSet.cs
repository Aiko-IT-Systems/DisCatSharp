using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for adding an app to the server.
/// </summary>
public sealed class IntegrationCreateChangeSet : DiscordAuditLogEntry
{
	internal IntegrationCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.IntegrationCreate;
	}

	public string Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public string Type => (string?)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;
	public bool? Enabled => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.NewValue;
	public bool? Syncing => (bool?)this.Changes.FirstOrDefault(x => x.Key == "syncing")?.NewValue;

	public ulong? RoleId => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "role_id")?.NewValue;
	public DiscordRole? Role => this.Discord.Guilds[this.GuildId].Roles.TryGetValue(this.RoleId ?? 0ul, out var role) ? role : null;

	public bool? EnableEmoticons => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enable_emoticons")?.NewValue;
	public IntegrationExpireBehavior? ExpireBehavior => (IntegrationExpireBehavior?)this.Changes.FirstOrDefault(x => x.Key == "expire_behavior")?.NewValue;
	public int? ExpireGracePeriod => (int?)this.Changes.FirstOrDefault(x => x.Key == "expire_grace_period")?.NewValue;
	public DiscordUser? Bot => (DiscordUser?)this.Changes.FirstOrDefault(x => x.Key == "user")?.NewValue;
	public DiscordIntegrationAccount? Account => (DiscordIntegrationAccount?)this.Changes.FirstOrDefault(x => x.Key == "account")?.NewValue;
	public DiscordApplication? Application => (DiscordApplication?)this.Changes.FirstOrDefault(x => x.Key == "application")?.NewValue;
	public bool? Revoked => (bool?)this.Changes.FirstOrDefault(x => x.Key == "revoked")?.NewValue;
	public IReadOnlyList<string>? Scopes => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "scopes")?.NewValue;
}
