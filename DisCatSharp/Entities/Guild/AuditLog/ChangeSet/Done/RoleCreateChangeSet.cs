using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for creating a role.
/// </summary>
public sealed class RoleCreateChangeSet : DiscordAuditLogEntry
{
	public RoleCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.RoleCreate;
	}

	public DiscordRole Role => this.Discord.Guilds[this.GuildId].GetRole(this.TargetId.Value!);

	public string Name => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public DiscordColor Color => new((int)this.Changes.FirstOrDefault(x => x.Key == "color")?.NewValue);
	public bool IsHoisted => (bool)this.Changes.FirstOrDefault(x => x.Key == "hoist")?.NewValue;
	public bool IsManaged => (bool)this.Changes.FirstOrDefault(x => x.Key == "managed")?.NewValue;
	public bool IsMentionable => (bool)this.Changes.FirstOrDefault(x => x.Key == "mentionable")?.NewValue;
	public string? IconHash => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.NewValue;
	public string? UnicodeEmoji => (string?)this.Changes.FirstOrDefault(x => x.Key == "unicode_emoji")?.NewValue;
	public int Position => (int)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;
	public Permissions Permissions => (Permissions)Convert.ToInt64(this.Changes.FirstOrDefault(x => x.Key == "permissions")?.NewValue);
	public DiscordRoleTags Tags => (DiscordRoleTags)this.Changes.FirstOrDefault(x => x.Key == "tags")?.NewValue;
	public RoleFlags Flags => (RoleFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;
}
