using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a attachment flag extensions.
/// </summary>
public static class EmbedMediaFlagExtensions
{
	/// <summary>
	///     Calculates whether these attachment flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasEmbedMediaFlag(this EmbedMediaFlags baseFlags, EmbedMediaFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional features of a attachment.
/// </summary>
[Flags]
public enum EmbedMediaFlags
{
	/// <summary>
	///     This embed media has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     This embed media is animated.
	/// </summary>
	IsAnimated = 1 << 5
}
