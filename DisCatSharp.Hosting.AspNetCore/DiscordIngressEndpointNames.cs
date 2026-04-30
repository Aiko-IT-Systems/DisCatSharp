namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Contains the stable endpoint names used by the DisCatSharp ASP.NET Core ingress surface.
/// </summary>
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
