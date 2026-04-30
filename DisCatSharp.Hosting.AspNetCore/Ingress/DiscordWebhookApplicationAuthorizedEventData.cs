using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents the payload for <see cref="DiscordWebhookEventNames.ApplicationAuthorized" />.
/// </summary>
public sealed class DiscordWebhookApplicationAuthorizedEventData : ObservableApiObject
{
	/// <summary>
	///     Gets the authorization installation context when Discord provided it.
	/// </summary>
	[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandIntegrationTypes? IntegrationType { get; internal set; }

	/// <summary>
	///     Gets the user that authorized the application.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? User { get; internal set; }

	/// <summary>
	///     Gets the scopes granted during authorization.
	/// </summary>
	[JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Scopes { get; internal set; } = [];

	/// <summary>
	///     Gets the guild the application was authorized for when the install target was a guild.
	/// </summary>
	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordWebhookAuthorizedGuild? Guild { get; internal set; }
}
