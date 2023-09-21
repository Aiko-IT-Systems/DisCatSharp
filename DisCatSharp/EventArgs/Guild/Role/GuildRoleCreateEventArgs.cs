using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildRoleCreated"/> event.
/// </summary>
public class GuildRoleCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild in which the role was created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the role that was created.
	/// </summary>
	public DiscordRole Role { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildRoleCreateEventArgs"/> class.
	/// </summary>
	internal GuildRoleCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
