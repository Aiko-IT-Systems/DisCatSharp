using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a role.
/// </summary>
public sealed class RoleDeleteChangeSet : DiscordAuditLogEntry
{
	public RoleDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.RoleDelete;
	}

	public DiscordRole Role => this.Discord.Guilds[this.GuildId].GetRole(this.TargetId.Value!);

	public string Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public DiscordColor Color => new((int)this.Changes.FirstOrDefault(x => x.Key == "color")?.OldValue);
	public bool IsHoisted => (bool)this.Changes.FirstOrDefault(x => x.Key == "hoist")?.OldValue;
	public bool IsManaged => (bool)this.Changes.FirstOrDefault(x => x.Key == "managed")?.OldValue;
	public bool IsMentionable => (bool)this.Changes.FirstOrDefault(x => x.Key == "mentionable")?.OldValue;
	public string? IconHash => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.OldValue;
	public string? UnicodeEmoji => (string?)this.Changes.FirstOrDefault(x => x.Key == "unicode_emoji")?.OldValue;
	public int Position => (int)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;
	public Permissions Permissions => (Permissions)Convert.ToInt64(this.Changes.FirstOrDefault(x => x.Key == "permissions")?.OldValue);
	public DiscordRoleTags Tags => (DiscordRoleTags)this.Changes.FirstOrDefault(x => x.Key == "tags")?.OldValue;
	public RoleFlags Flags => (RoleFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
}
