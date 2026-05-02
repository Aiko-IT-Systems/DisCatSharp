using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DisCatSharp.Hosting.DependencyInjection;

/// <summary>
///     Provides host builder extensions for registering DisCatSharp hosted services.
/// </summary>
public static class HostBuilderExtensions
{
	/// <summary>
	///     Adds your bot as a background service to the host application builder.
	/// </summary>
	/// <typeparam name="TService">The hosted Discord service implementation.</typeparam>
	/// <param name="builder">The host application builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDiscordHostedService<TService>(this IHostApplicationBuilder builder)
		where TService : class, IDiscordHostedService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDiscordHostedService<TService>();
		return builder;
	}

	/// <summary>
	///     Adds your bot as a sharded background service to the host application builder.
	/// </summary>
	/// <typeparam name="TService">The hosted Discord shard service implementation.</typeparam>
	/// <param name="builder">The host application builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDiscordHostedShardService<TService>(this IHostApplicationBuilder builder)
		where TService : class, IDiscordHostedShardService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDiscordHostedShardService<TService>();
		return builder;
	}

	/// <summary>
	///     Adds a Discord background service that can also be resolved via <typeparamref name="TInterface" /> to the host application builder.
	/// </summary>
	/// <typeparam name="TInterface">The service contract exposed from dependency injection.</typeparam>
	/// <typeparam name="TService">The hosted Discord service implementation.</typeparam>
	/// <param name="builder">The host application builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDiscordHostedService<TInterface, TService>(this IHostApplicationBuilder builder)
		where TInterface : class, IDiscordHostedService
		where TService : class, TInterface, IDiscordHostedService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDiscordHostedService<TInterface, TService>();
		return builder;
	}

	/// <summary>
	///     Adds a sharded Discord background service that can also be resolved via <typeparamref name="TInterface" /> to the host application builder.
	/// </summary>
	/// <typeparam name="TInterface">The service contract exposed from dependency injection.</typeparam>
	/// <typeparam name="TService">The hosted Discord shard service implementation.</typeparam>
	/// <param name="builder">The host application builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostApplicationBuilder AddDiscordHostedShardService<TInterface, TService>(this IHostApplicationBuilder builder)
		where TInterface : class, IDiscordHostedShardService
		where TService : class, TInterface, IDiscordHostedShardService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Services.AddDiscordHostedShardService<TInterface, TService>();
		return builder;
	}

	/// <summary>
	///     Adds your bot as a background service to the host builder.
	/// </summary>
	/// <typeparam name="TService">The hosted Discord service implementation.</typeparam>
	/// <param name="builder">The host builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDiscordHostedService<TService>(this IHostBuilder builder)
		where TService : class, IDiscordHostedService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices(static (_, services) => services.AddDiscordHostedService<TService>());
		return builder;
	}

	/// <summary>
	///     Adds your bot as a sharded background service to the host builder.
	/// </summary>
	/// <typeparam name="TService">The hosted Discord shard service implementation.</typeparam>
	/// <param name="builder">The host builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDiscordHostedShardService<TService>(this IHostBuilder builder)
		where TService : class, IDiscordHostedShardService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices(static (_, services) => services.AddDiscordHostedShardService<TService>());
		return builder;
	}

	/// <summary>
	///     Adds a Discord background service that can also be resolved via <typeparamref name="TInterface" /> to the host builder.
	/// </summary>
	/// <typeparam name="TInterface">The service contract exposed from dependency injection.</typeparam>
	/// <typeparam name="TService">The hosted Discord service implementation.</typeparam>
	/// <param name="builder">The host builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDiscordHostedService<TInterface, TService>(this IHostBuilder builder)
		where TInterface : class, IDiscordHostedService
		where TService : class, TInterface, IDiscordHostedService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices(static (_, services) => services.AddDiscordHostedService<TInterface, TService>());
		return builder;
	}

	/// <summary>
	///     Adds a sharded Discord background service that can also be resolved via <typeparamref name="TInterface" /> to the host builder.
	/// </summary>
	/// <typeparam name="TInterface">The service contract exposed from dependency injection.</typeparam>
	/// <typeparam name="TService">The hosted Discord shard service implementation.</typeparam>
	/// <param name="builder">The host builder to update.</param>
	/// <returns>The <paramref name="builder" /> for chaining purposes.</returns>
	public static IHostBuilder AddDiscordHostedShardService<TInterface, TService>(this IHostBuilder builder)
		where TInterface : class, IDiscordHostedShardService
		where TService : class, TInterface, IDiscordHostedShardService
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.ConfigureServices(static (_, services) => services.AddDiscordHostedShardService<TInterface, TService>());
		return builder;
	}
}
