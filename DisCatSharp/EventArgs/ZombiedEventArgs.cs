using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.Zombied"/> event.
/// </summary>
public class ZombiedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets how many heartbeat failures have occurred.
	/// </summary>
	public int Failures { get; internal set; }

	/// <summary>
	/// Gets whether the zombie event occurred whilst guilds are downloading.
	/// </summary>
	public bool GuildDownloadCompleted { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ZombiedEventArgs"/> class.
	/// </summary>
	internal ZombiedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
