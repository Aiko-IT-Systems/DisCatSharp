using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.SubscriptionCreated" />
/// </summary>
public class SubscriptionCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="SubscriptionCreateEventArgs" /> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	internal SubscriptionCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the subscription that was created.
	/// </summary>
	public DiscordSubscription Subscription { get; set; }
}
