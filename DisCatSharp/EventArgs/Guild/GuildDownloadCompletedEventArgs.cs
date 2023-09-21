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
	/// Initializes a new instance of the <see cref="GuildDownloadCompletedEventArgs"/> class.
	/// </summary>
	/// <param name="guilds">The guilds.</param>
	/// <param name="provider">Service provider.</param>
	internal GuildDownloadCompletedEventArgs(IReadOnlyDictionary<ulong, DiscordGuild> guilds, IServiceProvider provider)
		: base(provider)
	{
		this.Guilds = guilds;
	}
}
