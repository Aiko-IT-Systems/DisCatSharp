using DisCatSharp.Lavalink.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a track end event.
/// </summary>
internal sealed class TrackEndEvent : EventOp
{
	/// <summary>
	/// Gets the lavalink track.
	/// </summary>
	[JsonProperty("track")]
	internal LavalinkTrack Track { get; set; }

	/// <summary>
	/// Gets the end reason.
	/// </summary>
	[JsonProperty("reason")]
	internal LavalinkTrackEndReason Reason { get; set; }
}
