using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.SubscriptionCreated" />
/// </summary>
public class SubscriptionUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="SubscriptionUpdateEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal SubscriptionUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the subscription that was updated.
	/// </summary>
	public DiscordSubscription Subscription { get; set; }
}
