using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for lifting a server ban for a member.
/// </summary>
public sealed class MemberBanRemoveChangeSet : DiscordAuditLogEntry
{
	public MemberBanRemoveChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberBanRemove;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);
}
