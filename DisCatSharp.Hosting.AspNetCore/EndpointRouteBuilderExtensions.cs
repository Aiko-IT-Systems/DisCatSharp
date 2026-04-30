using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Provides ASP.NET Core endpoint mapping extensions for the DisCatSharp web ingress surface.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
	private const string DisCatSharpTag = "DisCatSharp";
	private const string IngressTag = "Ingress";
	private const string OAuthModule = "OAuth";
	private const string InteractionsModule = "Interactions";
	private const string WebhooksModule = "Webhooks";
	private const string IncomingWebhooksModule = "IncomingWebhooks";

	/// <summary>
	///     Maps the default DisCatSharp ingress surface under the configured route prefix.
	/// </summary>
	/// <param name="endpoints">The endpoint route builder to update.</param>
	/// <returns>The configured ingress route group.</returns>
	public static RouteGroupBuilder MapDisCatSharpIngress(this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var options = GetOptions(endpoints);
		var group = endpoints.MapGroup(NormalizeRoutePrefix(options.RoutePrefix))
			.WithTags(DisCatSharpTag, IngressTag);

		group.MapDiscordOAuthIngress();
		group.MapDiscordInteractionIngress();
		group.MapDiscordWebhookIngress();

		return group;
	}

	/// <summary>
	///     Maps the DisCatSharp OAuth ingress placeholder endpoints.
	/// </summary>
	/// <param name="endpoints">The endpoint route builder to update.</param>
	/// <returns>The configured OAuth route group.</returns>
	public static RouteGroupBuilder MapDiscordOAuthIngress(this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var options = GetOptions(endpoints);
		var oauthPath = NormalizeRouteSegment(options.OAuthPath);
		var callbackPath = NormalizeRouteSegment(options.OAuthCallbackPath);
		var group = endpoints.MapGroup(oauthPath)
			.WithTags(DisCatSharpTag, IngressTag, OAuthModule);

		group.MapGet(callbackPath, static () => CreatePlaceholderResult("The Discord OAuth callback endpoint is not implemented yet."))
			.WithName(DiscordIngressEndpointNames.OAuthCallback)
			.WithDisplayName("DisCatSharp OAuth callback")
			.WithTags(DisCatSharpTag, IngressTag, OAuthModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(OAuthModule, DiscordIngressEndpointNames.OAuthCallback, $"{oauthPath}/{callbackPath}"))
			.Produces(StatusCodes.Status501NotImplemented);

		return group;
	}

	/// <summary>
	///     Maps the DisCatSharp interactions ingress placeholder endpoint.
	/// </summary>
	/// <param name="endpoints">The endpoint route builder to update.</param>
	/// <returns>The configured interactions route group.</returns>
	public static RouteGroupBuilder MapDiscordInteractionIngress(this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var options = GetOptions(endpoints);
		var interactionsPath = NormalizeRouteSegment(options.InteractionsPath);
		var group = endpoints.MapGroup(interactionsPath)
			.WithTags(DisCatSharpTag, IngressTag, InteractionsModule);

		group.MapPost(string.Empty, static () => CreatePlaceholderResult("The Discord interactions endpoint is not implemented yet."))
			.WithName(DiscordIngressEndpointNames.Interactions)
			.WithDisplayName("DisCatSharp interactions")
			.WithTags(DisCatSharpTag, IngressTag, InteractionsModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(InteractionsModule, DiscordIngressEndpointNames.Interactions, interactionsPath))
			.Produces(StatusCodes.Status501NotImplemented);

		return group;
	}

	/// <summary>
	///     Maps the DisCatSharp webhook ingress placeholder endpoints.
	/// </summary>
	/// <param name="endpoints">The endpoint route builder to update.</param>
	/// <returns>The configured webhooks route group.</returns>
	public static RouteGroupBuilder MapDiscordWebhookIngress(this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		var options = GetOptions(endpoints);
		var webhooksPath = NormalizeRouteSegment(options.WebhooksPath);
		var webhookEventsPath = NormalizeRouteSegment(options.WebhookEventsPath);
		var incomingWebhooksPath = NormalizeRouteSegment(options.IncomingWebhooksPath);
		var group = endpoints.MapGroup(webhooksPath)
			.WithTags(DisCatSharpTag, IngressTag, WebhooksModule);

		group.MapPost(webhookEventsPath, static () => CreatePlaceholderResult("The Discord webhook events endpoint is not implemented yet."))
			.WithName(DiscordIngressEndpointNames.WebhookEvents)
			.WithDisplayName("DisCatSharp webhook events")
			.WithTags(DisCatSharpTag, IngressTag, WebhooksModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(WebhooksModule, DiscordIngressEndpointNames.WebhookEvents, $"{webhooksPath}/{webhookEventsPath}"))
			.Produces(StatusCodes.Status501NotImplemented);

		group.MapPost(incomingWebhooksPath, static () => CreatePlaceholderResult("The DisCatSharp incoming webhook endpoint is not implemented yet."))
			.WithName(DiscordIngressEndpointNames.IncomingWebhooks)
			.WithDisplayName("DisCatSharp incoming webhooks")
			.WithTags(DisCatSharpTag, IngressTag, WebhooksModule, IncomingWebhooksModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(IncomingWebhooksModule, DiscordIngressEndpointNames.IncomingWebhooks, $"{webhooksPath}/{incomingWebhooksPath}"))
			.Produces(StatusCodes.Status501NotImplemented);

		return group;
	}

	private static IResult CreatePlaceholderResult(string detail)
		=> Results.Problem(
			statusCode: StatusCodes.Status501NotImplemented,
			title: "DisCatSharp ingress endpoint is not implemented.",
			detail: detail);

	private static DiscordAspNetCoreIngressOptions GetOptions(IEndpointRouteBuilder endpoints)
	{
		var options = endpoints.ServiceProvider.GetService<IOptions<DiscordAspNetCoreIngressOptions>>();
		return options?.Value ?? throw new InvalidOperationException("DisCatSharp ASP.NET Core ingress services have not been registered. Call AddDisCatSharpAspNetCore before mapping ingress endpoints.");
	}

	private static string NormalizeRoutePrefix(string routePrefix)
	{
		ArgumentNullException.ThrowIfNull(routePrefix);

		var normalizedPrefix = routePrefix.Trim();
		if (string.IsNullOrEmpty(normalizedPrefix) || normalizedPrefix == "/")
			return string.Empty;

		return $"/{normalizedPrefix.Trim('/')}";
	}

	private static string NormalizeRouteSegment(string segment)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(segment);

		return segment.Trim().Trim('/');
	}
}
