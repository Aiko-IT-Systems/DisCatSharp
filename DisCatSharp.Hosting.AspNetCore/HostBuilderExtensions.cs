using System;

using DisCatSharp.Hosting.AspNetCore.Ingress;
using DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Provides host builder extensions for registering DisCatSharp ASP.NET Core ingress services.
/// </summary>
public static class HostBuilderExtensions
{
	/// <summary>
	///     Registers the DisCatSharp ASP.NET Core ingress services on a host application builder.
	/// </summary>
	/// <param name="builder">The host application builder to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <param name="configureOAuth">Optional configuration callback for the OAuth callback flow.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDisCatSharpAspNetCore(
		this IHostApplicationBuilder builder,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null,
		Action<DiscordOAuthIngressOptions>? configureOAuth = null
	)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDisCatSharpAspNetCore(configure, configureAspNetCore, configureOAuth);
		return builder;
	}

	/// <summary>
	///     Registers the optional self-hosted DisCatSharp ASP.NET Core ingress services on a host application builder.
	/// </summary>
	/// <param name="builder">The host application builder to update.</param>
	/// <param name="configureSelfHost">Optional configuration callback for the internal ASP.NET Core host.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDisCatSharpAspNetCoreSelfHost(
		this IHostApplicationBuilder builder,
		Action<DiscordAspNetCoreSelfHostOptions>? configureSelfHost = null,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null
	)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDisCatSharpAspNetCoreSelfHost(configureSelfHost, configure, configureAspNetCore);
		return builder;
	}

	/// <summary>
	///     Registers the DisCatSharp ASP.NET Core ingress services on a host builder.
	/// </summary>
	/// <param name="builder">The host builder to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <param name="configureOAuth">Optional configuration callback for the OAuth callback flow.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDisCatSharpAspNetCore(
		this IHostBuilder builder,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null,
		Action<DiscordOAuthIngressOptions>? configureOAuth = null
	)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices((_, services) => services.AddDisCatSharpAspNetCore(configure, configureAspNetCore, configureOAuth));
		return builder;
	}

	/// <summary>
	///     Registers the optional self-hosted DisCatSharp ASP.NET Core ingress services on a host builder.
	/// </summary>
	/// <param name="builder">The host builder to update.</param>
	/// <param name="configureSelfHost">Optional configuration callback for the internal ASP.NET Core host.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDisCatSharpAspNetCoreSelfHost(
		this IHostBuilder builder,
		Action<DiscordAspNetCoreSelfHostOptions>? configureSelfHost = null,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null
	)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices((_, services) => services.AddDisCatSharpAspNetCoreSelfHost(configureSelfHost, configure, configureAspNetCore));
		return builder;
	}
}
