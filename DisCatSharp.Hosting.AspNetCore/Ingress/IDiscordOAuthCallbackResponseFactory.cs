namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Shapes Discord OAuth callback results into transport-neutral ingress responses.
/// </summary>
public interface IDiscordOAuthCallbackResponseFactory
{
	/// <summary>
	///     Creates the ingress response for the specified callback result.
	/// </summary>
	/// <param name="result">The callback result to shape.</param>
	/// <returns>The ingress response to emit.</returns>
	DiscordIngressResponse CreateResponse(DiscordOAuthCallbackResult result);
}
