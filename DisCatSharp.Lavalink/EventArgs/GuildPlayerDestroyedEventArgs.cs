using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for guild player destroy events.
/// </summary>
public sealed class GuildPlayerDestroyedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the destroyed guild player.
	/// </summary>
	public LavalinkGuildPlayer Player { get; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild
		=> this.Discord.GuildsInternal.TryGetValue(this.Player.GuildId, out var guild) ? guild : null!;

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildPlayerDestroyedEventArgs"/> class.
	/// </summary>
	/// <param name="player">The player.</param>
	internal GuildPlayerDestroyedEventArgs(LavalinkGuildPlayer player)
		: base(player.Discord.ServiceProvider)
	{
		this.Discord = player.Discord;
		this.Player = player;
	}
}
