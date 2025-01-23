using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildJoinRequestDeleted" /> event.
/// </summary>
public class GuildJoinRequestDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildJoinRequestDeleteEventArgs" /> class.
	/// </summary>
	internal GuildJoinRequestDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the request ID of the join request that was deleted.
	/// </summary>
	public ulong RequestId { get; internal set; }

	/// <summary>
	///     Gets the user that deleted the join request.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the guild this join request was deleted from.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }
}
