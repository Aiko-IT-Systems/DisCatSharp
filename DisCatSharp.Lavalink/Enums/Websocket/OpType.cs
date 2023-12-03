using System.Runtime.Serialization;

using DisCatSharp.Lavalink.Entities;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DisCatSharp.Lavalink.Enums.Websocket;

/// <summary>
/// Represents various lavalink op types.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
internal enum OpType
{
	/// <summary>
	/// Indicates that the lavalink session is ready. Fires <see cref="LavalinkSession.LavalinkSessionConnected"/>.
	/// </summary>
	[EnumMember(Value = "ready")]
	Ready,

	/// <summary>
	/// Indicates that a <see cref="LavalinkPlayer"/> got updated. Fires <see cref="LavalinkSession.StatsReceived"/>.
	/// </summary>
	[EnumMember(Value = "playerUpdate")]
	PlayerUpdate,

	/// <summary>
	/// Indicates that the session stats got updated. Fires <see cref="LavalinkSession.StatsReceived"/>.
	/// </summary>
	[EnumMember(Value = "stats")]
	Stats,

	/// <summary>
	/// Indicates that the op contains further information about an event.
	/// </summary>
	[EnumMember(Value = "event")]
	Event
}
