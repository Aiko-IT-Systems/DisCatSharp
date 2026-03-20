using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildScheduledEventExceptionDeleted" /> event.
/// </summary>
public sealed class GuildScheduledEventExceptionDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildScheduledEventExceptionDeleteEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	internal GuildScheduledEventExceptionDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the scheduled event exception that was deleted.
	/// </summary>
	public DiscordScheduledEventException Exception { get; internal set; }

	/// <summary>
	///     Gets the guild in which the exception was deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
