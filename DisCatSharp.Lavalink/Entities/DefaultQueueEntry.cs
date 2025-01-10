using System.Threading.Tasks;

namespace DisCatSharp.Lavalink.Entities;
public class DefaultQueueEntry : IQueueEntry
{
	public LavalinkTrack Track { get; set; }

	public Task<bool> BeforePlayingAsync(LavalinkGuildPlayer player) => Task.FromResult(true);

	public Task AfterPlayingAsync(LavalinkGuildPlayer player) => Task.CompletedTask;
}
