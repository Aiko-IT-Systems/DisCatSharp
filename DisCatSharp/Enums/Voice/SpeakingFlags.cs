using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a speaking flag extensions.
/// </summary>
public static class SpeakingFlagExtensions
{
	/// <summary>
	/// Calculates whether these speaking flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasSpeakingFlag(this SpeakingFlags baseFlags, SpeakingFlags flag) => (baseFlags & flag) == flag;
}

[Flags]
public enum SpeakingFlags
{
	/// <summary>
	/// Not speaking.
	/// </summary>
	NotSpeaking = 0,

	/// <summary>
	/// Normal transmission of voice audio.
	/// </summary>
	Microphone = 1 << 0,

	/// <summary>
	/// Transmission of context audio for video, no speaking indicator.
	/// </summary>
	Soundshare = 1 << 1,

	/// <summary>
	/// Priority speaker, lowering audio of other speakers.
	/// </summary>
	Priority = 1 << 2
}
