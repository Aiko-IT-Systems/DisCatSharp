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

	/// <summary>
	/// Gets a value indicating whether the allowed permissions have changed.
	/// </summary>
	public bool AllowChanged => this.AllowBefore is not null || this.AllowAfter is not null;

	/// <summary>
	/// Gets the old allowed permissions before the change.
	/// </summary>
	public Permissions? AllowBefore => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.OldValue;

	/// <summary>
	/// Gets the new allowed permissions after the change.
	/// </summary>
	public Permissions? AllowAfter => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.NewValue;

	/// <summary>
	/// Gets a value indicating whether the denied permissions have changed.
	/// </summary>
	public bool DenyChanged => this.DenyBefore is not null || this.DenyAfter is not null;

	/// <summary>
	/// Gets the old denied permissions before the change.
	/// </summary>
	public Permissions? DenyBefore => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "deny")?.OldValue;

	/// <summary>
	/// Gets the new denied permissions after the change.
	/// </summary>
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
