using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberRemoved"/> event.
/// </summary>
public class GuildMemberRemoveEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild the member was removed from.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the member that was removed.
	/// </summary>
	public DiscordMember Member { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildMemberRemoveEventArgs"/> class.
	/// </summary>
	internal GuildMemberRemoveEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
