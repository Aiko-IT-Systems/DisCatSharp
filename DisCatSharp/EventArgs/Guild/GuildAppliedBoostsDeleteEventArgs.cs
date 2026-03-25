using System;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildAppliedBoostsDeleted" /> event.
/// </summary>
public class GuildAppliedBoostsDeleteEventArgs : GuildAppliedBoostsUpdateEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildAppliedBoostsDeleteEventArgs" /> class.
	/// </summary>
	internal GuildAppliedBoostsDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
