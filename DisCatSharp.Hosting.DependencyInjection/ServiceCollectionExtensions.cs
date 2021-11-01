using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Hosting.DependencyInjection
{
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
        /// <returns></returns>
        public static IServiceCollection AddDiscordHostedService<TService>(this IServiceCollection services)
            where TService : class, IDiscordHostedService
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
        /// <returns></returns>
        public static IServiceCollection AddDiscordHostedService<TInterface, TService>(this IServiceCollection services)
            where TInterface :  class, IDiscordHostedService
            where TService : class, TInterface, IDiscordHostedService
        {
            services.AddSingleton<TInterface, TService>();
            services.AddHostedService(provider => provider.GetRequiredService<TInterface>());

            return services;
        }
    }
}
