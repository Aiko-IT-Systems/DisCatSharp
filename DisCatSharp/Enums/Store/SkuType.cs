namespace DisCatSharp.Enums;

/// <summary>
///     Represents the sku type.
/// </summary>
public enum SkuType
{
	/// <summary>
	///     An application sku.
	/// </summary>
	Application = 1,

	/// <summary>
	///     Durable one-time purchase.
	/// </summary>
	Durable = 2,

	/// <summary>
	///     Consumable one-time purchase.
	/// </summary>
	Consumable = 3,

	/// <summary>
	///     A bundle sku.
	/// </summary>
	Bundle = 4,

	/// <summary>
	///     Represents a recurring subscription.
	/// </summary>
	Subscription = 5,

	/// <summary>
	///     System-generated group for each SUBSCRIPTION SKU created .
	/// </summary>
	SubscriptionGroup = 6
}
