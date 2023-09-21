using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// The event type.
/// </summary>
internal enum EventType
{
	/// <summary>
	/// Track start event
	/// </summary>
	TrackStartEvent,

	/// <summary>
	/// Track end event
	/// </summary>
	TrackEndEvent,

	/// <summary>
	/// Track exception event
	/// </summary>
	TrackExceptionEvent,

	/// <summary>
	/// Track stuck event
	/// </summary>
	TrackStuckEvent,

	/// <summary>
	/// Websocket closed event
	/// </summary>
	WebSocketClosedEvent
}

/// <summary>
/// Represents arguments for a track playback start event.
/// </summary>
public sealed class TrackStartEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the track that started playing.
	/// </summary>
	public LavalinkTrack Track { get; }

	/// <summary>
	/// Gets the player that started playback.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TrackStartEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="track">The track.</param>
	internal TrackStartEventArgs(LavalinkGuildConnection lvl, LavalinkTrack track) : base(lvl.Node.Discord.ServiceProvider)
	{
		this.Track = track;
		this.Player = lvl;
	}
}

/// <summary>
/// Represents track finish data
/// </summary>
internal struct TrackFinishData
{
	/// <summary>
	/// Gets or sets the track.
	/// </summary>
	public string Track { get; set; }
	/// <summary>
	/// Gets or sets the reason.
	/// </summary>
	public TrackEndReason Reason { get; set; }
}

/// <summary>
/// Represents arguments for a track playback finish event.
/// </summary>
public sealed class TrackFinishEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the track that finished playing.
	/// </summary>
	public LavalinkTrack Track { get; }

	/// <summary>
	/// Gets the reason why the track stopped playing.
	/// </summary>
	public TrackEndReason Reason { get; }

	/// <summary>
	/// Gets the player that finished playback.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TrackFinishEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="track">The track.</param>
	/// <param name="reason">The reason.</param>
	internal TrackFinishEventArgs(LavalinkGuildConnection lvl, LavalinkTrack track, TrackEndReason reason) : base(lvl.Node.Discord.ServiceProvider)
	{
		this.Track = track;
		this.Reason = reason;
		this.Player = lvl;
	}
}

/// <summary>
/// Represents a reason why a track finished playing.
/// </summary>
public enum TrackEndReason
{
	/// <summary>
	/// This means that the track itself emitted a terminator. This is usually caused by the track reaching the end,
	/// however it will also be used when it ends due to an exception.
	/// </summary>
	Finished,
	/// <summary>
	/// This means that the track failed to start, throwing an exception before providing any audio.
	/// </summary>
	LoadFailed,
	/// <summary>
	/// The track was stopped due to the player being stopped by either calling stop() or playTrack(null).
	/// </summary>
	Stopped,
	/// <summary>
	/// The track stopped playing because a new track started playing. Note that with this reason, the old track will still
	/// play until either its buffer runs out or audio from the new track is available.
	/// </summary>
	Replaced,
	/// <summary>
	/// The track was stopped because the cleanup threshold for the audio player was reached. This triggers when the amount
	/// of time passed since the last call to AudioPlayer#provide() has reached the threshold specified in player manager
	/// configuration. This may also indicate either a leaked audio player which was discarded, but not stopped.
	/// </summary>
	Cleanup
}

/// <summary>
/// Represents track stuck data
/// </summary>
internal struct TrackStuckData
{
	/// <summary>
	/// Gets or sets the threshold.
	/// </summary>
	public long Threshold { get; set; }
	/// <summary>
	/// Gets or sets the track.
	/// </summary>
	public string Track { get; set; }
}

/// <summary>
/// Represents event arguments for a track stuck event.
/// </summary>
public sealed class TrackStuckEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the millisecond threshold for the stuck event.
	/// </summary>
	public long ThresholdMilliseconds { get; }

	/// <summary>
	/// Gets the track that got stuck.
	/// </summary>
	public LavalinkTrack Track { get; }

	/// <summary>
	/// Gets the player that got stuck.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TrackStuckEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="thresholdMs">The threshold ms.</param>
	/// <param name="track">The track.</param>
	internal TrackStuckEventArgs(LavalinkGuildConnection lvl, long thresholdMs, LavalinkTrack track) : base(lvl.Node.Discord.ServiceProvider)
	{
		this.ThresholdMilliseconds = thresholdMs;
		this.Track = track;
		this.Player = lvl;
	}
}

/// <summary>
/// Represents track exception data
/// </summary>
internal struct TrackExceptionData
{
	/// <summary>
	/// Gets or sets the error.
	/// </summary>
	public string Error { get; set; }
	/// <summary>
	/// Gets or sets the track.
	/// </summary>
	public string Track { get; set; }
}

/// <summary>
/// Represents event arguments for a track exception event.
/// </summary>
public sealed class TrackExceptionEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the exception that occurred during playback.
	/// </summary>
	public LavalinkLoadFailedInfo Exception { get; }

	/// <summary>
	/// Gets the track that got stuck.
	/// </summary>
	public LavalinkTrack Track { get; }

	/// <summary>
	/// Gets the player that got stuck.
	/// </summary>
	public LavalinkGuildConnection Player { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TrackExceptionEventArgs"/> class.
	/// </summary>
	/// <param name="lvl">The lvl.</param>
	/// <param name="exception">The exception.</param>
	/// <param name="track">The track.</param>
	internal TrackExceptionEventArgs(LavalinkGuildConnection lvl, LavalinkLoadFailedInfo exception, LavalinkTrack track) : base(lvl.Node.Discord.ServiceProvider)
	{
		this.Exception = exception;
		this.Track = track;
		this.Player = lvl;
	}
}
