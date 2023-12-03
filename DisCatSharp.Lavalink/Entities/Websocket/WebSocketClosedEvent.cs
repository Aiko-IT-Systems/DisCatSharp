using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a websocket close event.
/// </summary>
internal sealed class WebSocketClosedEvent : EventOp
{
	/// <summary>
	/// Gets the Discord close event code.
	/// </summary>
	[JsonProperty("code")]
	internal int Code { get; set; }

	/// <summary>
	/// Gets the close reason.
	/// </summary>
	[JsonProperty("reason")]
	internal string Reason { get; set; }

	/// <summary>
	/// Gets whether the connection was closed by Discord.
	/// </summary>
	[JsonProperty("byRemote")]
	internal bool ByRemote { get; set; }
}
