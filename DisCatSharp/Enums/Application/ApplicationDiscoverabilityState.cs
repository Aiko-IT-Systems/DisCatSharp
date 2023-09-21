namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application discoverability state.
/// </summary>
public enum ApplicationDiscoverabilityState : int
{
	Ineligible = 1,
	NotDiscoverable = 2,
	Discoverable = 3,
	Featureable = 4,
	Blocked = 5
}
