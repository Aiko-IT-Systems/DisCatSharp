
using System;
using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// The lavalink state.
/// </summary>
internal sealed class LavalinkState
{
	/// <summary>
	/// Gets the time.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Time => Utilities.GetDateTimeOffsetFromMilliseconds(this._time);
	[JsonProperty("time")]
	private readonly long _time;

	/// <summary>
	/// Gets the position.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Position => TimeSpan.FromMilliseconds(this._position);
	[JsonProperty("position")]
	private readonly long _position;

	[JsonProperty("connected")]
	public bool IsConnected;
}

/// <summary>
/// Represents current state of given player.
/// </summary>
public sealed class LavalinkPlayerState
{
	/// <summary>
	/// Gets the timestamp at which this state was last updated.
	/// </summary>
	public DateTimeOffset LastUpdate { get; internal set; }

	/// <summary>
	/// Gets the current playback position.
	/// </summary>
	public TimeSpan PlaybackPosition { get; internal set; }

	/// <summary>
	/// Gets the currently-played track.
	/// </summary>
	public LavalinkTrack CurrentTrack { get; internal set; }

	/// <summary>
	/// Gets whether the player is currently connected to the voice gateway.
	/// </summary>
	public bool IsConnected { get; internal set; }
}

/// <summary>
/// The lavalink stats.
/// </summary>
internal sealed class LavalinkStats
{
	/// <summary>
	/// Gets or sets the active players.
	/// </summary>
	[JsonProperty("playingPlayers")]
	public int ActivePlayers { get; set; }

	/// <summary>
	/// Gets or sets the total players.
	/// </summary>
	[JsonProperty("players")]
	public int TotalPlayers { get; set; }

	/// <summary>
	/// Gets the uptime.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Uptime => TimeSpan.FromMilliseconds(this._uptime);
	[JsonProperty("uptime")]
	private readonly long _uptime;

	/// <summary>
	/// Gets or sets the cpu.
	/// </summary>
	[JsonProperty("cpu")]
	public CpuStats Cpu { get; set; }

	/// <summary>
	/// Gets or sets the memory.
	/// </summary>
	[JsonProperty("memory")]
	public MemoryStats Memory { get; set; }

	/// <summary>
	/// Gets or sets the frames.
	/// </summary>
	[JsonProperty("frameStats")]
	public FrameStats Frames { get; set; }

	/// <summary>
	/// The cpu stats.
	/// </summary>
	internal sealed class CpuStats
	{
		/// <summary>
		/// Gets or sets the cores.
		/// </summary>
		[JsonProperty("cores")]
		public int Cores { get; set; }

		/// <summary>
		/// Gets or sets the system load.
		/// </summary>
		[JsonProperty("systemLoad")]
		public double SystemLoad { get; set; }

		/// <summary>
		/// Gets or sets the lavalink load.
		/// </summary>
		[JsonProperty("lavalinkLoad")]
		public double LavalinkLoad { get; set; }
	}

	/// <summary>
	/// The memory stats.
	/// </summary>
	internal sealed class MemoryStats
	{
		/// <summary>
		/// Gets or sets the reservable.
		/// </summary>
		[JsonProperty("reservable")]
		public long Reservable { get; set; }

		/// <summary>
		/// Gets or sets the used.
		/// </summary>
		[JsonProperty("used")]
		public long Used { get; set; }

		/// <summary>
		/// Gets or sets the free.
		/// </summary>
		[JsonProperty("free")]
		public long Free { get; set; }

		/// <summary>
		/// Gets or sets the allocated.
		/// </summary>
		[JsonProperty("allocated")]
		public long Allocated { get; set; }
	}

	/// <summary>
	/// The frame stats.
	/// </summary>
	internal sealed class FrameStats
	{
		/// <summary>
		/// Gets or sets the sent.
		/// </summary>
		[JsonProperty("sent")]
		public int Sent { get; set; }

		/// <summary>
		/// Gets or sets the nulled.
		/// </summary>
		[JsonProperty("nulled")]
		public int Nulled { get; set; }

		/// <summary>
		/// Gets or sets the deficit.
		/// </summary>
		[JsonProperty("deficit")]
		public int Deficit { get; set; }
	}
}

/// <summary>
/// Represents statistics of Lavalink resource usage.
/// </summary>
public sealed class LavalinkStatistics
{
	/// <summary>
	/// Gets the number of currently-playing players.
	/// </summary>
	public int ActivePlayers { get; private set; }

	/// <summary>
	/// Gets the total number of players.
	/// </summary>
	public int TotalPlayers { get; private set; }

	/// <summary>
	/// Gets the node uptime.
	/// </summary>
	public TimeSpan Uptime { get; private set; }

	/// <summary>
	/// Gets the number of CPU cores available.
	/// </summary>
	public int CpuCoreCount { get; private set; }

	/// <summary>
	/// Gets the total % of CPU resources in use on the system.
	/// </summary>
	public double CpuSystemLoad { get; private set; }

	/// <summary>
	/// Gets the total % of CPU resources used by lavalink.
	/// </summary>
	public double CpuLavalinkLoad { get; private set; }

	/// <summary>
	/// Gets the amount of reservable RAM, in bytes.
	/// </summary>
	public long RamReservable { get; private set; }

	/// <summary>
	/// Gets the amount of used RAM, in bytes.
	/// </summary>
	public long RamUsed { get; private set; }

	/// <summary>
	/// Gets the amount of free RAM, in bytes.
	/// </summary>
	public long RamFree { get; private set; }

	/// <summary>
	/// Gets the amount of allocated RAM, in bytes.
	/// </summary>
	public long RamAllocated { get; private set; }

	/// <summary>
	/// Gets the average number of sent frames per minute.
	/// </summary>
	public int AverageSentFramesPerMinute { get; private set; }

	/// <summary>
	/// Gets the average number of frames that were sent as null per minute.
	/// </summary>
	public int AverageNulledFramesPerMinute { get; private set; }

	/// <summary>
	/// Gets the average frame deficit per minute.
	/// </summary>
	public int AverageDeficitFramesPerMinute { get; private set; }

	internal bool Updated;

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkStatistics"/> class.
	/// </summary>
	internal LavalinkStatistics()
	{
		this.Updated = false;
	}

	/// <summary>
	/// Updates the stats.
	/// </summary>
	/// <param name="newStats">The new stats.</param>
	internal void Update(LavalinkStats newStats)
	{
		if (!this.Updated)
			this.Updated = true;

		this.ActivePlayers = newStats.ActivePlayers;
		this.TotalPlayers = newStats.TotalPlayers;
		this.Uptime = newStats.Uptime;

		this.CpuCoreCount = newStats.Cpu.Cores;
		this.CpuSystemLoad = newStats.Cpu.SystemLoad;
		this.CpuLavalinkLoad = newStats.Cpu.LavalinkLoad;

		this.RamReservable = newStats.Memory.Reservable;
		this.RamUsed = newStats.Memory.Used;
		this.RamFree = newStats.Memory.Free;
		this.RamAllocated = newStats.Memory.Allocated;
		this.RamReservable = newStats.Memory.Reservable;

		this.AverageSentFramesPerMinute = newStats.Frames?.Sent ?? 0;
		this.AverageNulledFramesPerMinute = newStats.Frames?.Nulled ?? 0;
		this.AverageDeficitFramesPerMinute = newStats.Frames?.Deficit ?? 0;
	}
}
