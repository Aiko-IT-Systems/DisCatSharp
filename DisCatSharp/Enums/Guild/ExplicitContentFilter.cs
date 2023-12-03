namespace DisCatSharp.Enums;

/// <summary>
/// Represents the value of explicit content filter in a guild.
/// </summary>
public enum ExplicitContentFilter
{
	/// <summary>
	/// Explicit content filter is disabled.
	/// </summary>
	Disabled = 0,

	/// <summary>
	/// Only messages from members without any roles are scanned.
	/// </summary>
	MembersWithoutRoles = 1,

	/// <summary>
	/// Messages from all members are scanned.
	/// </summary>
	AllMembers = 2
}
