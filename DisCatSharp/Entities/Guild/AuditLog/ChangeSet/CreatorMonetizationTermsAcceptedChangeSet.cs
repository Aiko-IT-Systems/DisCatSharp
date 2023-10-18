using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for the acceptance of creator monetization terms.
/// </summary>
public sealed class CreatorMonetizationTermsAcceptedChangeSet : DiscordAuditLogEntry
{
	public CreatorMonetizationTermsAcceptedChangeSet()
	{
		this.ValidFor = AuditLogActionType.CreatorMonetizationTermsAccepted;
	}

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"{this.UserId} accepted Creator Monetization Terms";
}
