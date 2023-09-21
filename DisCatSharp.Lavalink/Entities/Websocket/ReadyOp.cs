using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a ready op.
/// </summary>
internal sealed class ReadyOp : LavalinkOp
{
	/// <summary>
	/// Gets whether the session was resumed.
	/// </summary>
	[JsonProperty("resumed")]
	internal bool Resumed { get; set; }

	/// <summary>
	/// Gets the lavalink session id.
	/// </summary>
	[JsonProperty("sessionId")]
	internal string SessionId { get; set; }
}
