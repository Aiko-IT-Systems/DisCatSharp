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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting
{
    public class DiscordHostedService : BackgroundService, IDiscordHostedService
    {
        public DiscordClient Client { get; private set; }

        private readonly ILogger<DiscordHostedService> _logger;

        public Dictionary<string, object> Extensions { get; } = new();

        public DiscordHostedService(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider)
        {
            this._logger = logger;
            this.Initialize(config, provider);
        }

        internal void Initialize(IConfiguration config, IServiceProvider provider)
        {
            var typeMap = config.FindImplementedConfigs();

            foreach (var typePair in typeMap)
                try
                {
                    // First we must create an instance of the configuration type
                    object configInstance = ActivatorUtilities.CreateInstance(provider, typePair.Value.ConfigType);

                    // Secondly -- instantiate corresponding implemented types
                    if (typePair.Value.ImplementationType == typeof(DiscordClient))
                    {
                        this.Client = (DiscordClient)ActivatorUtilities.CreateInstance(provider,
                            typePair.Value.ImplementationType, configInstance);

                        continue;
                    }

                    object instance = ActivatorUtilities.CreateInstance(provider, typePair.Value.ImplementationType, configInstance);

                    this.Extensions.Add(typePair.Value.ImplementationType.Name, instance);
                    this.Client.AddExtension((BaseExtension) instance);
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

            // Wait indefinitely -- but use stopping token so we can properly cancel if needed
            await Task.Delay(-1, stoppingToken);
        }

    }
}
