namespace DisCatSharp.Enums;

/// <summary>
/// Represents multi-factor authentication level required by a guild to use administrator functionality.
/// </summary>
public enum MfaLevel
{
	/// <summary>
	/// Multi-factor authentication is not required to use administrator functionality.
	/// </summary>
	Disabled = 0,

	/// <summary>
	/// Multi-factor authentication is required to use administrator functionality.
	/// </summary>
	Enabled = 1
}
