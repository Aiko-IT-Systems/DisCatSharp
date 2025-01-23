using System.Threading.Tasks;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
///     Represents a default queue entry.
/// </summary>
public sealed class DefaultQueueEntry : IQueueEntry
{
	/// <inheritdoc />
	public LavalinkTrack Track { get; set; }

	/// <inheritdoc />
	public Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player)
		=> Task.FromResult(true);

	/// <inheritdoc />
	public Task AfterPlayingAsync(LavalinkGuildPlayer player)
		=> Task.CompletedTask;
}
