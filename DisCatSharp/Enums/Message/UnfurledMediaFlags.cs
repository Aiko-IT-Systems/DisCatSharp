using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a unfurled media flag extensions.
/// </summary>
public static class UnfurledMediaFlagExtensions
{
	/// <summary>
	///     Calculates whether these unfurled media flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasUnfurledMediaFlag(this UnfurledMediaFlags baseFlags, UnfurledMediaFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional features of unfurled media.
/// </summary>
[Flags]
public enum UnfurledMediaFlags
{
	/// <summary>
	///     This unfurled media has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = int.MaxValue
}
