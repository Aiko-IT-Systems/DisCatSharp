using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.ThreadUpdated" /> event.
/// </summary>
public class ThreadUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ThreadUpdateEventArgs" /> class.
	/// </summary>
	internal ThreadUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the post-update thread.
	/// </summary>
	public DiscordXThreadChannel ThreadAfter { get; internal set; }

	/// <summary>
	///     Gets the pre-update thread.
	/// </summary>
	public DiscordXThreadChannel ThreadBefore { get; internal set; }

	/// <summary>
	///     Gets the threads parent channel.
	/// </summary>
	public DiscordChannel Parent { get; internal set; }

	/// <summary>
	///     Gets the guild in which the thread was updated.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
