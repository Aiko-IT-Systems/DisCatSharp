using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildScheduledEventExceptionCreated" /> event.
/// </summary>
public sealed class GuildScheduledEventExceptionCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildScheduledEventExceptionCreateEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	internal GuildScheduledEventExceptionCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the scheduled event exception that was created.
	/// </summary>
	public DiscordScheduledEventException Exception { get; internal set; }

	/// <summary>
	///     Gets the guild in which the exception was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
