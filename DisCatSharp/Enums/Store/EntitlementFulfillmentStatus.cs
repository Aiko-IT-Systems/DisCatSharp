namespace DisCatSharp.Enums;

/// <summary>
///     Represents the tenant fulfillment status of an entitlement.
/// </summary>
public enum EntitlementFulfillmentStatus
{
	/// <summary>
	///     Unknown fulfillment status.
	/// </summary>
	Unknown = 0,

	/// <summary>
	///     Fulfillment is not needed for this entitlement.
	/// </summary>
	FulfillmentNotNeeded = 1,

	/// <summary>
	///     Fulfillment is needed for this entitlement.
	/// </summary>
	FulfillmentNeeded = 2,

	/// <summary>
	///     The entitlement has been fulfilled.
	/// </summary>
	Fulfilled = 3,

	/// <summary>
	///     Fulfillment of the entitlement has failed.
	/// </summary>
	FulfillmentFailed = 4,

	/// <summary>
	///     Unfulfillment is needed for this entitlement.
	/// </summary>
	UnfulfillmentNeeded = 5,

	/// <summary>
	///     The entitlement has been unfulfilled.
	/// </summary>
	Unfulfilled = 6,

	/// <summary>
	///     Unfulfillment of the entitlement has failed.
	/// </summary>
	UnfulfillmentFailed = 7,

	/// <summary>
	///     Manual unfulfillment is needed for this entitlement.
	/// </summary>
	UnfulfillmentNeededManual = 8
}
