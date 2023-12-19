using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ChannelPinsUpdated"/> event.
/// </summary>
public class ChannelPinsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel in which the update occurred.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the timestamp of the latest pin.
	/// </summary>
	public DateTimeOffset? LastPinTimestamp { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelPinsUpdateEventArgs"/> class.
	/// </summary>
	internal ChannelPinsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
