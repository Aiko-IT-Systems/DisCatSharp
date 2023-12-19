using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildRoleUpdated"/> event.
/// </summary>
public class GuildRoleUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the post-update role.
	/// </summary>
	public DiscordRole RoleAfter { get; internal set; }

	/// <summary>
	/// Gets the pre-update role.
	/// </summary>
	public DiscordRole RoleBefore { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildRoleUpdateEventArgs"/> class.
	/// </summary>
	internal GuildRoleUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
