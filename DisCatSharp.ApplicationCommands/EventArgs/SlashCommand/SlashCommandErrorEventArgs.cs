using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="ApplicationCommandsExtension.SlashCommandErrored"/> event
/// </summary>
public class SlashCommandErrorEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The context of the command.
	/// </summary>
	public InteractionContext Context { get; internal set; }

	/// <summary>
	/// The exception thrown.
	/// </summary>
	public Exception Exception { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SlashCommandErrorEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public SlashCommandErrorEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
