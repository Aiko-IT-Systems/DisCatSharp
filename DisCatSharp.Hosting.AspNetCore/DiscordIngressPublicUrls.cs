using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Represents the externally visible ASP.NET Core ingress URLs exposed by DisCatSharp.
/// </summary>
public sealed class DiscordIngressPublicUrls
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIngressPublicUrls" /> class.
	/// </summary>
	/// <param name="baseUrl">The base URL used to build the ingress surface.</param>
	/// <param name="ingressRootPath">The normalized ingress root path.</param>
	/// <param name="ingressRootUrl">The absolute ingress root URL.</param>
	/// <param name="oauthCallbackUrl">The OAuth callback URL.</param>
	/// <param name="interactionsUrl">The interactions URL.</param>
	/// <param name="webhookEventsUrl">The webhook events URL.</param>
	/// <param name="incomingWebhooksUrl">The incoming webhooks URL.</param>
	/// <param name="roleConnectionsVerificationUrl">The linked-roles verification URL when configured.</param>
	private DiscordIngressPublicUrls(
		Uri baseUrl,
		string ingressRootPath,
		Uri ingressRootUrl,
		Uri oauthCallbackUrl,
		Uri interactionsUrl,
		Uri webhookEventsUrl,
		Uri incomingWebhooksUrl,
		Uri? roleConnectionsVerificationUrl
	)
	{
		this.BaseUrl = baseUrl;
		this.IngressRootPath = ingressRootPath;
		this.IngressRootUrl = ingressRootUrl;
		this.OAuthCallbackUrl = oauthCallbackUrl;
		this.InteractionsUrl = interactionsUrl;
		this.WebhookEventsUrl = webhookEventsUrl;
		this.IncomingWebhooksUrl = incomingWebhooksUrl;
		this.RoleConnectionsVerificationUrl = roleConnectionsVerificationUrl;
	}

	/// <summary>
	///     Gets the base URL used to build the ingress surface.
	/// </summary>
	public Uri BaseUrl { get; }

	/// <summary>
	///     Gets the normalized public ingress root path.
	/// </summary>
	public string IngressRootPath { get; }

	/// <summary>
	///     Gets the public ingress root URL.
	/// </summary>
	public Uri IngressRootUrl { get; }

	/// <summary>
	///     Gets the public OAuth callback URL.
	/// </summary>
	public Uri OAuthCallbackUrl { get; }

	/// <summary>
	///     Gets the public interactions URL.
	/// </summary>
	public Uri InteractionsUrl { get; }

	/// <summary>
	///     Gets the public webhook events URL.
	/// </summary>
	public Uri WebhookEventsUrl { get; }

	/// <summary>
	///     Gets the public incoming webhooks URL.
	/// </summary>
	public Uri IncomingWebhooksUrl { get; }

	/// <summary>
	///     Gets the public linked-roles verification URL when linked-roles support is configured.
	/// </summary>
	public Uri? RoleConnectionsVerificationUrl { get; }

	/// <summary>
	///     Creates a public URL snapshot from the supplied base URL and ingress options.
	/// </summary>
	/// <param name="baseUrl">The externally visible base URL for the ASP.NET Core app.</param>
	/// <param name="options">The ingress route options. Defaults to the package defaults when omitted.</param>
	/// <param name="linkedRolesOptions">The optional linked-roles route options.</param>
	/// <returns>The resolved public ingress URLs.</returns>
	public static DiscordIngressPublicUrls Create(Uri baseUrl, DiscordAspNetCoreIngressOptions? options = null, DiscordLinkedRolesOptions? linkedRolesOptions = null)
	{
		ArgumentNullException.ThrowIfNull(baseUrl);

		if (!baseUrl.IsAbsoluteUri)
			throw new ArgumentException("The base URL must be absolute.", nameof(baseUrl));
		if (!string.IsNullOrEmpty(baseUrl.Query) || !string.IsNullOrEmpty(baseUrl.Fragment))
			throw new ArgumentException("The base URL cannot contain query string or fragment components.", nameof(baseUrl));

		options ??= new();

		var routePrefix = NormalizeRoutePrefix(options.RoutePrefix);
		var oauthPath = NormalizeRouteSegment(options.OAuthPath);
		var oauthCallbackPath = NormalizeRouteSegment(options.OAuthCallbackPath);
		var interactionsPath = NormalizeRouteSegment(options.InteractionsPath);
		var webhooksPath = NormalizeRouteSegment(options.WebhooksPath);
		var webhookEventsPath = NormalizeRouteSegment(options.WebhookEventsPath);
		var incomingWebhooksPath = NormalizeRouteSegment(options.IncomingWebhooksPath);

		var ingressRootPath = BuildPath(baseUrl.AbsolutePath, routePrefix);
		var ingressRootUrl = BuildUri(baseUrl, ingressRootPath);
		var oauthCallbackUrl = BuildUri(baseUrl, BuildPath(baseUrl.AbsolutePath, routePrefix, oauthPath, oauthCallbackPath));
		var interactionsUrl = BuildUri(baseUrl, BuildPath(baseUrl.AbsolutePath, routePrefix, interactionsPath));
		var webhookEventsUrl = BuildUri(baseUrl, BuildPath(baseUrl.AbsolutePath, routePrefix, webhooksPath, webhookEventsPath));
		var incomingWebhooksUrl = BuildUri(baseUrl, BuildPath(baseUrl.AbsolutePath, routePrefix, webhooksPath, incomingWebhooksPath));
		var roleConnectionsVerificationUrl = linkedRolesOptions is null
			? null
			: BuildUri(baseUrl, BuildPath(baseUrl.AbsolutePath, NormalizeRouteSegment(linkedRolesOptions.VerificationPath)));

		return new(baseUrl, ingressRootPath, ingressRootUrl, oauthCallbackUrl, interactionsUrl, webhookEventsUrl, incomingWebhooksUrl, roleConnectionsVerificationUrl);
	}

	private static Uri BuildUri(Uri baseUrl, string path)
	{
		UriBuilder builder = new(baseUrl)
		{
			Path = path,
			Query = string.Empty,
			Fragment = string.Empty
		};

		return builder.Uri;
	}

	private static string BuildPath(params string?[] parts)
	{
		var segments = new List<string>();
		foreach (var part in parts.Where(static part => !string.IsNullOrWhiteSpace(part)))
		{
			segments.AddRange(part
				.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Where(static segment => !string.IsNullOrWhiteSpace(segment)));
		}

		return segments.Count is 0
			? "/"
			: $"/{string.Join('/', segments)}";
	}

	private static string NormalizeRoutePrefix(string? routePrefix)
		=> string.IsNullOrWhiteSpace(routePrefix) || routePrefix.Trim() == "/" ? string.Empty : routePrefix.Trim().Trim('/');

	private static string NormalizeRouteSegment(string? segment)
	{
		return string.IsNullOrWhiteSpace(segment)
			? throw new ArgumentException("Route segments must contain a non-empty value.", nameof(segment))
			: segment.Trim().Trim('/');
	}
}
