namespace DisCatSharp.ApplicationCommands.Enums;

/// <summary>
/// Defines how are command cooldowns applied.
/// </summary>
public enum CooldownBucketType
{
	/// <summary>
	/// Denotes that the command will have its cooldown applied globally.
	/// </summary>
	Global = 0,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-user.
	/// </summary>
	User = 1,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-channel.
	/// </summary>
	Channel = 2,

	/// <summary>
	/// Denotes that the command will have its cooldown applied per-guild. In DMs, this applies the cooldown per-channel.
	/// </summary>
	Guild = 4
}
