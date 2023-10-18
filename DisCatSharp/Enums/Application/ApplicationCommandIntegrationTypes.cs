
namespace DisCatSharp.Enums;

/// <summary>
/// Represents the application command integration types.
/// </summary>
public enum ApplicationCommandIntegrationTypes : int
{
	/// <summary>
	/// Application command is installed for guild (default).
	/// </summary>
	InstalledToGuild = 0,

	/// <summary>
	/// Application command is installed as user app.
	/// </summary>
	InstalledToUser = 1
}
