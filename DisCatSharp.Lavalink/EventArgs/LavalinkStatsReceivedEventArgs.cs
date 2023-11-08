using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink stats events .
/// </summary>
public sealed class LavalinkStatsReceivedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the updated statistics.
	/// </summary>
	public LavalinkStats Statistics { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkTrackEndedEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="stats">The received statistics.</param>
	internal LavalinkStatsReceivedEventArgs(DiscordClient client, LavalinkStats stats)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.Statistics = stats;
	}
}
