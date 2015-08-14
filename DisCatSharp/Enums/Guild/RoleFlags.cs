using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents additional details of a role.
/// </summary>
[Flags]
public enum RoleFlags
{
	/// <summary>
	/// This role has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// This role is in a prompt.
	/// </summary>
	InPrompt = 1 << 0
}
