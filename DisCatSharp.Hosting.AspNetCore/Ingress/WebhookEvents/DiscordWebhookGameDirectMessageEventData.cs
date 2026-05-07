using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents the payload for Social SDK game direct message webhook events.
/// </summary>
public sealed class DiscordWebhookGameDirectMessageEventData : DiscordWebhookSocialMessageEventData
{
	/// <summary>
	///     Gets the other direct message participant identifier when Discord provided it.
	/// </summary>
	[JsonProperty("recipient_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? RecipientId { get; internal set; }
}
