namespace DisCatSharp.Enums;

/// <summary>
/// Represents the store application state.
/// </summary>
public enum StoreApplicationState
{
	/// <summary>
	/// This application does not have a commerce license
	/// </summary>
	None = 1,

	/// <summary>
	/// This application has a commerce license but has not yet submitted a store approval request
	/// </summary>
	Paid = 2,

	/// <summary>
	/// This application has submitted a store approval request
	/// </summary>
	Submitted = 3,

	/// <summary>
	/// This application has been approved for the store
	/// </summary>
	Approved = 4,

	/// <summary>
	/// This application has been rejected from the store
	/// </summary>
	Rejected = 5,

	/// <summary>
	/// This application is blocked from the store
	/// </summary>
	Blocked = 6
}
