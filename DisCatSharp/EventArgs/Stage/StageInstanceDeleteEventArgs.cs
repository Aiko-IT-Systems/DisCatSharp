using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceDeleted"/> event.
/// </summary>
public class StageInstanceDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the stage instance that was deleted.
	/// </summary>
	public DiscordStageInstance StageInstance { get; internal set; }

	/// <summary>
	/// Gets the guild in which the stage instance was deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StageInstanceDeleteEventArgs"/> class.
	/// </summary>
	internal StageInstanceDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
