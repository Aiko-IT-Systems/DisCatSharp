namespace DisCatSharp.VoiceNext;

/// <summary>
/// Represents encoder settings preset for Opus.
/// </summary>
public enum VoiceApplication
{
	/// <summary>
	/// Defines that the encoder must optimize settings for voice data.
	/// </summary>
	Voice = 2048,

	/// <summary>
	/// Defines that the encoder must optimize settings for music data.
	/// </summary>
	Music = 2049,

	/// <summary>
	/// Defines that the encoder must optimize settings for low latency applications.
	/// </summary>
	LowLatency = 2051
}
