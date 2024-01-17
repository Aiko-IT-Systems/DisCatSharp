using System;

namespace DisCatSharp.Enums.Core;

/// <summary>
/// Defines how are command cooldowns applied.
/// </summary>
[Flags]
public enum CooldownBucketType
{
	/// <summary>
	/// Denotes that the command will have its cooldown applied globally.
	/// </summary>
	Global = 0,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-user globally.
	/// </summary>
	User = 1,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-channel.
	/// </summary>
	Channel = 2,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-guild. Skipped for DMs.
	/// </summary>
	Guild = 3,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-member per-guild. Skipped for DMs.
	/// </summary>
	Member = 4
}
