using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents the arguments for a <see cref="ApplicationCommandsExtension.SlashCommandExecuted"/> event
/// </summary>
public class SlashCommandExecutedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The context of the command.
	/// </summary>
	public InteractionContext Context { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SlashCommandExecutedEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public SlashCommandExecutedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
