using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceUpdated"/> event.
/// </summary>
public class StageInstanceUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the stage instance that was updated.
	/// </summary>
	public DiscordStageInstance StageInstance { get; internal set; }

	/// <summary>
	/// Gets the guild in which the stage instance was updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StageInstanceUpdateEventArgs"/> class.
	/// </summary>
	internal StageInstanceUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
