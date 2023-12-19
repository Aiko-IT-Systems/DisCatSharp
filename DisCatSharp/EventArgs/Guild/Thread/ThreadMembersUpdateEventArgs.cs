using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadMembersUpdated"/> event.
/// </summary>
public class ThreadMembersUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the approximate number of members in the thread, capped at 50.
	/// </summary>
	public int MemberCount { get; internal set; }

	/// <summary>
	/// Gets the users who were removed from the thread.
	/// </summary>
	public IReadOnlyList<DiscordMember> RemovedMembers { get; internal set; }

	/// <summary>
	/// Gets the users who were added to the thread.
	/// </summary>
	public IReadOnlyList<DiscordThreadChannelMember> AddedMembers { get; internal set; }

	/// <summary>
	/// Gets the id of the thread.
	/// </summary>
	public ulong ThreadId { get; internal set; }

	/// <summary>
	/// Gets the id of the thread.
	/// </summary>
	public DiscordThreadChannel Thread { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadMembersUpdateEventArgs"/> class.
	/// </summary>
	internal ThreadMembersUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
