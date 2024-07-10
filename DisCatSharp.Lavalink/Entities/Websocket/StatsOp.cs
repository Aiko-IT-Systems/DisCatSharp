using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities.Websocket;

/// <summary>
/// Represents lavalink server statistics received via websocket.
/// </summary>
internal sealed class StatsOp : LavalinkOp
{
	/// <summary>
	/// Gets the total count of players.
	/// </summary>
	[JsonProperty("players")]
	internal int Players { get; set; }

	/// <summary>
	/// Gets the count of playing (active) players.
	/// </summary>
	[JsonProperty("playingPlayers")]
	internal int PlayingPlayers { get; set; }

	/// <summary>
	/// Gets or sets the uptime.
	/// </summary>
	[JsonProperty("uptime")]
	internal readonly long UptimeInt;

	/// <summary>
	/// Gets or sets the memory stats.
	/// </summary>
	[JsonProperty("memory")]
	internal MemoryStats Memory { get; set; }

	/// <summary>
	/// Gets or sets the cpu stats.
	/// </summary>
	[JsonProperty("cpu")]
	internal CpuStats Cpu { get; set; }

	/// <summary>
	/// Gets or sets the frame stats.
	/// </summary>
	[JsonProperty("frameStats")]
	internal FrameStats Frames { get; set; }
}
