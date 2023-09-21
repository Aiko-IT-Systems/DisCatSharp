using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.EntitlementCreated"/>
/// </summary>
public class EntitlementCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the entitlement that was created.
	/// </summary>
	public DiscordEntitlement Entitlement { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EntitlementCreateEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal EntitlementCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
