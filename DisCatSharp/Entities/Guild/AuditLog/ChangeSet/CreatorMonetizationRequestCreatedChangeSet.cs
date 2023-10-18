using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for the creation of a creator monetization request.
/// </summary>
public sealed class CreatorMonetizationRequestCreatedChangeSet : DiscordAuditLogEntry
{
	public CreatorMonetizationRequestCreatedChangeSet()
	{
		this.ValidFor = AuditLogActionType.CreatorMonetizationRequestCreated;
	}

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.UserId} created Creator Monetization Request";
}
