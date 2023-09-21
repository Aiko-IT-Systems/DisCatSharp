using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for application command permissions update events.
/// </summary>
public sealed class ApplicationCommandPermissionsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the application command permissions.
	/// </summary>
	public List<DiscordApplicationCommandPermission> Permissions { get; internal set; }

	/// <summary>
	/// Gets the application command.
	/// </summary>
	public DiscordApplicationCommand Command { get; internal set; }

	/// <summary>
	/// Gets the application id.
	/// </summary>
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandPermissionsUpdateEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ApplicationCommandPermissionsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
