using System;

using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.GameDirectMessageCreated" />.
/// </summary>
public sealed class WebhookGameDirectMessageCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookGameDirectMessageCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed game direct message payload.
	/// </summary>
	public DiscordWebhookGameDirectMessageEventData Message { get; internal set; } = null!;
}
