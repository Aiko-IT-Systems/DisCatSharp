using System;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildAppliedBoostsCreated" /> event.
/// </summary>
public class GuildAppliedBoostsCreateEventArgs : GuildAppliedBoostsUpdateEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildAppliedBoostsCreateEventArgs" /> class.
	/// </summary>
	internal GuildAppliedBoostsCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
