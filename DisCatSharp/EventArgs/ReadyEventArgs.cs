using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.Ready"/> event.
/// </summary>
public sealed class ReadyEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ReadyEventArgs"/> class.
	/// </summary>
	internal ReadyEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
