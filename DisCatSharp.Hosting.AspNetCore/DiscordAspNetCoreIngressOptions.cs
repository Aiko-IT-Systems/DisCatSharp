namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Configures the public ASP.NET Core route surface for DisCatSharp web ingress.
/// </summary>
public sealed class DiscordAspNetCoreIngressOptions
{
	/// <summary>
	///     The default route prefix used for DisCatSharp ingress endpoints.
	/// </summary>
	public const string DefaultRoutePrefix = "/discord";

	/// <summary>
	///     The default route segment for OAuth-related ingress endpoints.
	/// </summary>
	public const string DefaultOAuthPath = "oauth";

	/// <summary>
	///     The default route segment for the OAuth callback endpoint.
	/// </summary>
	public const string DefaultOAuthCallbackPath = "callback";

	/// <summary>
	///     The default route segment for the interactions endpoint.
	/// </summary>
	public const string DefaultInteractionsPath = "interactions";

	/// <summary>
	///     The default route segment for webhook-related ingress endpoints.
	/// </summary>
	public const string DefaultWebhooksPath = "webhooks";

	/// <summary>
	///     The default route segment for webhook event endpoints.
	/// </summary>
	public const string DefaultWebhookEventsPath = "events";

	/// <summary>
	///     The default route segment for incoming webhook endpoints.
	/// </summary>
	public const string DefaultIncomingWebhooksPath = "incoming";

	/// <summary>
	///     Gets or sets the root route prefix used by <see cref="EndpointRouteBuilderExtensions.MapDisCatSharpIngress(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)" />.
	/// </summary>
	public string RoutePrefix { get; set; } = DefaultRoutePrefix;

	/// <summary>
	///     Gets or sets the route segment used for OAuth-related endpoints.
	/// </summary>
	public string OAuthPath { get; set; } = DefaultOAuthPath;

	/// <summary>
	///     Gets or sets the route segment used for the OAuth callback endpoint.
	/// </summary>
	public string OAuthCallbackPath { get; set; } = DefaultOAuthCallbackPath;

	/// <summary>
	///     Gets or sets the route segment used for the Discord interactions endpoint.
	/// </summary>
	public string InteractionsPath { get; set; } = DefaultInteractionsPath;

	/// <summary>
	///     Gets or sets the route segment used for webhook-related endpoints.
	/// </summary>
	public string WebhooksPath { get; set; } = DefaultWebhooksPath;

	/// <summary>
	///     Gets or sets the route segment used for webhook event endpoints.
	/// </summary>
	public string WebhookEventsPath { get; set; } = DefaultWebhookEventsPath;

	/// <summary>
	///     Gets or sets the route segment used for incoming webhook endpoints.
	/// </summary>
	public string IncomingWebhooksPath { get; set; } = DefaultIncomingWebhooksPath;
}
