using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelCreated"/> event.
/// </summary>
public class ChannelCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the channel that was created.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the guild in which the channel was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelCreateEventArgs"/> class.
	/// </summary>
	internal ChannelCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
