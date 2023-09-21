using DisCatSharp.EventArgs;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents event arguments for lavalink session disconnection.
/// </summary>
public sealed class LavalinkSessionDisconnectedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	public DiscordClient Discord { get; }

	/// <summary>
	/// Gets the session that was disconnected.
	/// </summary>
	public LavalinkSession Session { get; }

	/// <summary>
	/// Gets whether disconnect was clean.
	/// </summary>
	public bool IsCleanClose { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LavalinkSessionDisconnectedEventArgs"/> class.
	/// </summary>
	/// <param name="session">The session.</param>
	/// <param name="isClean">If true, is clean.</param>
	internal LavalinkSessionDisconnectedEventArgs(LavalinkSession session, bool isClean)
		: base(session.Discord.ServiceProvider)
	{
		this.Discord = session.Discord;
		this.Session = session;
		this.IsCleanClose = isClean;
	}
}
