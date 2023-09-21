using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildScheduledEventUserRemoved"/> event.
/// </summary>
public class GuildScheduledEventUserRemoveEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the scheduled event.
	/// </summary>
	public DiscordScheduledEvent ScheduledEvent { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the user which has unsubscribed from this scheduled event.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the member which has unsubscribed from this scheduled event.
	/// </summary>
	public DiscordMember Member { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildScheduledEventUserRemoveEventArgs"/> class.
	/// </summary>
	internal GuildScheduledEventUserRemoveEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
