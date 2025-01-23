using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildJoinRequestUpdated" /> event.
/// </summary>
public class GuildJoinRequestUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildJoinRequestUpdateEventArgs" /> class.
	/// </summary>
	internal GuildJoinRequestUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the request of the join request that was updated.
	/// </summary>
	public DiscordGuildJoinRequest Request { get; internal set; }

	/// <summary>
	///     Gets the status of the join request.
	/// </summary>
	public JoinRequestStatusType Status { get; internal set; }

	/// <summary>
	///     Gets the user who's join request was updated.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the guild this join request was updated for.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
