namespace DisCatSharp.Hosting.AspNetCore.Routing;

/// <summary>
///     Contains the stable endpoint names used by the DisCatSharp ASP.NET Core ingress surface.
/// </summary>
/// <remarks>
///     These names are intentionally stable so callers can look up mapped endpoints or attach conventions
///     without depending on the configured route segments.
/// </remarks>
public static class DiscordIngressEndpointNames
{
	/// <summary>
	///     The OAuth callback endpoint name.
	/// </summary>
	public const string OAuthCallback = "DisCatSharp.Ingress.OAuth.Callback";

	/// <summary>
	///     The interactions endpoint name.
	/// </summary>
	public const string Interactions = "DisCatSharp.Ingress.Interactions";

	/// <summary>
	///     The webhook events endpoint name.
	/// </summary>
	public const string WebhookEvents = "DisCatSharp.Ingress.Webhooks.Events";

	/// <summary>
	///     The incoming webhooks endpoint name.
	/// </summary>
	public const string IncomingWebhooks = "DisCatSharp.Ingress.Webhooks.Incoming";
}
