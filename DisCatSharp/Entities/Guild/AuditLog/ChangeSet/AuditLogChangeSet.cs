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
	public string? ChangeDescription { get; internal set; } = null;

	public AuditLogActionType ValidFor { get; internal set; } = AuditLogActionType.Invalid;

	public bool IsValid
		=> this.ActionType == this.ValidFor;

	/// <inheritdoc />
	public override string ToString()
		=> this.ChangeDescription ?? $"{this.UserId} executed {this.GetType().Name}";
}
