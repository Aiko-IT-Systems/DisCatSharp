namespace DisCatSharp.Enums;

/// <summary>
/// Represents the version check mode.
/// </summary>
public enum VersionCheckMode
{
	/// <summary>
	/// Checks for updates on GitHub.
	/// </summary>
	GitHub = 0,

	/// <summary>
	/// Checks for updates on NuGet.
	/// </summary>
	NuGet = 1
}
