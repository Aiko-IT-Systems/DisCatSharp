namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a OAuth2 payload.
/// </summary>
internal interface IOAuth2Payload
{
	/// <summary>
	/// Gets or sets the access token.
	/// </summary>
	string AccessToken { get; set; }
}
