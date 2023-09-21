using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.UserUpdated"/> event.
/// </summary>
public class UserUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the post-update user.
	/// </summary>
	public DiscordUser UserAfter { get; internal set; }

	/// <summary>
	/// Gets the pre-update user.
	/// </summary>
	public DiscordUser UserBefore { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="UserUpdateEventArgs"/> class.
	/// </summary>
	internal UserUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
