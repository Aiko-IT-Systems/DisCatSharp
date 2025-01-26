namespace DisCatSharp.Enums;

/// <summary>
///     Represents the RPC application state.
/// </summary>
public enum RpcApplicationState
{
	/// <summary>
	///     The application is disabled or none.
	/// </summary>
	DisabledOrNone = 0,

	/// <summary>
	///     The application is unsubmitted.
	/// </summary>
	Unsubmitted = 1,

	/// <summary>
	///     The application is submitted.
	/// </summary>
	Submitted = 2,

	/// <summary>
	///     The application is approved.
	/// </summary>
	Approved = 3,

	/// <summary>
	///     The application is rejected.
	/// </summary>
	Rejected = 4
}
