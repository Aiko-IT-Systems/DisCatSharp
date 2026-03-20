using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildScheduledEventExceptionUpdated" /> event.
/// </summary>
public sealed class GuildScheduledEventExceptionUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildScheduledEventExceptionUpdateEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	internal GuildScheduledEventExceptionUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the scheduled event exception before the update, if it was present in cache.
	/// </summary>
	public DiscordScheduledEventException? ExceptionBefore { get; internal set; }

	/// <summary>
	///     Gets the scheduled event exception after the update.
	/// </summary>
	public DiscordScheduledEventException ExceptionAfter { get; internal set; }

	/// <summary>
	///     Gets the guild in which the exception was updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
