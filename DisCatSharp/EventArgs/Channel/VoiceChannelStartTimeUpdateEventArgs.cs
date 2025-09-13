using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.VoiceChannelStartTimeUpdated" /> event.
/// </summary>
public class VoiceChannelStartTimeUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceChannelStartTimeUpdateEventArgs" /> class.
	/// </summary>
	internal VoiceChannelStartTimeUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the channel in which the update occurred.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	///     Gets the new voice start time.
	/// </summary>
	public DateTimeOffset? VoiceStartTime { get; internal set; }
}
