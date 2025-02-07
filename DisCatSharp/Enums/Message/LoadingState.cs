namespace DisCatSharp.Enums;

/// <summary>
/// Represents the loading state of a URL media resource.
/// </summary>
public enum LoadingState
{
	/// <summary>
	/// The loading state is unknown.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// The resource is currently loading.
	/// </summary>
	Loading = 1,

	/// <summary>
	/// The resource has successfully loaded.
	/// </summary>
	LoadedSuccess = 2,

	/// <summary>
	/// The resource was not found.
	/// </summary>
	LoadedNotFound = 3
}
