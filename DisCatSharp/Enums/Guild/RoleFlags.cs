using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents additional details of a role.
/// </summary>
[Flags]
public enum RoleFlags : long
{
	/// <summary>
	///     This role has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     This role is in a prompt.
	/// </summary>
	InPrompt = 1L << 0,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
