using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents a provisional account token issued through a bot-authenticated request.</summary>
/// <remarks>Treat both tokens as secrets and return the access token only to the authenticated game client.</remarks>
public sealed class DiscordProvisionalToken : ObservableApiObject
{
	/// <summary>Gets the provisional account access token.</summary>
	[JsonProperty("access_token")] public string AccessToken { get; internal set; } = string.Empty;
	/// <summary>Gets the identity token when Discord supplies one.</summary>
	[JsonProperty("id_token", NullValueHandling = NullValueHandling.Ignore)] public string? IdToken { get; internal set; }
	/// <summary>Gets the token type.</summary>
	[JsonProperty("token_type")] public string TokenType { get; internal set; } = string.Empty;
	/// <summary>Gets the lifetime in seconds.</summary>
	[JsonProperty("expires_in")] public int ExpiresIn { get; internal set; }
	/// <summary>Gets the granted scopes.</summary>
	[JsonProperty("scope")] public string Scope { get; internal set; } = string.Empty;
}
