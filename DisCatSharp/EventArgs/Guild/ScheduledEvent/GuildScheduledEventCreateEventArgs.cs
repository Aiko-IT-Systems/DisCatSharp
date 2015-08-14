using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildScheduledEventCreated"/> event.
/// </summary>
public class GuildScheduledEventCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the scheduled event that was created.
	/// </summary>
	public DiscordScheduledEvent ScheduledEvent { get; internal set; }

	/// <summary>
	/// Gets the guild in which the scheduled event was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildScheduledEventCreateEventArgs"/> class.
	/// </summary>
	internal GuildScheduledEventCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
