using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a track exception.
/// </summary>
internal sealed class TrackExceptionEvent : EventOp
{
	/// <summary>
	/// Gets the lavalink track.
	/// </summary>
	[JsonProperty("track")]
	internal LavalinkTrack Track { get; set; }

	/// <summary>
	/// Gets the track exception.
	/// </summary>
	[JsonProperty("exception")]
	internal TrackException Exception { get; set; }
}
