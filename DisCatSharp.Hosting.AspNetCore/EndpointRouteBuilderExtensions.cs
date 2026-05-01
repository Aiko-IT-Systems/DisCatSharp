using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore.Ingress;

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
	///     Maps the DisCatSharp OAuth ingress callback endpoint.
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

		group.MapGet(callbackPath, HandleOAuthCallbackAsync)
			.WithName(DiscordIngressEndpointNames.OAuthCallback)
			.WithDisplayName("DisCatSharp OAuth callback")
			.WithTags(DisCatSharpTag, IngressTag, OAuthModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(OAuthModule, DiscordIngressEndpointNames.OAuthCallback, $"{oauthPath}/{callbackPath}"))
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status403Forbidden)
			.Produces(StatusCodes.Status500InternalServerError)
			.Produces(StatusCodes.Status502BadGateway);

		return group;
	}

	/// <summary>
	///     Maps the DisCatSharp interactions ingress endpoint.
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

		group.MapPost(
				string.Empty,
				static (HttpRequest request, DiscordInteractionEndpointHandler handler, CancellationToken cancellationToken)
					=> handler.HandleAsync(request, cancellationToken).AsTask())
			.WithName(DiscordIngressEndpointNames.Interactions)
			.WithDisplayName("DisCatSharp interactions")
			.WithTags(DisCatSharpTag, IngressTag, InteractionsModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(InteractionsModule, DiscordIngressEndpointNames.Interactions, interactionsPath))
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status413PayloadTooLarge)
			.Produces(StatusCodes.Status501NotImplemented);

		return group;
	}

	/// <summary>
	///     Maps the DisCatSharp webhook ingress endpoints.
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

		group.MapPost(
				webhookEventsPath,
				static (HttpRequest request, DiscordWebhookEventEndpointHandler handler, CancellationToken cancellationToken)
					=> handler.HandleAsync(request, cancellationToken).AsTask())
			.WithName(DiscordIngressEndpointNames.WebhookEvents)
			.WithDisplayName("DisCatSharp webhook events")
			.WithTags(DisCatSharpTag, IngressTag, WebhooksModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(WebhooksModule, DiscordIngressEndpointNames.WebhookEvents, $"{webhooksPath}/{webhookEventsPath}"))
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status413PayloadTooLarge);

		group.MapPost(
				incomingWebhooksPath,
				static (HttpRequest request, DiscordIncomingWebhookEndpointHandler handler, CancellationToken cancellationToken)
					=> handler.HandleAsync(request, cancellationToken).AsTask())
			.WithName(DiscordIngressEndpointNames.IncomingWebhooks)
			.WithDisplayName("DisCatSharp incoming webhooks")
			.WithTags(DisCatSharpTag, IngressTag, WebhooksModule, IncomingWebhooksModule)
			.WithMetadata(new DiscordIngressEndpointMetadata(IncomingWebhooksModule, DiscordIngressEndpointNames.IncomingWebhooks, $"{webhooksPath}/{incomingWebhooksPath}"))
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status202Accepted)
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status413PayloadTooLarge)
			.Produces(StatusCodes.Status501NotImplemented);

		return group;
	}

	private static async Task<IResult> HandleOAuthCallbackAsync(
		HttpContext httpContext,
		IDiscordOAuthCallbackHandler callbackHandler,
		IDiscordOAuthCallbackResponseFactory responseFactory,
		CancellationToken cancellationToken
	)
	{
		var request = CreateOAuthCallbackRequest(httpContext.Request);
		var result = await callbackHandler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
		return new DiscordIngressHttpResult(responseFactory.CreateResponse(result));
	}

	private static DiscordAspNetCoreIngressOptions GetOptions(IEndpointRouteBuilder endpoints)
	{
		var options = endpoints.ServiceProvider.GetService<IOptions<DiscordAspNetCoreIngressOptions>>();
		return options?.Value ?? throw new InvalidOperationException("DisCatSharp ASP.NET Core ingress services have not been registered. Call AddDisCatSharpAspNetCore before mapping ingress endpoints.");
	}

	private static string NormalizeRoutePrefix(string routePrefix)
	{
		ArgumentNullException.ThrowIfNull(routePrefix);

		var normalizedPrefix = routePrefix.Trim();
		return string.IsNullOrEmpty(normalizedPrefix) || normalizedPrefix == "/" ? string.Empty : $"/{normalizedPrefix.Trim('/')}";
	}

	private static string NormalizeRouteSegment(string segment)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(segment);

		return segment.Trim().Trim('/');
	}

	private static DiscordOAuthCallbackRequest CreateOAuthCallbackRequest(HttpRequest request)
	{
		Dictionary<string, string?> queryParameters = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in request.Query)
			queryParameters[key] = string.IsNullOrWhiteSpace(value.ToString()) ? null : value.ToString();

		Uri? callbackUri = null;
		if (request.Host.HasValue && !string.IsNullOrWhiteSpace(request.Scheme))
			callbackUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}");

		return new DiscordOAuthCallbackRequest(
			code: GetQueryValue(request, "code"),
			state: GetQueryValue(request, "state"),
			error: GetQueryValue(request, "error"),
			errorDescription: GetQueryValue(request, "error_description"),
			callbackUri: callbackUri,
			queryParameters: queryParameters);
	}

	private static string? GetQueryValue(HttpRequest request, string key)
	{
		var value = request.Query.TryGetValue(key, out var queryValue) ? queryValue.ToString() : null;
		return string.IsNullOrWhiteSpace(value) ? null : value;
	}
}
