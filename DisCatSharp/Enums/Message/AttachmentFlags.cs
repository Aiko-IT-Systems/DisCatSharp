using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a attachment flag extensions.
/// </summary>
public static class AttachmentFlagExtensions
{
	/// <summary>
	///     Calculates whether these attachment flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasAttachmentFlag(this AttachmentFlags baseFlags, AttachmentFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents additional features of a attachment.
/// </summary>
[Flags]
public enum AttachmentFlags
{
	/// <summary>
	///     This attachment has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     This attachment is a clip.
	/// </summary>
	IsClip = 1 << 0,

	/// <summary>
	///     This attachment is a thumbnail.
	/// </summary>
	IsThumbnail = 1 << 1,

	/// <summary>
	///     This attachment was edited with remix (https://support.discord.com/hc/en-us/articles/15145601963031).
	/// </summary>
	IsRemix = 1 << 2,

	/// <summary>
	///     This attachment is a spoiler.
	/// </summary>
	IsSpoiler = 1 << 3,

	/// <summary>
	///     This attachment contains explicit media.
	/// </summary>
	ContainsExplicitMedia = 1 << 4,

	/// <summary>
	///     This attachment is animated.
	/// </summary>
	IsAnimated = 1 << 5
}
