// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting
{
    /// <summary>
    /// Simple implementation for <see cref="DiscordClient"/> to work as a <see cref="BackgroundService"/>
    /// </summary>
    public abstract class DiscordHostedService : BackgroundService, IDiscordHostedService
    {
        /// <inheritdoc/>
        public DiscordClient Client { get; private set; }

        protected readonly ILogger<DiscordHostedService> Logger;
        protected readonly IHostApplicationLifetime ApplicationLifetime;

        #pragma warning disable 8618
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordHostedService"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="applicationLifetime">Current hosting environment. This will be used for shutting down the application on error</param>
        /// <param name="configBotSection">Name within the configuration which contains the config info for our bot. Default is DisCatSharp</param>
        protected DiscordHostedService(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider, IHostApplicationLifetime applicationLifetime, string configBotSection = Configuration.ConfigurationExtensions.DefaultRootLib)
        {
            this.Logger = logger;
            this.ApplicationLifetime = applicationLifetime;
            this.Initialize(config, provider, configBotSection);
        }

        #pragma warning restore 8618

        /// <summary>
        /// When the bot fails to start, this method will be invoked. (Default behavior is to shutdown)
        /// </summary>
        /// <param name="ex">The exception/reason the bot couldn't start</param>
        protected virtual void OnInitializationError(Exception ex)
        {
            this.ApplicationLifetime.StopApplication();
        }

        /// <summary>
        /// Automatically search for and configure <see cref="Client"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="provider"></param>
        /// <param name="configBotSection">Name within the configuration which contains the config info for our bot</param>
        private void Initialize(IConfiguration config, IServiceProvider provider, string configBotSection)
        {
            var typeMap = config.FindImplementedExtensions(configBotSection);

            this.Logger.LogDebug($"Found the following config types: {string.Join("\n\t", typeMap.Keys)}");

            try
            {
                this.Client = config.BuildClient(configBotSection);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Was unable to build {nameof(DiscordClient)} for {this.GetType().Name}");
                this.OnInitializationError(ex);
            }

            foreach (var typePair in typeMap)
                try
                {
                    /*
                        If section is null --> utilize the default constructor
                        This means the extension was explicitly added in the 'Using' array,
                        but user did not wish to override any value(s) in the extension's config
                     */

                    var configInstance = typePair.Value.Section.HasValue
                        ? typePair.Value.Section.Value.ExtractConfig(() =>
                            ActivatorUtilities.CreateInstance(provider, typePair.Value.ConfigType))
                        : ActivatorUtilities.CreateInstance(provider, typePair.Value.ConfigType);

                    /*
                        Explanation for bindings

                        Internal Constructors --> NonPublic
                        Public Constructors --> Public
                        Constructors --> Instance
                     */

                    var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                    var ctors = typePair.Value.ImplementationType.GetConstructors(flags);

                    var instance = ctors.Any(x => x.GetParameters().Length == 1 && x.GetParameters().First().ParameterType == typePair.Value.ConfigType)
                        ? Activator.CreateInstance(typePair.Value.ImplementationType, flags, null,
                                                    new[] { configInstance }, null)
                        : Activator.CreateInstance(typePair.Value.ImplementationType, true);

                    /*
                       Certain extensions do not require a configuration argument
                       Those who do -- pass config instance in,
                       Those who don't -- simply instantiate

                       ActivatorUtilities requires a public constructor, anything with internal breaks
                     */


                    if (instance == null)
                    {
                        this.Logger.LogError($"Unable to instantiate '{typePair.Value.ImplementationType.Name}'");
                        continue;
                    }

                    // Add an easy reference to our extensions for later use
                    this.Client.AddExtension((BaseExtension)instance);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError($"Unable to register '{typePair.Value.ImplementationType.Name}': \n\t{ex.Message}");
                    this.OnInitializationError(ex);
                }
        }

        /// <summary>
        /// Executes the bot.
        /// </summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>A Task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (this.Client == null)
                    throw new NullReferenceException("Discord Client cannot be null");

                await this.PreConnect();
                await this.Client.ConnectAsync();
                await this.PostConnect();
            }
            catch (Exception ex)
            {
                /*
                 * Anything before DOTNET 6 will
                 * fail silently despite throwing an exception in this method
                 * So to overcome this obstacle we need to log what happened and manually exit
                 */

                this.Logger.LogError(ex, $"Was unable to start {this.GetType().Name} Bot as a hosted service.");
                this.OnInitializationError(ex);
            }

            // Wait indefinitely -- but use stopping token so we can properly cancel if needed
            await Task.Delay(-1, stoppingToken);
        }

        /// <summary>
        /// Runs just prior to the bot connecting
        /// </summary>
        protected virtual Task PreConnect() => Task.CompletedTask;

        /// <summary>
        /// Runs immediately after the bot connects
        /// </summary>
        protected virtual Task PostConnect() => Task.CompletedTask;

    }
}
