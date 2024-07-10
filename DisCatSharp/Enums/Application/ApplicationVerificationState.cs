namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application verification state (<see cref="UserFlags.VerifiedBot"/>).
/// </summary>
public enum ApplicationVerificationState
{
	/// <summary>
	/// This application is ineligible for verification
	/// </summary>
	Ineligible = 1,

	/// <summary>
	/// This application has not yet been applied for verification
	/// </summary>
	Unsubmitted = 2,

	/// <summary>
	/// This application has submitted a verification request
	/// </summary>
	Submitted = 3,

	/// <summary>
	/// This application has been verified
	/// </summary>
	Succeeded = 4
}
