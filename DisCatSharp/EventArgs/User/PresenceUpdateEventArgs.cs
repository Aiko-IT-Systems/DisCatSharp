using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.PresenceUpdated" /> event.
/// </summary>
public class PresenceUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="PresenceUpdateEventArgs" /> class.
	/// </summary>
	internal PresenceUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the user whose presence was updated.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the user's new game.
	/// </summary>
	public DiscordActivity? Activity { get; internal set; }

	/// <summary>
	///     Gets the user's new activitites.
	/// </summary>
	public IReadOnlyList<DiscordActivity>? Activities { get; internal set; }

	/// <summary>
	///     Gets the user's status.
	/// </summary>
	public UserStatus Status { get; internal set; }

	/// <summary>
	///     Gets the user's old presence.
	/// </summary>
	public DiscordPresence? PresenceBefore { get; internal set; }

	/// <summary>
	///     Gets the user's new presence.
	/// </summary>
	public DiscordPresence PresenceAfter { get; internal set; }
}
