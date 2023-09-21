using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
/// Represents a <see cref="DiscordAccessToken"/>.
/// </summary>
public sealed class DiscordAccessToken : ObservableApiObject
{
	/// <summary>
	/// Gets the access token.
	/// </summary>
	[JsonProperty("access_token")]
	public string AccessToken { get; internal set; }

	/// <summary>
	/// Gets the token type.
	/// </summary>
	[JsonProperty("token_type")]
	public string TokenType { get; internal set; }

	/// <summary>
	/// Gets when the token expires.
	/// </summary>
	[JsonProperty("expires_in")]
	public int ExpiresIn { get; internal set; }

	/// <summary>
	/// Gets the refresh token.
	/// </summary>
	[JsonProperty("refresh_token")]
	public string RefreshToken { get; internal set; }

	/// <summary>
	/// Gets the scope.
	/// </summary>
	[JsonProperty("scope")]
	public string Scope { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="DiscordAccessToken"/> for usage with <see cref="DiscordOAuth2Client"/>.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	/// <param name="tokenType">The token type.</param>
	/// <param name="expiresIn">When the token expires.</param>
	/// <param name="refreshToken">The refresh token.</param>
	/// <param name="scope">The scope(s).</param>
	public DiscordAccessToken(string accessToken, string tokenType, int expiresIn, string refreshToken, string scope)
	{
		this.AccessToken = accessToken;
		this.TokenType = tokenType;
		this.ExpiresIn = expiresIn;
		this.RefreshToken = refreshToken;
		this.Scope = scope;
	}

	/// <summary>
	/// Constructs a new <see cref="DiscordAccessToken"/>.
	/// </summary>
	internal DiscordAccessToken()
	{ }

	/// <summary>
	/// Generates a new <see cref="DiscordAccessToken"/> from json.
	/// </summary>
	/// <param name="json">The json.</param>
	public static DiscordAccessToken FromJson(string json)
		=> DiscordJson.DeserializeObject<DiscordAccessToken>(json, null);
}
