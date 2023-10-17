using DisCatSharp.Enums;

namespace DisCatSharp.Entities.Guild.AuditLog;

/// <summary>
/// Represents a dynamic change set.
/// </summary>
public class AuditLogChangeSet : DiscordAuditLogEntry
{
	/// <summary>
	/// Gets the change description.
	/// </summary>
	public virtual string? ChangeDescription { get; internal set; } = null;

	/// <summary>
	/// Gets the valid action type for this change set.
	/// </summary>
	public AuditLogActionType ValidFor { get; internal set; }

	/// <summary>
	/// Gets whether this change set is valid.
	/// </summary>
	public bool IsValid
		=> this.ActionType == this.ValidFor;

	/// <inheritdoc />
	public override string ToString()
		=> this.ChangeDescription ?? $"{this.UserId} executed {this.GetType().Name}";
}
