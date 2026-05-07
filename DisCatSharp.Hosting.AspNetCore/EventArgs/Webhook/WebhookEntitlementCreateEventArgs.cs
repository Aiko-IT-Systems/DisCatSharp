using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementCreated" />.
/// </summary>
public sealed class WebhookEntitlementCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}
