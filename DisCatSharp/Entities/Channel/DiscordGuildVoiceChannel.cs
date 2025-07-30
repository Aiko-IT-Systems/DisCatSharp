using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a base class for all guild voice-based channels.
/// </summary>
public abstract class DiscordGuildVoiceChannel : DiscordGuildChannel
{
	/// <summary>
	/// Gets the bitrate (in bits) of the voice channel.
	/// </summary>
	[JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
	public int? Bitrate { get; internal set; }

	/// <summary>
	/// Gets the user limit of the voice channel.
	/// </summary>
	[JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? UserLimit { get; internal set; }

	/// <summary>
	/// Gets the voice region id (deprecated, but present in API).
	/// </summary>
	[JsonProperty("rtc_region", NullValueHandling = NullValueHandling.Ignore)]
	public string RtcRegion { get; internal set; }

	/// <summary>
	/// Gets the video quality mode.
	/// </summary>
	[JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
	public int? VideoQualityMode { get; internal set; }
}
