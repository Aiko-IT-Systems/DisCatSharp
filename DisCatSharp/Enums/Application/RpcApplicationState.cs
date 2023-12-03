namespace DisCatSharp.Enums;

/// <summary>
/// Represents the rpc application state.
/// </summary>
public enum RpcApplicationState
{
	DisabledOrNone = 0,
	Unsubmitted = 1,
	Submitted = 2,
	Approved = 3,
	Rejected = 4
}
