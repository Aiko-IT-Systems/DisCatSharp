using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for banning a member from the server.
/// </summary>
public sealed class MemberBanAddChangeSet : DiscordAuditLogEntry
{
	internal MemberBanAddChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberBanAdd;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);
}
