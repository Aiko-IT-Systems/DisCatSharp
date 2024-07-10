using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.GuildApplicationCommandsRegistered"/> event.
/// </summary>
public sealed class GuildApplicationCommandsRegisteredEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the target guild id.
	/// </summary>
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets all registered guild commands.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommand> RegisteredCommands { get; internal set; } = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildApplicationCommandsRegisteredEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal GuildApplicationCommandsRegisteredEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
