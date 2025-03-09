using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a embed flag extensions.
/// </summary>
public static class EmbedFlagExtensions
{
	/// <summary>
	///     Calculates whether these embed flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasEmbedFlag(this EmbedFlags baseFlags, EmbedFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional features of a embed.
/// </summary>
[Flags]
public enum EmbedFlags : long
{
	/// <summary>
	///     This embed has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     This embed contains explicit media.
	/// </summary>
	ContainsExplicitMedia = 1L << 4,

	/// <summary>
	///     This embed is a content inventory entry.
	/// </summary>
	IsContentInventoryEntry = 1L << 5,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
