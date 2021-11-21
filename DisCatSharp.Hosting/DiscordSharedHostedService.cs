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
using System.Threading.Tasks;
using DisCatSharp.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting
{
    /// <summary>
    /// Simple Implementation for <see cref="DiscordShardedClient"/> to work as a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/>
    /// </summary>
    public abstract class DiscordShardedHostedService : BaseHostedService, IDiscordHostedShardService
    {
        public DiscordShardedClient ShardedClient { get; protected set; }

        #pragma warning disable 8618
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordShardedHostedService"/> class.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="configBotSection">The config bot section.</param>
        protected DiscordShardedHostedService(IConfiguration config,
            ILogger<DiscordShardedHostedService> logger,
            IServiceProvider serviceProvider,
            IHostApplicationLifetime applicationLifetime,
            string configBotSection = DisCatSharp.Configuration.ConfigurationExtensions.DefaultRootLib)
            : base(config, logger, serviceProvider, applicationLifetime, configBotSection)
        {

        }
        #pragma warning restore 8618

        protected override Task ConfigureAsync()
        {
            try
            {
                var config = this.Configuration.ExtractConfig<DiscordConfiguration>(this.ServiceProvider, "Discord", this.BotSection);
                this.ShardedClient = new DiscordShardedClient(config);
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Was unable to build {nameof(DiscordShardedClient)} for {this.GetType().Name}");
                this.OnInitializationError(ex);
            }

            return Task.CompletedTask;
        }

        protected sealed override async Task ConnectAsync() => await this.ShardedClient.StartAsync();

        protected override Task ConfigureExtensionsAsync()
        {
            foreach (var client in this.ShardedClient.ShardClients.Values)
            {
                this.InitializeExtensions(client);
            }

            return Task.CompletedTask;
        }
    }
}
