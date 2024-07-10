using System;

using DisCatSharp.Lavalink.Entities.Websocket;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents lavalink server statistics.
/// </summary>
public sealed class LavalinkStats
{
	/// <summary>
	/// Gets the total count of players.
	/// </summary>
	[JsonProperty("players")]
	public int Players { get; internal set; }

	/// <summary>
	/// Gets the count of playing (active) players.
	/// </summary>
	[JsonProperty("playingPlayers")]
	public int PlayingPlayers { get; internal set; }

	/// <summary>
	/// Gets the uptime <see cref="TimeSpan"/> of the lavalink server.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Uptime => TimeSpan.FromMilliseconds(this._uptime);

	/// <summary>
	/// Gets the uptime of the lavalink server.
	/// </summary>
	[JsonProperty("uptime")]
	private readonly long _uptime;

	/// <summary>
	/// Gets the memory stats.
	/// </summary>
	[JsonProperty("memory")]
	public MemoryStats Memory { get; internal set; }

	/// <summary>
	/// Gets the cpu stats.
	/// </summary>
	[JsonProperty("cpu")]
	public CpuStats Cpu { get; internal set; }

	/// <summary>
	/// Gets or sets the frames.
	/// </summary>
	[JsonProperty("frameStats")]
	public FrameStats? Frames { get; internal set; }

	/// <summary>
	///  Constructs an empty <see cref="LavalinkStats"/> object.
	/// </summary>
	[JsonConstructor]
	internal LavalinkStats()
	{ }

	/// <summary>
	/// Constructs a new <see cref="LavalinkStats"/> object from a <see cref="StatsOp"/> received via websocket.
	/// <para>This includes <see cref="Frames"/>.</para>
	/// </summary>
	/// <param name="websocketStats">The gateway stats.</param>
	internal LavalinkStats(StatsOp websocketStats)
	{
		this.Players = websocketStats.Players;
		this.PlayingPlayers = websocketStats.PlayingPlayers;
		this._uptime = websocketStats.UptimeInt;
		this.Memory = websocketStats.Memory;
		this.Cpu = websocketStats.Cpu;
		this.Frames = websocketStats.Frames;
	}

	/// <summary>
	/// Constructs a new <see cref="LavalinkStats"/> object from a <see cref="LavalinkStats"/> received via rest.
	/// <para>Pulls <see cref="Frames"/> from existing <see cref="LavalinkStats"/> received via gateway as <see cref="StatsOp"/>.</para>
	/// </summary>
	/// <param name="restStats">The rest stats object.</param>
	/// <param name="websocketStats">The gateway stats object (previous stats object).</param>
	internal LavalinkStats(LavalinkStats restStats, LavalinkStats? websocketStats)
	{
		this.Players = restStats.Players;
		this.PlayingPlayers = restStats.PlayingPlayers;
		this._uptime = restStats._uptime;
		this.Memory = restStats.Memory;
		this.Cpu = restStats.Cpu;
		this.Frames = websocketStats?.Frames;
	}
}

/// <summary>
/// Represents memory stats.
/// </summary>
public sealed class MemoryStats
{
	/// <summary>
	/// Gets the reservable memory.
	/// </summary>
	[JsonProperty("reservable")]
	public long Reservable { get; internal set; }

	/// <summary>
	/// Gets the used memory.
	/// </summary>
	[JsonProperty("used")]
	public long Used { get; internal set; }

	/// <summary>
	/// Gets the free memory.
	/// </summary>
	[JsonProperty("free")]
	public long Free { get; internal set; }

	/// <summary>
	/// Gets the allocated memory.
	/// </summary>
	[JsonProperty("allocated")]
	public long Allocated { get; internal set; }
}

/// <summary>
/// Represents cpu stats.
/// </summary>
public sealed class CpuStats
{
	/// <summary>
	/// Gets the cores.
	/// </summary>
	[JsonProperty("cores")]
	public int Cores { get; internal set; }

	/// <summary>
	/// Gets the system load.
	/// </summary>
	[JsonProperty("systemLoad")]
	public float SystemLoad { get; internal set; }

	/// <summary>
	/// Gets the lavalink load.
	/// </summary>
	[JsonProperty("lavalinkLoad")]
	public float LavalinkLoad { get; internal set; }
}

/// <summary>
/// Represents frame stats.
/// </summary>
public sealed class FrameStats
{
	/// <summary>
	/// Gets the sent frames count.
	/// </summary>
	[JsonProperty("sent")]
	public int Sent { get; internal set; }

	/// <summary>
	/// Gets the nulled frames count.
	/// </summary>
	[JsonProperty("nulled")]
	public int Nulled { get; internal set; }

	/// <summary>
	/// Gets the deficit.
	/// </summary>
	[JsonProperty("deficit")]
	public int Deficit { get; internal set; }
}
