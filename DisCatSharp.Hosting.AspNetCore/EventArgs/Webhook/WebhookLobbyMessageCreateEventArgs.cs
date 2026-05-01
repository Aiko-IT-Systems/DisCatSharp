using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageCreated" />.
/// </summary>
public sealed class WebhookLobbyMessageCreateEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message payload.
	/// </summary>
	public DiscordWebhookLobbyMessageEventData Message { get; internal set; } = null!;
}
