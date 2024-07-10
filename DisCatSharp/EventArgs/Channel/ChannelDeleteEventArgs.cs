using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelDeleted"/> event.
/// </summary>
public class ChannelDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the channel that was deleted.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the guild this channel belonged to.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelDeleteEventArgs"/> class.
	/// </summary>
	internal ChannelDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
