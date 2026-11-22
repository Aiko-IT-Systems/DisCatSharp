using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>Connects Social SDK webhook messages to bot-authenticated moderation operations.</summary>
public static class DiscordWebhookSocialMessageExtensions
{
	/// <summary>Updates moderation metadata for a lobby message received by webhook.</summary>
	/// <param name="message">The webhook message.</param><param name="client">The bot-authenticated lobby client.</param>
	/// <param name="metadata">The moderation metadata.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the request.</returns>
	/// <exception cref="InvalidOperationException">The payload lacks the lobby or message ID.</exception>
	public static Task UpdateModerationMetadataAsync(this DiscordWebhookLobbyMessageEventData message, DiscordSocialSdkClient client, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(client);
		if (message.LobbyId is not > 0 || message.Id == 0)
			throw new InvalidOperationException("The webhook message must include a lobby ID and message ID.");
		return client.UpdateLobbyMessageModerationAsync(message.LobbyId.Value, message.Id, metadata, cancellationToken);
	}

	/// <summary>Updates moderation metadata for a game direct message received by webhook.</summary>
	/// <param name="message">The webhook message.</param><param name="client">The bot-authenticated Social SDK client.</param>
	/// <param name="metadata">The moderation metadata.</param><param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the request.</returns>
	/// <exception cref="InvalidOperationException">The payload lacks a message ID, author ID, or recipient ID.</exception>
	public static Task UpdateModerationMetadataAsync(this DiscordWebhookGameDirectMessageEventData message, DiscordSocialSdkClient client, IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(client);
		if (message.Author is null || message.Author.Id == 0 || message.RecipientId is not > 0 || message.Id == 0)
			throw new InvalidOperationException("The webhook message must include a message ID, author ID, and recipient ID.");
		return client.UpdateDirectMessageModerationAsync(message.Author.Id, message.RecipientId.Value, message.Id, metadata, cancellationToken);
	}
}
