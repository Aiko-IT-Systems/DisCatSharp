using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating clyde in the server.
/// </summary>
public sealed class ClydeUpdateChangeSet : DiscordAuditLogEntry
{
	public ClydeUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ClydeAIProfileUpdate;
	}

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool AvatarHashChanged => this.AvatarHashBefore is not null || this.AvatarHashAfter is not null;
	public string? AvatarHashBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.OldValue;
	public string? AvatarHashAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "avatar_hash")?.NewValue;

	public bool BannerHashChanged => this.BannerHashBefore is not null || this.BannerHashAfter is not null;
	public string? BannerHashBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "banner_hash")?.OldValue;
	public string? BannerHashAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "banner_hash")?.NewValue;

	public bool BioChanged => this.BioBefore is not null || this.BioAfter is not null;
	public string? BioBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "bio")?.OldValue;
	public string? BioAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "bio")?.NewValue;

	public bool PersonalityChanged => this.PersonalityBefore is not null || this.PersonalityAfter is not null;
	public string? PersonalityBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "personality")?.OldValue;
	public string? PersonalityAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "personality")?.NewValue;

	public bool ClydeProfileIdChanged => this.ClydeProfileIdBefore is not null || this.ClydeProfileIdAfter is not null;
	public ulong? ClydeProfileIdBefore => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "clyde_profile_id")?.OldValue;
	public ulong? ClydeProfileIdAfter => (ulong?)this.Changes.FirstOrDefault(x => x.Key == "clyde_profile_id")?.NewValue;

	public bool ThemeColorsChanged => this.ThemeColorIntsBefore is not null || this.ThemeColorIntsAfter is not null;
	public List<int>? ThemeColorIntsBefore => (List<int>?)this.Changes.FirstOrDefault(x => x.Key == "theme_colors")?.OldValue;
	public IReadOnlyList<DiscordColor>? ThemeColoreBefore => !(this.ThemeColorIntsBefore is not null && this.ThemeColorIntsBefore.Count != 0) ? null : this.ThemeColorIntsBefore.Select(x => new DiscordColor(x)).ToList();
	public List<int>? ThemeColorIntsAfter => (List<int>?)this.Changes.FirstOrDefault(x => x.Key == "theme_colors")?.NewValue;
	public IReadOnlyList<DiscordColor>? ThemeColorsAfter => !(this.ThemeColorIntsAfter is not null && this.ThemeColorIntsAfter.Count != 0) ? null : this.ThemeColorIntsAfter.Select(x => new DiscordColor(x)).ToList();
}
