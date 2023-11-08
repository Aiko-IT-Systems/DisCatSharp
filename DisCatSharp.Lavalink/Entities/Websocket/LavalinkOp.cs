using DisCatSharp.Lavalink.Enums.Websocket;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a lavalink op.
/// </summary>
internal class LavalinkOp
{
	/// <summary>
	/// Gets the op type.
	/// </summary>
	[JsonProperty("op")]
	internal OpType Op { get; set; }
}
