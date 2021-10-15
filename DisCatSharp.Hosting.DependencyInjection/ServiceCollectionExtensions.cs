using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Hosting.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add <see cref="IDiscordHostedService"/> as a background service
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
            services.AddSingleton<IDiscordHostedService, TService>();
            services.AddHostedService(provider => provider.GetRequiredService<IDiscordHostedService>());
            return services;
        }
    }
}
