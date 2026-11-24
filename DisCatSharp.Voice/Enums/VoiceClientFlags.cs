using System;

namespace DisCatSharp.Voice.Enums;

/// <summary>
///     User-client-reported Discord Clips preferences received from the voice gateway.
/// </summary>
/// <remarks>
///     These flags are supplied by the user's Discord client and are not verified by Discord. They only describe
///     Discord Clips preferences and must not be interpreted as consent for an application to record voice.
/// </remarks>
[Flags]
public enum VoiceClientFlags
{
	/// <summary>
	///     No Clips preferences were reported.
	/// </summary>
	None = 0,

	/// <summary>
	///     The user has Discord Clips enabled.
	/// </summary>
	ClipsEnabled = 1 << 0,

	/// <summary>
	///     The user allows their voice in other users' Discord Clips.
	/// </summary>
	AllowVoiceRecording = 1 << 1,

	/// <summary>
	///     The user allows anyone viewing their stream to create Discord Clips.
	/// </summary>
	AllowAnyViewerClips = 1 << 2
}
