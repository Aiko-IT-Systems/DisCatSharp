using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.InviteDeleted"/>
/// </summary>
public sealed class InviteDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild that deleted the invite.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel that the invite was for.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the deleted invite.
	/// </summary>
	public DiscordInvite Invite { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="InviteDeleteEventArgs"/> class.
	/// </summary>
	internal InviteDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
