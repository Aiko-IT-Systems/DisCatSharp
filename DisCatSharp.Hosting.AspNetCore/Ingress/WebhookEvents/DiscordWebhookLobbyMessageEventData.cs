using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents the payload for lobby message create and update webhook events.
/// </summary>
public sealed class DiscordWebhookLobbyMessageEventData : DiscordWebhookSocialMessageEventData
{
	/// <summary>
	///     Gets the lobby-scoped metadata dictionary when present.
	/// </summary>
	[JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyDictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();
}
