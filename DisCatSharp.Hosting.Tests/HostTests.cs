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
using DisCatSharp.Interactivity;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DisCatSharp.Hosting.Tests
{
    public class Bot : DiscordHostedService
    {
        public Bot(IConfiguration config, ILogger<DiscordHostedService> logger, IServiceProvider provider) : base(config, logger, provider)
        {
        }
    }

    public class HostTests
    {
        private Dictionary<string, string> DefaultDiscord() =>
            new()
            {
                { "DisCatSharp:Discord:Token", "1234567890" },
                { "DisCatSharp:Discord:TokenType", "Bot" },
                { "DisCatSharp:Discord:MinimumLogLevel", "Information" },
                { "DisCatSharp:Discord:UseRelativeRateLimit", "true" },
                { "DisCatSharp:Discord:LogTimestampFormat", "yyyy-MM-dd HH:mm:ss zzz" },
                { "DisCatSharp:Discord:LargeThreshold", "250" },
                { "DisCatSharp:Discord:AutoReconnect", "true" },
                { "DisCatSharp:Discord:ShardId", "123123" },
                { "DisCatSharp:Discord:GatewayCompressionLevel", "Stream" },
                { "DisCatSharp:Discord:MessageCacheSize", "1024" },
                { "DisCatSharp:Discord:HttpTimeout", "00:00:20" },
                { "DisCatSharp:Discord:ReconnectIndefinitely", "false" },
                { "DisCatSharp:Discord:AlwaysCacheMembers", "true" },
                { "DisCatSharp:Discord:DiscordIntents", "AllUnprivileged" },
                { "DisCatSharp:Discord:MobileStatus", "false" },
                { "DisCatSharp:Discord:UseCanary", "false" },
                { "DisCatSharp:Discord:AutoRefreshChannelCache", "false" },
                { "DisCatSharp:Discord:Intents", "AllUnprivileged" }
            };

        public Dictionary<string, string> DiscordInteractivity() => new (this.DefaultDiscord())
        {
            {"DisCatSharp:Using","[\"DisCatSharp.Interactivity\"]"},
        };

        public Dictionary<string, string> DiscordInteractivityAndLavalink() => new (this.DefaultDiscord())
        {
            {"DisCatSharp:Using","[\"DisCatSharp.Interactivity\", \"DisCatSharp.Lavalink\"]"},
        };

        IHostBuilder Create(Dictionary<string, string> configValues) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddSingleton<IDiscordHostedService, Bot>())
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(configValues));

        [Fact]
        public void TestNoExtensions()
        {
            IHost? host = null;

            try
            {
                host = this.Create(this.DefaultDiscord()).Build();

                var service = host.Services.GetRequiredService<IDiscordHostedService>();
                Assert.NotNull(service);
                Assert.NotNull(service.Client);
            }
            finally
            {
                host?.Dispose();
            }
        }

        [Fact]
        public void TestInteractivityExtension()
        {
            IHost? host = null;

            try
            {
                host = this.Create(this.DiscordInteractivity()).Build();

                var service = host.Services.GetRequiredService<IDiscordHostedService>();
                Assert.NotNull(service);
                Assert.NotNull(service.Client);
                Assert.NotNull(service.Client.GetExtension<InteractivityExtension>());
            }
            finally
            {
                host?.Dispose();
            }
        }

        [Fact]
        public void TestInteractivityLavalinkExtensions()
        {
            IHost? host = null;

            try
            {
                host = this.Create(this.DiscordInteractivityAndLavalink()).Build();

                var service = host.Services.GetRequiredService<IDiscordHostedService>();

                Assert.NotNull(service);
                Assert.NotNull(service.Client);
                Assert.NotNull(service.Client.GetExtension<InteractivityExtension>());
                Assert.NotNull(service.Client.GetExtension<LavalinkExtension>());
            }
            finally
            {
                host?.Dispose();
            }
        }
    }
}
