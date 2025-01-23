using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a attachment flag extensions.
/// </summary>
public static class EmbedFlagExtensions
{
	/// <summary>
	///     Calculates whether these attachment flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasEmbedFlag(this EmbedFlags baseFlags, EmbedFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional features of a attachment.
/// </summary>
[Flags]
public enum EmbedFlags
{
	/// <summary>
	///     This embed has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     This embed contains explicit media.
	/// </summary>
	ContainsExplicitMedia = 1 << 4,

	/// <summary>
	///     This embed is a content inventory entry.
	/// </summary>
	IsContentInventoryEntry = 1 << 5
}
