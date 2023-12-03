namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application discoverability state.
/// </summary>
public enum ApplicationDiscoverabilityState
{
	/// <summary>
	/// This application is ineligible for the application directory
	/// </summary>
	Ineligible = 1,

	/// <summary>
	/// This application is not listed in the application directory
	/// </summary>
	NotDiscoverable = 2,

	/// <summary>
	/// This application is listed in the application directory
	/// </summary>
	Discoverable = 3,

	/// <summary>
	/// This application is featurable in the application directory
	/// </summary>
	Featureable = 4,

	/// <summary>
	/// This application has been blocked from appearing in the application directory
	/// </summary>
	Blocked = 5
}
