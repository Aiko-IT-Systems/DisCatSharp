using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a permission overwrite from a channel.
/// </summary>
public sealed class ChannelOverwriteDeleteChangeSet : DiscordAuditLogEntry
{
	public ChannelOverwriteDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelOverwriteDelete;
	}

	/// <summary>
	/// Gets the allowed permissions before the change.
	/// </summary>
	public Permissions? Allowed => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.OldValue;

	/// <summary>
	/// Gets the denied permissions before the change.
	/// </summary>
	public Permissions? Denied => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "deny")?.OldValue;
}
