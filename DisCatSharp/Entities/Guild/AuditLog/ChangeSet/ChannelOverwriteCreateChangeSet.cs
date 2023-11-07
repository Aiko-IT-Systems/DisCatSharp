using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for adding a permission overwrite to a channel.
/// </summary>
public sealed class ChannelOverwriteCreateChangeSet : DiscordAuditLogEntry
{
	public ChannelOverwriteCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.ChannelOverwriteCreate;
	}

	/// <summary>
	/// Gets the allowed permissions after the change.
	/// </summary>
	public Permissions? Allowed => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "allow")?.NewValue;

	/// <summary>
	/// Gets the denied permissions after the change.
	/// </summary>
	public Permissions? Denied => (Permissions?)this.Changes.FirstOrDefault(x => x.Key == "deny")?.NewValue;

	/// <inheritdoc />
	internal override string ChangeDescription
	{
		get
		{
			var description = $"{this.User} executed {this.GetType().Name.Replace("ChangeSet", string.Empty)} for {this.Options!.OverwrittenEntityType} with id{this.Options.OverwrittenEntityId} {(this.Options.OverwrittenEntityType == OverwriteType.Role ? $" and name {this.Options.RoleName}" : string.Empty)} with reason {this.Reason ?? $"No reason given".Italic()}\n";

			description += this.Allowed is not null ? $"- Set Allowed Permissions to {this.Allowed}\n" : "- No Allowed Permissions\n";

			description += this.Denied is not null ? $"- Set Denied Permissions to {this.Denied}\n" : "- No Denied Permissions\n";

			return description;
		}
	}
}
