using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.ThreadMemberUpdated" /> event.
/// </summary>
public class ThreadMemberUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ThreadMemberUpdateEventArgs" /> class.
	/// </summary>
	internal ThreadMemberUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the thread member that was updated.
	/// </summary>
	public DiscordThreadChannelMember ThreadMember { get; internal set; }

	/// <summary>
	///     Gets the thread.
	/// </summary>
	public DiscordXThreadChannel Thread { get; internal set; }
}
