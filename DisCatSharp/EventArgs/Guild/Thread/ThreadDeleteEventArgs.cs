using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadDeleted"/> event.
/// </summary>
public class ThreadDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the thread that was deleted.
	/// </summary>
	public DiscordThreadChannel Thread { get; internal set; }

	/// <summary>
	/// Gets the threads parent channel.
	/// </summary>
	public DiscordChannel Parent { get; internal set; }

	/// <summary>
	/// Gets the guild this thread belonged to.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the threads type.
	/// </summary>
	public ChannelType Type { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadDeleteEventArgs"/> class.
	/// </summary>
	internal ThreadDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
