
namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application verification state (<see cref="UserFlags.VerifiedBot"/>).
/// </summary>
public enum ApplicationVerificationState : int
{
	Ineligible = 1,
	Unsubmitted = 2,
	Submitted = 3,
	Succeeded = 4
}
