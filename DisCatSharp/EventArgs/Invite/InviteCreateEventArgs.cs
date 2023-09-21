using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.InviteCreated"/>
/// </summary>
public sealed class InviteCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the guild that created the invite.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel that the invite is for.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the created invite.
	/// </summary>
	public DiscordInvite Invite { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="InviteCreateEventArgs"/> class.
	/// </summary>
	internal InviteCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
