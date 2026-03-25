using System;
using System.Collections.Generic;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.EventArgs;

namespace DisCatSharp.ApplicationCommands.EventArgs;

/// <summary>
///     Represents arguments for a <see cref="ApplicationCommandsExtension.ContextMenuChecksFailed" /> event.
/// </summary>
public sealed class ContextMenuChecksFailedEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ContextMenuChecksFailedEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public ContextMenuChecksFailedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     The context of the command.
	/// </summary>
	public ContextMenuContext Context { get; internal set; }

	/// <summary>
	///     The checks that failed.
	/// </summary>
	public IReadOnlyList<ApplicationCommandCheckBaseAttribute> FailedChecks { get; internal set; } = [];
}
