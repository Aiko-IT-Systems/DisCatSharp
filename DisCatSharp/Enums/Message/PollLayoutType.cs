using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents the layout type of a poll.
/// </summary>
public enum PollLayoutType
{
	/// <summary>
	///     The default layout type.
	/// </summary>
	Default = 1,

	/// <summary>
	/// Poll answers are images only.
	/// </summary>
	[Obsolete("Do not use", true), DiscordUnreleased]
    ImageOnlyAnswers = 2

}
