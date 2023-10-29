using System;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for editing a role.
/// </summary>
public sealed class RoleUpdateChangeSet : DiscordAuditLogEntry
{
	public RoleUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.RoleUpdate;
	}

	public DiscordRole Role => this.Discord.Guilds[this.GuildId].GetRole(this.TargetId.Value!);

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool ColorChanged => this.ColorBefore is not null || this.ColorAfter is not null;
	public DiscordColor? ColorBefore => new((int)this.Changes.FirstOrDefault(x => x.Key == "color")?.OldValue);
	public DiscordColor? ColorAfter => new((int)this.Changes.FirstOrDefault(x => x.Key == "color")?.NewValue);

	public bool IsHoistedChanged => this.IsHoistedBefore is not null || this.IsHoistedAfter is not null;
	public bool? IsHoistedBefore => (bool)this.Changes.FirstOrDefault(x => x.Key == "hoist")?.OldValue;
	public bool? IsHoistedAfter => (bool)this.Changes.FirstOrDefault(x => x.Key == "hoist")?.NewValue;

	public bool IsManagedChanged => this.IsManagedBefore is not null || this.IsManagedAfter is not null;
	public bool? IsManagedBefore => (bool)this.Changes.FirstOrDefault(x => x.Key == "managed")?.OldValue;
	public bool? IsManagedAfter => (bool)this.Changes.FirstOrDefault(x => x.Key == "managed")?.NewValue;

	public bool IsMentionableChanged => this.IsMentionableBefore is not null || this.IsMentionableAfter is not null;
	public bool? IsMentionableBefore => (bool)this.Changes.FirstOrDefault(x => x.Key == "mentionable")?.OldValue;
	public bool? IsMentionableAfter => (bool)this.Changes.FirstOrDefault(x => x.Key == "mentionable")?.NewValue;

	public bool IconHashChanged => this.IconHashBefore is not null || this.IconHashAfter is not null;
	public string? IconHashBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.OldValue;
	public string? IconHashAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "icon")?.NewValue;

	public bool UnicodeEmojiChanged => this.UnicodeEmojiBefore is not null || this.UnicodeEmojiAfter is not null;
	public string? UnicodeEmojiBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "unicode_emoji")?.OldValue;
	public string? UnicodeEmojiAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "unicode_emoji")?.NewValue;

	public bool PositionChanged => this.PositionBefore is not null || this.PositionAfter is not null;
	public int? PositionBefore => (int)this.Changes.FirstOrDefault(x => x.Key == "position")?.OldValue;
	public int? PositionAfter => (int)this.Changes.FirstOrDefault(x => x.Key == "position")?.NewValue;

	public bool PermissionsChanged => this.PermissionsBefore is not null || this.PermissionsAfter is not null;
	public Permissions? PermissionsBefore => (Permissions)Convert.ToInt64(this.Changes.FirstOrDefault(x => x.Key == "permissions")?.OldValue);
	public Permissions? PermissionsAfter => (Permissions)Convert.ToInt64(this.Changes.FirstOrDefault(x => x.Key == "permissions")?.NewValue);

	public bool TagsChanged => this.TagsBefore is not null || this.TagsAfter is not null;
	public DiscordRoleTags? TagsBefore => (DiscordRoleTags)this.Changes.FirstOrDefault(x => x.Key == "tags")?.OldValue;
	public DiscordRoleTags? TagsAfter => (DiscordRoleTags)this.Changes.FirstOrDefault(x => x.Key == "tags")?.NewValue;

	public bool FlagsChanged => this.FlagsBefore is not null || this.FlagsAfter is not null;
	public RoleFlags? FlagsBefore => (RoleFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.OldValue;
	public RoleFlags? FlagsAfter => (RoleFlags)this.Changes.FirstOrDefault(x => x.Key == "flags")?.NewValue;
}
