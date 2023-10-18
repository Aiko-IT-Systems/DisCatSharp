using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.EntitlementDeleted"/>
/// </summary>
public class EntitlementDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the entitlement that was deleted.
	/// </summary>
	public DiscordEntitlement Entitlement { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EntitlementDeleteEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal EntitlementDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
