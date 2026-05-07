using System;

using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.ApplicationDeauthorized" />.
/// </summary>
public sealed class WebhookApplicationDeauthorizedEventArgs : DiscordWebhookEventArgs
{
	internal WebhookApplicationDeauthorizedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed deauthorization payload.
	/// </summary>
	public DiscordWebhookApplicationDeauthorizedEventData Deauthorization { get; internal set; } = null!;
}
