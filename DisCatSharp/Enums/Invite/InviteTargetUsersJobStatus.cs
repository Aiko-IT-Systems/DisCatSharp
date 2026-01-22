namespace DisCatSharp.Enums;

/// <summary>
///     Represents the processing status for invite target users jobs.
/// </summary>
public enum InviteTargetUsersJobStatus
{
	/// <summary>
	///     The status is unspecified.
	/// </summary>
	Unspecified = 0,

	/// <summary>
	///     The target users job is processing.
	/// </summary>
	Processing = 1,

	/// <summary>
	///     The target users job completed successfully.
	/// </summary>
	Completed = 2,

	/// <summary>
	///     The target users job failed.
	/// </summary>
	Failed = 3
}
