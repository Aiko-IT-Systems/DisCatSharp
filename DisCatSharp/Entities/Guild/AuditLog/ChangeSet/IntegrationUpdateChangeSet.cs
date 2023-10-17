using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating an app, such as its scopes.
/// </summary>
public sealed class IntegrationUpdateChangeSet : DiscordAuditLogEntry
{
	internal IntegrationUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.IntegrationUpdate;
	}

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool EnabledChanged => this.EnabledBefore is not null || this.EnabledAfter is not null;
	public bool? EnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.OldValue;
	public bool? EnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.NewValue;

	public bool SyncingChanged => this.SyncingBefore is not null || this.SyncingAfter is not null;
	public bool? SyncingBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "syncing")?.OldValue;
	public bool? SyncingAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "syncing")?.NewValue;

	public bool RoleIdChanged => this.RoleIdBefore is not null || this.RoleIdAfter is not null;
	public ulong? RoleIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "role_id")?.OldValue;
	public ulong? RoleIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "role_id")?.NewValue;
	public DiscordRole? RoleBefore => this.Discord.Guilds[this.GuildId].Roles.TryGetValue(this.RoleIdBefore ?? 0ul, out var role) ? role : null;
	public DiscordRole? RoleAfter => this.Discord.Guilds[this.GuildId].Roles.TryGetValue(this.RoleIdAfter ?? 0ul, out var role) ? role : null;

	public bool EnableEmoticonsChanged => this.EnableEmoticonsBefore is not null || this.EnableEmoticonsAfter is not null;
	public bool? EnableEmoticonsBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enable_emoticons")?.OldValue;
	public bool? EnableEmoticonsAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enable_emoticons")?.NewValue;

	public bool ExpireBehaviorChanged => this.ExpireBehaviorBefore is not null || this.ExpireBehaviorAfter is not null;
	public IntegrationExpireBehavior? ExpireBehaviorBefore => (IntegrationExpireBehavior?)this.Changes.FirstOrDefault(x => x.Key == "expire_behavior")?.OldValue;
	public IntegrationExpireBehavior? ExpireBehaviorAfter => (IntegrationExpireBehavior?)this.Changes.FirstOrDefault(x => x.Key == "expire_behavior")?.NewValue;

	public bool ExpireGracePeriodChanged => this.ExpireGracePeriodBefore is not null || this.ExpireGracePeriodAfter is not null;
	public int? ExpireGracePeriodBefore => (int?)this.Changes.FirstOrDefault(x => x.Key == "expire_grace_period")?.OldValue;
	public int? ExpireGracePeriodAfter => (int?)this.Changes.FirstOrDefault(x => x.Key == "expire_grace_period")?.NewValue;

	public bool AccountChanged => this.AccountBefore is not null || this.AccountAfter is not null;
	public DiscordIntegrationAccount? AccountBefore => (DiscordIntegrationAccount?)this.Changes.FirstOrDefault(x => x.Key == "account")?.OldValue;
	public DiscordIntegrationAccount? AccountAfter => (DiscordIntegrationAccount?)this.Changes.FirstOrDefault(x => x.Key == "account")?.NewValue;

	public bool RevokedChanged => this.RevokedBefore is not null || this.RevokedAfter is not null;
	public bool? RevokedBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "revoked")?.OldValue;
	public bool? RevokedAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "revoked")?.NewValue;

	public bool ScopesChanged => this.ScopesBefore is not null || this.ScopesAfter is not null;
	public IReadOnlyList<string>? ScopesBefore => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "scopes")?.OldValue;
	public IReadOnlyList<string>? ScopesAfter => (IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "scopes")?.NewValue;
}
