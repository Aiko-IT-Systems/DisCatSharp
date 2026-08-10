using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for application command permissions update events.
/// </summary>
public sealed class ApplicationCommandPermissionsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ApplicationCommandPermissionsUpdateEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ApplicationCommandPermissionsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the application command permissions.
	/// </summary>
	public List<DiscordApplicationCommandPermission> Permissions { get; internal set; }

	/// <summary>
	///     Gets the identifier of the command or application whose permissions were updated.
	/// </summary>
	/// <remarks>
	///     Discord uses the application id for application-wide permissions.
	/// </remarks>
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the application id.
	/// </summary>
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	///     Gets whether this update contains application-wide permissions rather than command-specific permissions.
	/// </summary>
	public bool IsApplicationWide
		=> this.Id == this.ApplicationId;

	/// <summary>
	///     Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
