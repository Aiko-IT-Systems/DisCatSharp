using System;

using DisCatSharp.Common.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.EventArgs;

// Note: this might seem useless, but should we ever need to add a common property or method to all event arg
// classes, it would be useful to already have a base for all of it.

/// <summary>
/// Common base for all other <see cref="DiscordClient"/>-related event argument classes.
/// </summary>
public abstract class DiscordEventArgs : AsyncEventArgs
{
	/// <summary>
	/// <para>Gets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEventArgs"/> class.
	/// </summary>
	protected DiscordEventArgs(IServiceProvider provider)
	{
		if (provider != null)
			this.ServiceProvider = provider.CreateScope().ServiceProvider;
	}
}
