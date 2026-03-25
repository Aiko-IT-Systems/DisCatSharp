using System;
using System.Collections.Generic;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
///     Represents arguments for a <see cref="ApplicationCommandsExtension.SlashCommandChecksFailed" /> event.
/// </summary>
public class SlashCommandChecksFailedEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="SlashCommandChecksFailedEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public SlashCommandChecksFailedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     The context of the command.
	/// </summary>
	public InteractionContext Context { get; internal set; }

	/// <summary>
	///     The checks that failed.
	/// </summary>
	public IReadOnlyList<ApplicationCommandCheckBaseAttribute> FailedChecks { get; internal set; } = [];
}
