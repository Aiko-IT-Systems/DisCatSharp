using System;

namespace DisCatSharp.Voice.Enums;

/// <summary>
///     Controls outbound audio behavior when no currently executing DAVE media mode is usable.
/// </summary>
public enum DavePendingAudioBehavior
{
	/// <summary>
	///     Sends plain Opus frames using Discord transport encryption while DAVE media is not ready.
	/// </summary>
	PassThrough = 0,

	/// <summary>
	///     Drops outbound audio frames while DAVE media is not ready.
	/// </summary>
	Drop = 1,

	/// <summary>
	///     Throws <see cref="InvalidOperationException"/> when an outbound frame is prepared while DAVE media is not ready.
	/// </summary>
	Throw = 2
}
