using System;
using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildAppliedBoostsUpdated" /> event.
/// </summary>
public class GuildAppliedBoostsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildAppliedBoostsUpdateEventArgs" /> class.
	/// </summary>
	internal GuildAppliedBoostsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the user who applied the boost.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	///     Gets the boost id.
	/// </summary>
	public ulong BoostId { get; internal set; }

	/// <summary>
	///     Gets the time when the boost ends.
	/// </summary>
	public DateTimeOffset? EndsAt { get; internal set; }

	/// <summary>
	///     Gets the time when the boost pause ends.
	/// </summary>
	public DateTimeOffset? PauseEndsAt { get; internal set; }

	/// <summary>
	///     Gets whether the boost ended.
	/// </summary>
	public bool Ended { get; internal set; }
}
