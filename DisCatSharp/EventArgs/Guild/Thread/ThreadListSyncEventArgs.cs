using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadListSynced"/> event.
/// </summary>
public class ThreadListSyncEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets all thread member objects from the synced threads for the current user, indicating which threads the current user has been added to
	/// </summary>
	public IReadOnlyList<DiscordThreadChannelMember> Members { get; internal set; }

	/// <summary>
	/// Gets all active threads in the given channels that the current user can access.
	/// </summary>
	public IReadOnlyList<DiscordThreadChannel> Threads { get; internal set; }

	/// <summary>
	/// Gets the parent channels whose threads are being synced. If empty, then threads are synced for the guild. May contain channels that have no active threads as well.
	/// </summary>
	public IReadOnlyList<DiscordChannel> Channels { get; internal set; }

	/// <summary>
	/// Gets the guild being synced.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadListSyncEventArgs"/> class.
	/// </summary>
	internal ThreadListSyncEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
