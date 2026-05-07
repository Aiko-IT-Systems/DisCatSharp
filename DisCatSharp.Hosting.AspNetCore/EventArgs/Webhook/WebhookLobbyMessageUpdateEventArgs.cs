using System;

using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageUpdated" />.
/// </summary>
public sealed class WebhookLobbyMessageUpdateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message payload.
	/// </summary>
	public DiscordWebhookLobbyMessageEventData Message { get; internal set; } = null!;
}
