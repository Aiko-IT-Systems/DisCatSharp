using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadCreated"/> event.
/// </summary>
public class ThreadCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the thread that was created.
	/// </summary>
	public DiscordThreadChannel Thread { get; internal set; }

	/// <summary>
	/// Gets the threads parent channel.
	/// </summary>
	public DiscordChannel Parent { get; internal set; }

	/// <summary>
	/// Gets the guild in which the thread was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadCreateEventArgs"/> class.
	/// </summary>
	internal ThreadCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
