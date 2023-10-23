using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating a permission overwrite for a channel.
/// </summary>
public sealed class ChannelOverwriteUpdateChangeSet : DiscordAuditLogEntry
{
	public ChannelOverwriteUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelOverwriteUpdate;
	}

	public bool AllowChanged => this.AllowBefore is not null || this.AllowAfter is not null;
	public Permissions? AllowBefore => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.OldValue;
	public Permissions? AllowAfter => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.NewValue;

	public bool DenyChanged => this.DenyBefore is not null || this.DenyAfter is not null;
	public Permissions? DenyBefore => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "deny")?.OldValue;
	public Permissions? DenyAfter => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "deny")?.NewValue;

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.User} executed {this.GetType().Name.Replace("ChangeSet", string.Empty)} for {this.Options!.OverwrittenEntityType} with id{this.Options.OverwrittenEntityId} {(this.Options.OverwrittenEntityType == OverwriteType.Role ? $" and name {this.Options.RoleName}" : string.Empty)} with reason {this.Reason ?? $"No reason given".Italic()}\n";

			if (this.AllowChanged)
				description += this.AllowAfter is not null ? $"- Set Allowed Permissions to {this.AllowAfter}\n" : "- Unset Allowed Permissions\n";

			if (this.DenyChanged)
				description += this.DenyAfter is not null ? $"- Set Denied Permissions to {this.DenyAfter}\n" : "- Unset Denied Permissions\n";

			return description;
		}
	}
}
