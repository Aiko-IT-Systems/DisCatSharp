using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a track start event.
/// </summary>
internal sealed class TrackStartEvent : EventOp
{
	/// <summary>
	/// Gets the lavalink track.
	/// </summary>
	[JsonProperty("track")]
	internal LavalinkTrack Track { get; set; }
}
