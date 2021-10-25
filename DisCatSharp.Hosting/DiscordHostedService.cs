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
using System.Collections.Generic;
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

        private readonly ILogger<DiscordHostedService> _logger;

        #pragma warning disable 8618
        protected DiscordHostedService(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider)
        {
            this._logger = logger;
            this.Initialize(config, provider);
        }

        #pragma warning restore 8618

        /// <summary>
        /// Automatically search for and configure <see cref="Client"/>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="provider"></param>
        private void Initialize(IConfiguration config, IServiceProvider provider)
        {
            var typeMap = config.FindImplementedExtensions();

            this._logger.LogDebug($"Found the following config types: {string.Join("\n\t", typeMap.Keys)}");

            this.Client = config.BuildClient();

            foreach (var typePair in typeMap)
                try
                {
                    /*
                        If section is null --> utilize the default constructor
                        This means the extension was explicitly added in the 'Using' array,
                        but user did not wish to override any value(s) in the extension's config
                     */

                    object configInstance = typePair.Value.Section.HasValue
                        ? typePair.Value.Section.Value.ExtractConfig(() =>
                            ActivatorUtilities.CreateInstance(provider, typePair.Value.ConfigType))
                        : ActivatorUtilities.CreateInstance(provider, typePair.Value.ConfigType);

                    /*
                        Explanation for bindings

                        Internal Constructors --> NonPublic
                        Public Constructors --> Public
                        Constructors --> Instance
                     */

                    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                    var ctors = typePair.Value.ImplementationType.GetConstructors(flags);

                    object? instance;

                    /*
                       Certain extensions do not require a configuration argument
                       Those who do -- pass config instance in,
                       Those who don't -- simply instantiate

                       ActivatorUtilities requires a public constructor, anything with internal breaks
                     */

                    if (ctors.Any(x => x.GetParameters().Length == 1 && x.GetParameters().First().ParameterType == typePair.Value.ConfigType))
                        instance = Activator.CreateInstance(typePair.Value.ImplementationType, flags, null,
                                                    new[] { configInstance }, null);
                    else
                        instance = Activator.CreateInstance(typePair.Value.ImplementationType, true);

                    if (instance == null)
                    {
                        this._logger.LogError($"Unable to instantiate '{typePair.Value.ImplementationType.Name}'");
                        continue;
                    }

                    // Add an easy reference to our extensions for later use
                    this.Client.AddExtension((BaseExtension)instance);
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"Unable to register '{typePair.Value.ImplementationType.Name}': \n\t{ex.Message}");
                }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.Client == null)
                throw new NullReferenceException("Discord Client cannot be null");

            await this.PreConnect();
            await this.Client.ConnectAsync();
            await this.PostConnect();

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
