using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink player state updates.
/// </summary>
public sealed class LavalinkPlayerStateUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the player state.
	/// </summary>
	public LavalinkPlayerState State { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildPlayerDestroyedEventArgs"/> class.
	/// </summary>
	/// <param name="client">The discord client.</param>
	/// <param name="state">The state.</param>
	internal LavalinkPlayerStateUpdateEventArgs(DiscordClient client, LavalinkPlayerState state)
		: base(client.ServiceProvider)
	{
		this.Discord = client;
		this.State = state;
	}
}
