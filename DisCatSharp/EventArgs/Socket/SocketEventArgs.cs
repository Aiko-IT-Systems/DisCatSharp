using System;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents basic socket event arguments.
/// </summary>
public class SocketEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Creates a new event argument container.
	/// </summary>
	public SocketEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
