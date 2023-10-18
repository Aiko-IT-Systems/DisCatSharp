using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for adding or removing a role from a member.
/// </summary>
public sealed class MemberRoleUpdateChangeSet : DiscordAuditLogEntry
{
	internal MemberRoleUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberRoleUpdate;
	}

	public IReadOnlyList<DiscordRole> AddedRoles
		=> ((IReadOnlyList<DiscordRole>)this.Changes.FirstOrDefault(x => x.Key == "$add")?.OldValue)
				.Select(x => this.Discord.Guilds[this.GuildId].GetRole(x.Id)).ToList();

	public IReadOnlyList<DiscordRole> RemovedRoles
		=> ((IReadOnlyList<DiscordRole>)this.Changes.FirstOrDefault(x => x.Key == "$remove")?.OldValue)
				.Select(x => this.Discord.Guilds[this.GuildId].GetRole(x.Id)).ToList();
}
