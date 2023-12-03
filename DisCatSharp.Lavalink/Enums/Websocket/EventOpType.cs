using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums.Websocket;

/// <summary>
/// Represents various event op types.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
internal enum EventOpType
{
	/// <summary>
	/// Event indicates that a track started. Fires <see cref="LavalinkGuildPlayer.TrackStarted"/>.
	/// </summary>
	[EnumMember(Value = "TrackStartEvent")]
	TrackStartEvent,

	/// <summary>
	/// Event indicates that a track ended. Fires <see cref="LavalinkGuildPlayer.TrackEnded"/>.
	/// </summary>
	[EnumMember(Value = "TrackEndEvent")]
	TrackEndEvent,

	/// <summary>
	/// Event indicates that a track encountered an exception. Fires <see cref="LavalinkGuildPlayer.TrackException"/>.
	/// </summary>
	[EnumMember(Value = "TrackExceptionEvent")]
	TrackExceptionEvent,

	/// <summary>
	/// Event indicates that a track got stuck. Fires <see cref="LavalinkGuildPlayer.TrackStuck"/>.
	/// </summary>
	[EnumMember(Value = "TrackStuckEvent")]
	TrackStuckEvent,

	/// <summary>
	/// Event indicates that a track started. Executes <see cref="LavalinkSession.Lavalink_WebSocket_Disconnected"/>.
	/// </summary>
	[EnumMember(Value = "WebSocketClosedEvent")]
	WebsocketClosedEvent
}
