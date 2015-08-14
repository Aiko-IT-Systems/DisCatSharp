using System.Threading.Tasks;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents an interface for using the built-in DisCatSharp Lavalink queue.
/// </summary>
public interface IQueueEntry
{
	/// <summary>
	/// The lavalink track to play.
	/// </summary>
	LavalinkTrack Track { get; set; }

	/// <summary>
	/// Adds a track.
	/// </summary>
	/// <param name="track">The track to add.</param>
	/// <returns>The queue entry.</returns>
	public IQueueEntry AddTrack(LavalinkTrack track)
	{
		this.Track = track;
		return this;
	}

	/// <summary>
	/// Actions to execute before this queue entry gets played.
	/// Return <see langword="false"/> if entry shouldn't be played.
	/// </summary>
	abstract Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player);

	/// <summary>
	/// Actions to execute after this queue entry was played.
	/// </summary>
	abstract Task AfterPlayingAsync(LavalinkGuildPlayer player);
}
