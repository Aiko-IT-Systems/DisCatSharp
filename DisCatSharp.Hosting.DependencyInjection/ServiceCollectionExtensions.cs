// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Hosting.DependencyInjection;

/// <summary>
/// The service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds your bot as a BackgroundService, registered in Dependency Injection as <typeparamref name="TService"/>
	/// </summary>
	/// <remarks>
	/// <see cref="IDiscordHostedService"/> is scoped to ServiceLifetime.Singleton. <br/>
	/// Maps to Implementation of <typeparamref name="TService"/>
	/// </remarks>
	/// <param name="services"></param>
	/// <typeparam name="TService"></typeparam>
	/// <returns>Reference to <paramref name="services"/> for chaining purposes</returns>
	public static IServiceCollection AddDiscordHostedService<TService>(this IServiceCollection services)
		where TService : class, IDiscordHostedService
	{
		services.AddSingleton<TService>();
		services.AddHostedService(provider => provider.GetRequiredService<TService>());
		return services;
	}

	/// <summary>
	/// Adds your bot as a BackgroundService, registered in Dependency Injection as <typeparamref name="TService"/>
	/// </summary>
	/// <remarks>
	/// <see cref="IDiscordHostedShardService"/> is scoped to ServiceLifetime.Singleton. <br/>
	/// Maps to Implementation of <typeparamref name="TService"/>
	/// </remarks>
	/// <param name="services"></param>
	/// <typeparam name="TService"></typeparam>
	/// <returns>Reference to <paramref name="services"/> for chaining purposes</returns>
	public static IServiceCollection AddDiscordHostedShardService<TService>(this IServiceCollection services)
		where TService : class, IDiscordHostedShardService
	{
		services.AddSingleton<TService>();
		services.AddHostedService(provider => provider.GetRequiredService<TService>());
		return services;
	}

	/// <summary>
	/// Add <typeparamref name="TService"/> as a background service which derives from
	/// <typeparamref name="TInterface"/> and <see cref="IDiscordHostedService"/>
	/// </summary>
	/// <remarks>
	/// To retrieve your bot via Dependency Injection you can reference it via <typeparamref name="TInterface"/>
	/// </remarks>
	/// <param name="services"></param>
	/// <typeparam name="TInterface">Interface which <typeparamref name="TService"/> inherits from</typeparam>
	/// <typeparam name="TService">Your custom bot</typeparam>
	/// <returns>Reference to <paramref name="services"/> for chaining purposes</returns>
	public static IServiceCollection AddDiscordHostedService<TInterface, TService>(this IServiceCollection services)
		where TInterface : class, IDiscordHostedService
		where TService : class, TInterface, IDiscordHostedService
	{
		services.AddSingleton<TInterface, TService>();
		services.AddHostedService(provider => provider.GetRequiredService<TInterface>());

		return services;
	}

	/// <summary>
	/// Add <typeparamref name="TService"/> as a background service which derives from
	/// <typeparamref name="TInterface"/> and <see cref="IDiscordHostedShardService"/>
	/// </summary>
	/// <remarks>
	/// To retrieve your bot via Dependency Injection you can reference it via <typeparamref name="TInterface"/>
	/// </remarks>
	/// <param name="services"></param>
	/// <typeparam name="TInterface">Interface which <typeparamref name="TService"/> inherits from</typeparam>
	/// <typeparam name="TService">Your custom bot</typeparam>
	/// <returns>Reference to <paramref name="services"/> for chaining purposes</returns>
	public static IServiceCollection AddDiscordHostedShardService<TInterface, TService>(
		this IServiceCollection services)
		where TInterface : class, IDiscordHostedShardService
		where TService : class, TInterface, IDiscordHostedShardService
	{
		services.AddSingleton<TInterface, TService>();
		services.AddHostedService(provider => provider.GetRequiredService<TInterface>());
		return services;
	}
}
