namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application discoverability state.
/// </summary>
public enum ApplicationMonetizationState
{
	/// <summary>
	/// This application does not have monetization set up
	/// </summary>
	None = 1,

	/// <summary>
	/// This application has monetization set up
	/// </summary>
	Enabled = 2,

	/// <summary>
	/// This application has been blocked from monetizing
	/// </summary>
	Blocked = 3
}
