namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

/// <summary>
///     Shapes Discord OAuth callback results into transport-neutral ingress responses.
/// </summary>
/// <remarks>
///     The default implementation emits cache-busting headers and deliberately excludes raw access and refresh tokens from the response
///     payload.
/// </remarks>
public interface IDiscordOAuthCallbackResponseFactory
{
	/// <summary>
	///     Creates the ingress response for the specified callback result.
	/// </summary>
	/// <param name="result">The callback result to shape.</param>
	/// <returns>The ingress response to emit.</returns>
	DiscordIngressResponse CreateResponse(DiscordOAuthCallbackResult result);
}
