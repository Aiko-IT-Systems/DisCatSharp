using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.PresenceUpdated"/> event.
/// </summary>
public class PresenceUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the user whose presence was updated.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the user's new game.
	/// </summary>
	public DiscordActivity Activity { get; internal set; }

	/// <summary>
	/// Gets the user's status.
	/// </summary>
	public UserStatus Status { get; internal set; }

	/// <summary>
	/// Gets the user's old presence.
	/// </summary>
	public DiscordPresence PresenceBefore { get; internal set; }

	/// <summary>
	/// Gets the user's new presence.
	/// </summary>
	public DiscordPresence PresenceAfter { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PresenceUpdateEventArgs"/> class.
	/// </summary>
	internal PresenceUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
