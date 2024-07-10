using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink session connection.
/// </summary>
public sealed class LavalinkSessionConnectedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the session that was connected.
	/// </summary>
	public LavalinkSession Session { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkSessionConnectedEventArgs"/> class.
	/// </summary>
	/// <param name="session">The session.</param>
	internal LavalinkSessionConnectedEventArgs(LavalinkSession session)
		: base(session.Discord.ServiceProvider)
	{
		this.Discord = session.Discord;
		this.Session = session;
	}
}
