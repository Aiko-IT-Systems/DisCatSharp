using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Hosting.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add <see cref="IDiscordHostedService"/> as a background service
        /// </summary>
        /// <remarks>
        /// <see cref="IDiscordHostedService"/> is scoped to <see cref="ServiceLifetime.Singleton"/>. <br/>
        /// Maps to implementation of <see cref="DiscordHostedService"/>
        /// </remarks>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDiscordHostedService(this IServiceCollection services)
        {
            services.AddSingleton<IDiscordHostedService, DiscordHostedService>();
            services.AddHostedService(provider => provider.GetRequiredService<IDiscordHostedService>());

            return services;
        }

        /// <summary>
        /// Add <see cref="IDiscordHostedService"/> as a background service
        /// </summary>
        /// <remarks>
        /// <see cref="IDiscordHostedService"/> is scoped to <see cref="ServiceLifetime.Singleton"/>. <br/>
        /// Maps to Implementation of <typeparamref name="TService"/>
        /// </remarks>
        /// <param name="services"></param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddDiscordHostedService<TService>(this IServiceCollection services)
            where TService : class, IDiscordHostedService
        {
            services.AddSingleton<IDiscordHostedService, TService>();
            services.AddHostedService(provider => provider.GetRequiredService<IDiscordHostedService>());
            return services;
        }
    }
}
