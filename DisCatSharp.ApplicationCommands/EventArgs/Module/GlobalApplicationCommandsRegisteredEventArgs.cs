using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.GlobalApplicationCommandsRegistered"/> event.
/// </summary>
public sealed class GlobalApplicationCommandsRegisteredEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets all registered global commands.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommand> RegisteredCommands { get; internal set; } = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalApplicationCommandsRegisteredEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal GlobalApplicationCommandsRegisteredEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
