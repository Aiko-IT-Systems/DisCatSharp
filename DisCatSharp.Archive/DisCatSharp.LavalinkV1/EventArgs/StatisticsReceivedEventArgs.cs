using System;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink.Entities;

namespace DisCatSharp.Lavalink.EventArgs;

/// <summary>
/// Represents arguments for Lavalink statistics received.
/// </summary>
public sealed class StatisticsReceivedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the Lavalink statistics received.
	/// </summary>
	public LavalinkStatistics Statistics { get; }


	/// <summary>
	/// Initializes a new instance of the <see cref="StatisticsReceivedEventArgs"/> class.
	/// </summary>
	/// <param name="provider">Service provider.</param>
	/// <param name="stats">The stats.</param>
	internal StatisticsReceivedEventArgs(IServiceProvider provider, LavalinkStatistics stats) : base(provider)
	{
		this.Statistics = stats;
	}
}
