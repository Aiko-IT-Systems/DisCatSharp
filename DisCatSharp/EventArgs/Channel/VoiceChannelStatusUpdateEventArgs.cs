using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceChannelStatusUpdated"/> event.
/// </summary>
public class VoiceChannelStatusUpdateEventArgs : DiscordEventArgs
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
	/// Gets the new status.
	/// </summary>
	public string? Status { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceChannelStatusUpdateEventArgs"/> class.
	/// </summary>
	internal VoiceChannelStatusUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
