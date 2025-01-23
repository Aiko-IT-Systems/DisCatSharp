using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildJoinRequestCreated" /> event.
/// </summary>
public class GuildJoinRequestCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildJoinRequestCreateEventArgs" /> class.
	/// </summary>
	internal GuildJoinRequestCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the request of the join request that was created.
	/// </summary>
	public DiscordGuildJoinRequest Request { get; internal set; }

	/// <summary>
	///     Gets the status of the join request.
	/// </summary>
	public JoinRequestStatusType Status { get; internal set; }

	/// <summary>
	///     Gets the user that created the join request.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the guild this join request was created for.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
