using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildDeleted"/> event.
/// </summary>
public class GuildDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild that was deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets whether the guild is unavailable or not.
	/// </summary>
	public bool Unavailable { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildDeleteEventArgs"/> class.
	/// </summary>
	internal GuildDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
