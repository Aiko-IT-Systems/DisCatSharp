using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;

/// <summary>
///     Represents the guild context included with application authorization webhook events.
/// </summary>
public sealed class DiscordWebhookAuthorizedGuild : ObservableApiObject
{
	/// <summary>
	///     Gets the guild identifier.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; internal set; }

	/// <summary>
	///     Gets the guild name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	/// <summary>
	///     Gets the guild icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? IconHash { get; internal set; }
}
