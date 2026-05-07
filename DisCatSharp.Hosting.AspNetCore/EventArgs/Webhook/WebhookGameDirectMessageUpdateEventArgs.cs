using System;

using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.GameDirectMessageUpdated" />.
/// </summary>
public sealed class WebhookGameDirectMessageUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookGameDirectMessageUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed game direct message payload.
	/// </summary>
	public DiscordWebhookGameDirectMessageEventData Message { get; internal set; } = null!;
}
