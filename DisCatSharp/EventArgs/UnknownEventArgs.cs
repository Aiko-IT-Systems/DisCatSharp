using System;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.UnknownEvent"/> event.
/// </summary>
public class UnknownEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the event's name.
	/// </summary>
	public string EventName { get; internal set; }

	/// <summary>
	/// Gets the event's data.
	/// </summary>
	public string Json { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="UnknownEventArgs"/> class.
	/// </summary>
	internal UnknownEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
