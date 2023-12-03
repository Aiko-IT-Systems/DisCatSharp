using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a track stuck event.
/// </summary>
internal sealed class TrackStuckEvent : EventOp
{
	/// <summary>
	/// Gets the lavalink track.
	/// </summary>
	[JsonProperty("track")]
	internal LavalinkTrack Track { get; set; }

	/// <summary>
	/// Gets the threshold in milliseconds that was exceeded.
	/// </summary>
	[JsonProperty("thresholdMs")]
	internal long ThresholdMs { get; set; }
}
