using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.EntitlementDeleted" />.
/// </summary>
public sealed class WebhookEntitlementDeleteEventArgs : DiscordWebhookEventArgs
{
	internal WebhookEntitlementDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the entitlement payload.
	/// </summary>
	public DiscordEntitlement Entitlement { get; internal set; } = null!;
}
