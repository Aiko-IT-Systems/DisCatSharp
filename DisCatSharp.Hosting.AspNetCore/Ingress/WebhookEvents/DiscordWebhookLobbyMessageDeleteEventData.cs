using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents the payload for <see cref="DiscordWebhookEventNames.LobbyMessageDelete" />.
/// </summary>
public sealed class DiscordWebhookLobbyMessageDeleteEventData : ObservableApiObject
{
	/// <summary>
	///     Gets the deleted message identifier.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the lobby identifier where the message was deleted.
	/// </summary>
	[JsonProperty("lobby_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong LobbyId { get; internal set; }
}
