using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.ApplicationCommandsModuleStartupFinished"/> event.
/// </summary>
public class ApplicationCommandsModuleStartupFinishedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets a list of all guild ids missing the application commands scope.
	/// </summary>
	public IReadOnlyList<ulong> GuildsWithoutScope { get; internal set; }

	/// <summary>
	/// Gets all registered global commands.
	/// </summary>
	public IReadOnlyList<DiscordApplicationCommand> RegisteredGlobalCommands { get; internal set; }

	/// <summary>
	/// Gets all registered guild commands mapped by guild id.
	/// </summary>
	public IReadOnlyDictionary<ulong, IReadOnlyList<DiscordApplicationCommand>> RegisteredGuildCommands { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandsModuleStartupFinishedEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal ApplicationCommandsModuleStartupFinishedEventArgs(IServiceProvider? provider)
		: base(provider)
	{ }
}
