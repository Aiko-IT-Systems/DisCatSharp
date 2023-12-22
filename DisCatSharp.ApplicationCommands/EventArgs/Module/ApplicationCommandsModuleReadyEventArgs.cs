using System;
using System.Collections.Generic;

using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.ApplicationCommandsModuleReady"/> event.
/// </summary>
public sealed class ApplicationCommandsModuleReadyEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets a list of all guild ids missing the application commands scope.
	/// </summary>
	public IReadOnlyList<ulong> GuildsWithoutScope { get; internal set; } = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="ApplicationCommandsModuleReadyEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal ApplicationCommandsModuleReadyEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
