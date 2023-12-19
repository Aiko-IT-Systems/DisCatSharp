using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildScheduledEventDeleted"/> event.
/// </summary>
public class GuildScheduledEventDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the scheduled event that was deleted.
	/// </summary>
	public DiscordScheduledEvent ScheduledEvent { get; internal set; }

	/// <summary>
	/// Gets the reason of deletion for the scheduled event.
	/// Important to determine why and how it was deleted.
	/// </summary>
	public ScheduledEventStatus Reason { get; internal set; }

	/// <summary>
	/// Gets the guild in which the scheduled event was deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildScheduledEventDeleteEventArgs"/> class.
	/// </summary>
	internal GuildScheduledEventDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
