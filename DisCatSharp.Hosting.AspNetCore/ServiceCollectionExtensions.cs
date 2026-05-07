using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;
using DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;
using DisCatSharp.Hosting.AspNetCore.Ingress.Interactions;
using DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;
using DisCatSharp.Hosting.AspNetCore.Ingress.Security;
using DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents;
using DisCatSharp.Hosting.AspNetCore.LinkedRoles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Provides ASP.NET Core service registration extensions for DisCatSharp.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	///     Registers the baseline ASP.NET Core service dependencies used by the DisCatSharp web ingress scaffold.
	/// </summary>
	/// <remarks>
	///     This scaffold wires the transport-agnostic ingress core services that future HTTP transports can build on.
	///     It does not register application commands or bridge incoming HTTP interactions into the
	///     <c>ApplicationCommandsExtension</c> module.
	/// </remarks>
	/// <param name="services">The service collection to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <param name="configureOAuth">Optional configuration callback for the OAuth callback flow.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDisCatSharpAspNetCore(
		this IServiceCollection services,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null,
		Action<DiscordOAuthIngressOptions>? configureOAuth = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddRouting();
		services.AddLogging();
		services.AddOptions<DiscordWebIngressOptions>()
			.Validate(static options => IsValidVerifyKey(options.ApplicationVerifyKey), $"{nameof(DiscordWebIngressOptions.ApplicationVerifyKey)} must be a 64-character hex encoded Ed25519 public key.")
			.Validate(static options => options.MaxRequestBodySize > 0, $"{nameof(DiscordWebIngressOptions.MaxRequestBodySize)} must be greater than zero.")
			.Validate(static options => options.PendingStateLifetime > TimeSpan.Zero, $"{nameof(DiscordWebIngressOptions.PendingStateLifetime)} must be greater than zero.")
			.Validate(static options => options.PendingStateCleanupInterval > TimeSpan.Zero, $"{nameof(DiscordWebIngressOptions.PendingStateCleanupInterval)} must be greater than zero.");
		services.AddOptions<DiscordAspNetCoreIngressOptions>()
			.Validate(static options => options.RoutePrefix is not null, $"{nameof(DiscordAspNetCoreIngressOptions.RoutePrefix)} cannot be null.")
			.Validate(static options => IsValidRouteSegment(options.OAuthPath), $"{nameof(DiscordAspNetCoreIngressOptions.OAuthPath)} must contain a route segment.")
			.Validate(static options => IsValidRouteSegment(options.OAuthCallbackPath), $"{nameof(DiscordAspNetCoreIngressOptions.OAuthCallbackPath)} must contain a route segment.")
			.Validate(static options => IsValidRouteSegment(options.InteractionsPath), $"{nameof(DiscordAspNetCoreIngressOptions.InteractionsPath)} must contain a route segment.")
			.Validate(static options => IsValidRouteSegment(options.WebhooksPath), $"{nameof(DiscordAspNetCoreIngressOptions.WebhooksPath)} must contain a route segment.")
			.Validate(static options => IsValidRouteSegment(options.WebhookEventsPath), $"{nameof(DiscordAspNetCoreIngressOptions.WebhookEventsPath)} must contain a route segment.")
			.Validate(static options => IsValidRouteSegment(options.IncomingWebhooksPath), $"{nameof(DiscordAspNetCoreIngressOptions.IncomingWebhooksPath)} must contain a route segment.");
		services.AddOptions<DiscordOAuthIngressOptions>()
			.Validate(static options => !string.IsNullOrWhiteSpace(options.PendingStateFlow), $"{nameof(DiscordOAuthIngressOptions.PendingStateFlow)} must contain a flow name.")
			.Validate(static options => AreOAuthOptionsConsistent(options), "Discord OAuth ingress options must configure ClientId, ClientSecret, and RedirectUri together.")
			.Validate(static options => IsValidRedirectUri(options.RedirectUri), $"{nameof(DiscordOAuthIngressOptions.RedirectUri)} must be an absolute URI when configured.");
		services.AddOptions<DiscordLinkedRolesOptions>()
			.Validate(static options => IsValidRouteSegment(options.VerificationPath), $"{nameof(DiscordLinkedRolesOptions.VerificationPath)} must contain a route segment.");

		if (configure is not null)
			services.Configure(configure);
		if (configureAspNetCore is not null)
			services.Configure(configureAspNetCore);
		if (configureOAuth is not null)
			services.Configure(configureOAuth);

		services.TryAddSingleton(TimeProvider.System);
		services.TryAddSingleton<IDiscordIngressBodyReader, DiscordIngressBodyReader>();
		services.TryAddSingleton<IDiscordIngressPendingStateStore, InMemoryDiscordIngressPendingStateStore>();
		services.TryAddEnumerable(ServiceDescriptor.Transient<IDiscordIngressSignatureValidator, DiscordEd25519IngressSignatureValidator>());
		services.TryAddTransient<IDiscordIngressSignatureValidationService, DiscordIngressSignatureValidationService>();
		services.TryAddSingleton<DiscordLinkedRolesService>();
		services.TryAddTransient<DiscordInteractionIngressService>();
		services.TryAddTransient<DiscordInteractionEndpointHandler>();
		services.TryAddSingleton<DiscordWebhookEventDispatcher>();
		services.TryAddTransient<DiscordWebhookEventIngressService>();
		services.TryAddTransient<DiscordWebhookEventEndpointHandler>();
		services.TryAddTransient<DiscordIncomingWebhookIngressService>();
		services.TryAddTransient<DiscordIncomingWebhookEndpointHandler>();
		services.TryAddTransient<IDiscordOAuthTokenExchangeService, DiscordOAuthTokenExchangeService>();
		services.TryAddTransient<IDiscordOAuthCallbackHandler, DiscordOAuthCallbackHandler>();
		services.TryAddSingleton<IDiscordOAuthCallbackResponseFactory, DiscordOAuthCallbackResponseFactory>();

		return services;
	}

	/// <summary>
	///     Registers the optional self-hosted ASP.NET Core ingress infrastructure for applications that do not already own an ASP.NET Core app.
	/// </summary>
	/// <param name="services">The service collection to update.</param>
	/// <param name="configureSelfHost">Optional configuration callback for the internal ASP.NET Core host.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDisCatSharpAspNetCoreSelfHost(
		this IServiceCollection services,
		Action<DiscordAspNetCoreSelfHostOptions>? configureSelfHost = null,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddDisCatSharpAspNetCore(configure, configureAspNetCore);
		services.AddOptions<DiscordAspNetCoreSelfHostOptions>()
			.Validate(static options => !string.IsNullOrWhiteSpace(options.ListenAddress), $"{nameof(DiscordAspNetCoreSelfHostOptions.ListenAddress)} cannot be null or whitespace.")
			.Validate(static options => options.ListenPort is >= 0 and <= 65535, $"{nameof(DiscordAspNetCoreSelfHostOptions.ListenPort)} must be between 0 and 65535.")
			.Validate(static options => IsValidScheme(options.Scheme), $"{nameof(DiscordAspNetCoreSelfHostOptions.Scheme)} must contain a valid URI scheme.")
			.Validate(static options => options.BaseUrl is null || options.BaseUrl.IsAbsoluteUri, $"{nameof(DiscordAspNetCoreSelfHostOptions.BaseUrl)} must be an absolute URI when provided.")
			.Validate(static options => options.BaseUrl is null || (string.IsNullOrEmpty(options.BaseUrl.Query) && string.IsNullOrEmpty(options.BaseUrl.Fragment)), $"{nameof(DiscordAspNetCoreSelfHostOptions.BaseUrl)} cannot include query or fragment components.");

		if (configureSelfHost is not null)
			services.Configure(configureSelfHost);

		services.TryAddSingleton<DiscordAspNetCoreSelfHostRuntime>();
		services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, DiscordAspNetCoreSelfHostService>());

		return services;
	}

	/// <summary>
	///     Registers a transport-agnostic ingress signature validator.
	/// </summary>
	/// <typeparam name="TValidator">The validator implementation to add.</typeparam>
	/// <param name="services">The service collection to update.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDiscordIngressSignatureValidator<TValidator>(this IServiceCollection services)
		where TValidator : class, IDiscordIngressSignatureValidator
	{
		ArgumentNullException.ThrowIfNull(services);

		services.TryAddEnumerable(ServiceDescriptor.Transient<IDiscordIngressSignatureValidator, TValidator>());

		return services;
	}

	/// <summary>
	///     Registers a Discord HTTP interactions ingress handler.
	/// </summary>
	/// <typeparam name="THandler">The handler implementation to add.</typeparam>
	/// <param name="services">The service collection to update.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDiscordInteractionIngressHandler<THandler>(this IServiceCollection services)
		where THandler : class, IDiscordInteractionIngressHandler
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddTransient<IDiscordInteractionIngressHandler, THandler>();

		return services;
	}

	/// <summary>
	///     Registers a generic incoming webhook ingress handler.
	/// </summary>
	/// <typeparam name="THandler">The handler implementation to add.</typeparam>
	/// <param name="services">The service collection to update.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDiscordIncomingWebhookHandler<THandler>(this IServiceCollection services)
		where THandler : class, IDiscordIncomingWebhookHandler
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddTransient<IDiscordIncomingWebhookHandler, THandler>();

		return services;
	}

	/// <summary>
	///     Registers linked-roles helpers for verification URL generation, metadata synchronization, and OAuth role connection updates.
	/// </summary>
	/// <param name="services">The service collection to update.</param>
	/// <param name="configure">Optional configuration callback for linked-roles support.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDiscordLinkedRolesSupport(this IServiceCollection services, Action<DiscordLinkedRolesOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		if (configure is not null)
			services.Configure(configure);

		services.TryAddSingleton<DiscordLinkedRolesService>();
		return services;
	}

	/// <summary>
	///     Registers the metadata provider used by <see cref="DiscordLinkedRolesService" /> when synchronizing linked-roles metadata.
	/// </summary>
	/// <typeparam name="TProvider">The metadata provider implementation to add.</typeparam>
	/// <param name="services">The service collection to update.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDiscordLinkedRolesMetadataProvider<TProvider>(this IServiceCollection services)
		where TProvider : class, IDiscordLinkedRolesMetadataProvider
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<IDiscordLinkedRolesMetadataProvider, TProvider>();
		return services;
	}

	private static bool IsValidRouteSegment(string? segment)
		=> !string.IsNullOrWhiteSpace(segment) && segment.AsSpan().Trim().Trim('/').Length > 0;

	private static bool IsValidVerifyKey(string? verifyKey)
	{
		if (string.IsNullOrWhiteSpace(verifyKey))
			return true;

		var normalizedVerifyKey = verifyKey.Trim();
		if (normalizedVerifyKey.Length != 64)
			return false;

		try
		{
			return Convert.FromHexString(normalizedVerifyKey).Length == 32;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static bool AreOAuthOptionsConsistent(DiscordOAuthIngressOptions options)
	{
		var hasAnyConfiguration = options.ClientId != 0 || !string.IsNullOrWhiteSpace(options.ClientSecret) || !string.IsNullOrWhiteSpace(options.RedirectUri);
		return !hasAnyConfiguration || options.IsConfigured;
	}

	private static bool IsValidRedirectUri(string? redirectUri)
		=> string.IsNullOrWhiteSpace(redirectUri) || Uri.TryCreate(redirectUri, UriKind.Absolute, out _);

	private static bool IsValidScheme(string? scheme)
		=> !string.IsNullOrWhiteSpace(scheme) && Uri.CheckSchemeName(scheme.Trim());
}
