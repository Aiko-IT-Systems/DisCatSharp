using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for removing a member from the server.
/// </summary>
public sealed class MemberKickChangeSet : DiscordAuditLogEntry
{
	public MemberKickChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberKick;
	}

	/// <summary>
	/// Gets the user who was kicked.
	/// </summary>
	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.User} kicked {this.Target} with reason {this.Reason ?? "none"}";
}
