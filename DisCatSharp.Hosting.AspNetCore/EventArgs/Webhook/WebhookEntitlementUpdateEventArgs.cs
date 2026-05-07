using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementUpdated" />.
/// </summary>
public sealed class WebhookEntitlementUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}
