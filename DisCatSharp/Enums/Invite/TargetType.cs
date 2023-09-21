namespace DisCatSharp.Enums;

/// <summary>
/// Represents the invite type .
/// </summary>
public enum TargetType
{
	/// <summary>
	/// Represents a streaming invite.
	/// </summary>
	Streaming = 1,

	/// <summary>
	/// Represents a activity invite.
	/// </summary>
	EmbeddedApplication = 2,

	/// <summary>
	/// Represents a role subscription invite.
	/// Not creatable by bots.
	/// </summary>
	RoleSubscriptionsPurchase = 3,

	/// <summary>
	/// Represents a promo page generated invite.
	/// Not creatable, system generated.
	/// </summary>
	PromoPage = 4
}
