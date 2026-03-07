namespace DisCatSharp.Voice;

/// <summary>
///     Controls outbound audio behavior while DAVE is negotiated but not yet active.
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
	Drop = 1
}
