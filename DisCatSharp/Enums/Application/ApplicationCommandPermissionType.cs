using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of the application command permission.
/// </summary>
[Flags]
public enum ApplicationCommandPermissionType
{
	/// <summary>
	/// The permission is bound to a role.
	/// </summary>
	Role = 1,

	/// <summary>
	/// The permission is bound to a user.
	/// </summary>
	User = 2,

	/// <summary>
	/// The permission is bound to a channel.
	/// </summary>
	Channel = 3
}
