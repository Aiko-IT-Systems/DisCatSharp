using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildPowerupEntitlementsDeleted" /> event.
/// </summary>
public class GuildPowerupEntitlementsDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildPowerupEntitlementsDeleteEventArgs" /> class.
	/// </summary>
	internal GuildPowerupEntitlementsDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild in which the entitlements were deleted.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the deleted entitlements from the payload.
	/// </summary>
	public IReadOnlyList<DiscordEntitlement> Entitlements { get; internal set; }
}
