using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildDownloadCompleted"/> event.
/// </summary>
public class GuildDownloadCompletedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the dictionary of guilds that just finished downloading.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

	/// <summary>
	/// If <see langword="true"/>, the bot isn't in any guilds (yet).
	/// </summary>
	public bool NoGuilds { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildDownloadCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="guilds">The guilds.</param>
	/// <param name="noGuilds">Whether the bot isn't in any guilds..</param>
	/// <param name="provider">Service provider.</param>
	internal GuildDownloadCompletedEventArgs(IReadOnlyDictionary<ulong, DiscordGuild> guilds, bool noGuilds, IServiceProvider provider)
		: base(provider)
	{
		this.Guilds = guilds;
		this.NoGuilds = noGuilds;
	}
}
