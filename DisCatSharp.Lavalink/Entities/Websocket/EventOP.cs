using DisCatSharp.Lavalink.Enums.Websocket;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents an event op.
/// </summary>
internal class EventOp : LavalinkOp
{
	/// <summary>
	/// Gets the op type of event.
	/// </summary>
	[JsonProperty("type")]
	public EventOpType Type { get; internal set; }

	/// <summary>
	/// Gets the related guild id.
	/// </summary>
	[JsonProperty("guildId")]
	public string GuildId { get; internal set; }
}
