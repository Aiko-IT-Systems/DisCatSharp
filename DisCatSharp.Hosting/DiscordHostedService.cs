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
    /// Simple implementation for <see cref="DiscordClient"/> to work as a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/>
    /// </summary>
    public abstract class DiscordHostedService : BaseHostedService, IDiscordHostedService
    {
        /// <inheritdoc/>
        public DiscordClient Client { get; protected set; }

        #pragma warning disable 8618
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordHostedService"/> class.
        /// </summary>
        /// <param name="config">IConfiguration provided via Dependency Injection. Aggregate method to access configuration files </param>
        /// <param name="logger">An ILogger to work with, provided via Dependency Injection</param>
        /// <param name="serviceProvider">ServiceProvider reference which contains all items currently registered for Dependency Injection</param>
        /// <param name="applicationLifetime">Contains the appropriate methods for disposing / stopping BackgroundServices during runtime</param>
        /// <param name="configBotSection">The name of the JSON/Config Key which contains the configuration for this Discord Service</param>
        protected DiscordHostedService(IConfiguration config,
            ILogger<DiscordHostedService> logger,
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
                this.Client = this.Configuration.BuildClient(this.ServiceProvider, this.BotSection);
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Was unable to build {nameof(DiscordClient)} for {this.GetType().Name}");
                this.OnInitializationError(ex);
            }

            return Task.CompletedTask;
        }
        protected sealed override async Task ConnectAsync() => await this.Client.ConnectAsync();

        protected override Task ConfigureExtensionsAsync()
        {
            this.InitializeExtensions(this.Client);
            return Task.CompletedTask;
        }
    }
}
