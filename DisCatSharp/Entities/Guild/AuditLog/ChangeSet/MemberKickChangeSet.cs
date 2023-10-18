using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for removing a member from the server.
/// </summary>
public sealed class MemberKickChangeSet : DiscordAuditLogEntry
{
	internal MemberKickChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberKick;
	}

	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);
}
