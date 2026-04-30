using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
	/// </remarks>
	/// <param name="services">The service collection to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The service collection for chaining purposes.</returns>
	public static IServiceCollection AddDisCatSharpAspNetCore(
		this IServiceCollection services,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null
	)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddRouting();
		services.AddOptions<DiscordWebIngressOptions>()
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

		if (configure is not null)
			services.Configure<DiscordWebIngressOptions>(configure);
		if (configureAspNetCore is not null)
			services.Configure<DiscordAspNetCoreIngressOptions>(configureAspNetCore);

		services.TryAddSingleton<TimeProvider>(TimeProvider.System);
		services.TryAddSingleton<IDiscordIngressBodyReader, DiscordIngressBodyReader>();
		services.TryAddSingleton<IDiscordIngressPendingStateStore, InMemoryDiscordIngressPendingStateStore>();
		services.TryAddTransient<IDiscordIngressSignatureValidationService, DiscordIngressSignatureValidationService>();

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

	private static bool IsValidRouteSegment(string? segment)
		=> !string.IsNullOrWhiteSpace(segment) && segment.AsSpan().Trim().Trim('/').Length > 0;
}
