using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Identifies a user's external account associated with a Discord application.</summary>
public sealed class DiscordApplicationIdentity : ObservableApiObject
{
	/// <summary>Gets the Discord user ID returned by identity lookup endpoints.</summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)] public ulong? UserId { get; internal set; }
	/// <summary>Gets the external identity provider type.</summary>
	[JsonProperty("provider_type")] public string ProviderType { get; internal set; } = string.Empty;
	/// <summary>Gets the optional provider-specific identifier.</summary>
	[JsonProperty("provider_id", NullValueHandling = NullValueHandling.Ignore)] public string? ProviderId { get; internal set; }
	/// <summary>Gets the user ID assigned by the external provider.</summary>
	[JsonProperty("provider_issued_user_id")] public string ProviderIssuedUserId { get; internal set; } = string.Empty;
}
