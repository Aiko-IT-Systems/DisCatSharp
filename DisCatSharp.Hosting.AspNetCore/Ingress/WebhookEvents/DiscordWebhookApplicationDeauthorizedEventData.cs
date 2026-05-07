using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents the payload for <see cref="DiscordWebhookEventNames.ApplicationDeauthorized" />.
/// </summary>
public sealed class DiscordWebhookApplicationDeauthorizedEventData : ObservableApiObject
{
	/// <summary>
	///     Gets the user that deauthorized the application.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? User { get; internal set; }
}
