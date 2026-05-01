using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;

namespace DisCatSharp.Hosting.AspNetCore.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordWebhookEventDispatcher.LobbyMessageDeleted" />.
/// </summary>
public sealed class WebhookLobbyMessageDeleteEventArgs : DiscordWebhookEventArgs
{
	internal WebhookLobbyMessageDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the typed lobby message delete payload.
	/// </summary>
	public DiscordWebhookLobbyMessageDeleteEventData Message { get; internal set; } = null!;
}
