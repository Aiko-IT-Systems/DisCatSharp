using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ClientErrored"/> event.
/// </summary>
public class ClientErrorEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the exception thrown by the client.
	/// </summary>
	public Exception Exception { get; internal set; }

	/// <summary>
	/// Gets the name of the event that threw the exception.
	/// </summary>
	public string EventName { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ClientErrorEventArgs"/> class.
	/// </summary>
	internal ClientErrorEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
