namespace DisCatSharp.Enums;

/// <summary>
///     Represents the api channel.
/// </summary>
public enum ApiChannel
{
	/// <summary>
	///     Use the Stable channel (<c>discord.com</c>). This is the default.
	/// </summary>
	Stable = 0,

	/// <summary>
	///     Use the PTB channel (<c>ptb.discord.com</c>).
	/// </summary>
	Ptb = 1,

	/// <summary>
	///     Use the Canary channel (<c>canary.discord.com</c>).
	/// </summary>
	Canary = 2,

	/// <summary>
	///     Use the Staging channel (<c>staging.discord.co</c>).
	/// </summary>
	Staging = 3
}
