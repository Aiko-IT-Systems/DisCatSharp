using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildPowerupEntitlementsCreated" /> event.
/// </summary>
public class GuildPowerupEntitlementsCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildPowerupEntitlementsCreateEventArgs" /> class.
	/// </summary>
	internal GuildPowerupEntitlementsCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild in which the entitlements were created.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the created entitlements from the payload.
	/// </summary>
	public IReadOnlyList<DiscordEntitlement> Entitlements { get; internal set; }
}
