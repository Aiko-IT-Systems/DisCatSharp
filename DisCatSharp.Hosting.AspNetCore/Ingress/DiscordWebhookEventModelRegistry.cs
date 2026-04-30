using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Resolves the typed payload model associated with a documented Discord webhook event type.
/// </summary>
public static class DiscordWebhookEventModelRegistry
{
	/// <summary>
	///     Gets the recommended payload model type for a webhook event.
	/// </summary>
	/// <param name="eventType">The Discord webhook event type name.</param>
	/// <returns>The payload model type when one is known; otherwise <see langword="null" />.</returns>
	public static Type? GetPayloadType(string? eventType)
		=> eventType switch
		{
			DiscordWebhookEventNames.ApplicationAuthorized => typeof(DiscordWebhookApplicationAuthorizedEventData),
			DiscordWebhookEventNames.ApplicationDeauthorized => typeof(DiscordWebhookApplicationDeauthorizedEventData),
			DiscordWebhookEventNames.EntitlementCreate => typeof(DiscordEntitlement),
			DiscordWebhookEventNames.EntitlementUpdate => typeof(DiscordEntitlement),
			DiscordWebhookEventNames.EntitlementDelete => typeof(DiscordEntitlement),
			DiscordWebhookEventNames.LobbyMessageCreate => typeof(DiscordWebhookLobbyMessageEventData),
			DiscordWebhookEventNames.LobbyMessageUpdate => typeof(DiscordWebhookLobbyMessageEventData),
			DiscordWebhookEventNames.LobbyMessageDelete => typeof(DiscordWebhookLobbyMessageDeleteEventData),
			DiscordWebhookEventNames.GameDirectMessageCreate => typeof(DiscordWebhookGameDirectMessageEventData),
			DiscordWebhookEventNames.GameDirectMessageUpdate => typeof(DiscordWebhookGameDirectMessageEventData),
			DiscordWebhookEventNames.GameDirectMessageDelete => typeof(DiscordWebhookGameDirectMessageEventData),
			_ => null
		};
}
