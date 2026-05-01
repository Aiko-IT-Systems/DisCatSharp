namespace DisCatSharp.Enums;

/// <summary>
///     Represents the type of interaction used.
/// </summary>
public enum InteractionType
{
	/// <summary>
	///     Sent when registering an HTTP interaction endpoint with Discord. Must be replied to with a Pong.
	/// </summary>
	Ping = 1,

	/// <summary>
	///     An application command.
	/// </summary>
	ApplicationCommand = 2,

	/// <summary>
	///     A component.
	/// </summary>
	Component = 3,

	/// <summary>
	///     An autocomplete field.
	/// </summary>
	AutoComplete = 4,

	/// <summary>
	///     A modal component.
	/// </summary>
	ModalSubmit = 5,

	/// <summary>
	///    A request to check if a user is eligible to purchase a SKU from the social layer store.
	/// </summary>
	SocialLayerSkuPurchaseEligibility = 6
}
