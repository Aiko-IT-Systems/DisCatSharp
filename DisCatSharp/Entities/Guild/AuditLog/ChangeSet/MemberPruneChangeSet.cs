using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for pruning members from the server.
/// </summary>
public sealed class MemberPruneChangeSet : DiscordAuditLogEntry
{
	public MemberPruneChangeSet()
	{
		this.ValidFor = AuditLogActionType.MemberPrune;
	}

	public int DeleteMemberDays => (int)this.Changes.FirstOrDefault(x => x.Key == "delete_member_days")?.OldValue;
	public int Count => (int)this.Changes.FirstOrDefault(x => x.Key == "members_removed")?.OldValue;

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.User} pruned {this.Count} members with reason {this.Reason ?? "none"} and deleted messages from the last {this.DeleteMemberDays} days";
}
