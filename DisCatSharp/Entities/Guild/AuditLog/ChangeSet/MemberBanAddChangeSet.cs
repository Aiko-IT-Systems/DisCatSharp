using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for banning a member from the server.
/// </summary>
public sealed class MemberBanAddChangeSet : DiscordAuditLogEntry
{
	public MemberBanAddChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberBanAdd;
	}

	/// <summary>
	/// Gets the user who was banned.
	/// </summary>
	public DiscordUser Target => this.Discord.GetCachedOrEmptyUserInternal(this.TargetId!.Value);

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.User} banned {this.Target} with reason {this.Reason ?? "none"}";
}
