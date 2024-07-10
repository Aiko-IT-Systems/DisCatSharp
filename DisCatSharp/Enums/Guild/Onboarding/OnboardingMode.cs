namespace DisCatSharp.Enums;

/// <summary>
/// Represents the onboarding mode.
/// </summary>
public enum OnboardingMode
{
	/// <summary>
	/// Counts only Default Channels towards constraints.
	/// </summary>
	OnboardingDefault = 0,

	/// <summary>
	/// Counts Default Channels and Questions towards constraints.
	/// </summary>
	OnboardingAdvanced = 1
}
