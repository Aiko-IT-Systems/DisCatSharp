using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
///     Represents a lyric line.
/// </summary>
public sealed class LyricsLine
{
	/// <summary>
	///     Gets the timestamp of the line in milliseconds.
	/// </summary>
	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public int Timestamp { get; internal set; }

	/// <summary>
	///     Gets the duration of the line in milliseconds.
	/// </summary>
	[JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
	public int? Duration { get; internal set; }

	/// <summary>
	///     Gets the lyrics line.
	/// </summary>
	[JsonProperty("line", NullValueHandling = NullValueHandling.Ignore)]
	public string Line { get; internal set; }

	/// <summary>
	///     Gets additional plugin specific data.
	/// </summary>
	[JsonProperty("plugin", NullValueHandling = NullValueHandling.Ignore)]
	public LavalinkPluginInfo Plugin { get; internal set; }
}
