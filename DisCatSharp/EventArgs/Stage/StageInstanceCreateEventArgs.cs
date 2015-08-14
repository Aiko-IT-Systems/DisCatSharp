using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.StageInstanceCreated"/> event.
/// </summary>
public class StageInstanceCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the stage instance that was created.
	/// </summary>
	public DiscordStageInstance StageInstance { get; internal set; }

	/// <summary>
	/// Gets the guild in which the stage instance was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StageInstanceCreateEventArgs"/> class.
	/// </summary>
	internal StageInstanceCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
