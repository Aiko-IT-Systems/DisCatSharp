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
	/// <remarks>
	///     This is a convenience wrapper over <see cref="ServiceCollectionExtensions.AddDisCatSharpAspNetCore" /> for applications that
	///     configure services through <see cref="IHostApplicationBuilder" />.
	/// </remarks>
	/// <param name="builder">The host application builder to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <param name="configureOAuth">Optional configuration callback for the OAuth callback flow.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
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
	/// <remarks>
	///     This wraps <see cref="ServiceCollectionExtensions.AddDisCatSharpAspNetCoreSelfHost" /> and is intended for worker-style hosts
	///     that do not already manage an ASP.NET Core application.
	/// </remarks>
	/// <param name="builder">The host application builder to update.</param>
	/// <param name="configureSelfHost">Optional configuration callback for the internal ASP.NET Core host.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
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
	/// <remarks>
	///     This is the <see cref="IHostBuilder" /> counterpart to <see cref="AddDisCatSharpAspNetCore(IHostApplicationBuilder,Action{DiscordWebIngressOptions}?,Action{DiscordAspNetCoreIngressOptions}?,Action{DiscordOAuthIngressOptions}?)" />.
	/// </remarks>
	/// <param name="builder">The host builder to update.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <param name="configureOAuth">Optional configuration callback for the OAuth callback flow.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
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
	/// <remarks>
	///     This is the <see cref="IHostBuilder" /> counterpart to
	///     <see cref="AddDisCatSharpAspNetCoreSelfHost(IHostApplicationBuilder,Action{DiscordAspNetCoreSelfHostOptions}?,Action{DiscordWebIngressOptions}?,Action{DiscordAspNetCoreIngressOptions}?)" />.
	/// </remarks>
	/// <param name="builder">The host builder to update.</param>
	/// <param name="configureSelfHost">Optional configuration callback for the internal ASP.NET Core host.</param>
	/// <param name="configure">Optional configuration callback for the ingress subsystem.</param>
	/// <param name="configureAspNetCore">Optional configuration callback for ASP.NET Core endpoint conventions.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder" /> is <see langword="null" />.</exception>
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
