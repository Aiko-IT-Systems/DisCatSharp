using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.EntitlementUpdated"/>
/// </summary>
public class EntitlementUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the entitlement that was updated.
	/// </summary>
	public DiscordEntitlement Entitlement { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EntitlementUpdateEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal EntitlementUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
