using System;

using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.ApplicationAuthorized" />.
/// </summary>
public sealed class WebhookApplicationAuthorizedEventArgs : DiscordWebhookEventArgs
{
	internal WebhookApplicationAuthorizedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed authorization payload.
	/// </summary>
	public DiscordWebhookApplicationAuthorizedEventData Authorization { get; internal set; } = null!;
}
