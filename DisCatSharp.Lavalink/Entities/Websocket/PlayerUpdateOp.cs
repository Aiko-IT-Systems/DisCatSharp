using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents a player update op.
/// </summary>
internal sealed class PlayerUpdateOp : LavalinkOp
{
	/// <summary>
	/// Gets the related guild id.
	/// </summary>
	[JsonProperty("guildId")]
	internal ulong GuildId { get; set; }

	/// <summary>
	/// Gets the updated player state.
	/// </summary>
	[JsonProperty("state")]
	internal LavalinkPlayerState State { get; set; }
}
