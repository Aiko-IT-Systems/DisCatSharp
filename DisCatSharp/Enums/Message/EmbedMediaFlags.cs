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
	/// <returns>True if the specified flag is present; otherwise, false.</returns>
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
	///     This embed media is a clip.
	/// </summary>
	IsClip = 1 << 0,

	/// <summary>
	///     This embed media is a thumbnail.
	/// </summary>
	IsThumbnail = 1 << 1,

	/// <summary>
	///     This embed media is a remix.
	/// </summary>
	IsRemix = 1 << 2,

	/// <summary>
	///     This embed media is marked as a spoiler.
	/// </summary>
	IsSpoiler = 1 << 3,

	/// <summary>
	///     This embed media contains explicit content.
	/// </summary>
	ContainsExplicitMedia = 1 << 4,

	/// <summary>
	///     This embed media is animated.
	/// </summary>
	IsAnimated = 1 << 5
}
