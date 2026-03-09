using System;

namespace DisCatSharp.Voice.Enums;

/// <summary>
///     Controls outbound audio behavior while DAVE is negotiated but not active.
/// </summary>
public enum DavePendingAudioBehavior
{
	/// <summary>
	///     Send plain Opus frames over transport encryption until DAVE becomes active.
	/// </summary>
	PassThrough = 0,

	/// <summary>
	///     Drop outbound audio frames until DAVE is active.
	/// </summary>
	Drop = 1,

	/// <summary>
	///     Throw <see cref="InvalidOperationException"/> when an outbound frame is prepared while DAVE is not active.
	/// </summary>
	Throw = 2
}
