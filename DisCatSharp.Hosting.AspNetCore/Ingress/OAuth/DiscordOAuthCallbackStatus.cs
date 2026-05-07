namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

/// <summary>
///     Describes the outcome of processing a Discord OAuth callback.
/// </summary>
public enum DiscordOAuthCallbackStatus
{
	/// <summary>
	///     The callback completed successfully and the authorization code was exchanged.
	/// </summary>
	Success = 0,

	/// <summary>
	///     The callback request was malformed or Discord returned an OAuth error response.
	/// </summary>
	InvalidRequest = 1,

	/// <summary>
	///     The callback state could not be matched to a pending authorization request.
	/// </summary>
	InvalidState = 2,

	/// <summary>
	///     The callback failed an additional security validation step.
	/// </summary>
	/// <remarks>
	///     Examples include mismatched flow names, mismatched stored state, or a redirect URI that no longer matches the configured
	///     callback URI.
	/// </remarks>
	SecurityFailure = 3,

	/// <summary>
	///     The callback could not be processed because the OAuth ingress flow is not configured.
	/// </summary>
	ConfigurationFailure = 4,

	/// <summary>
	///     The authorization code exchange failed after the callback had been validated.
	/// </summary>
	ExchangeFailure = 5
}
