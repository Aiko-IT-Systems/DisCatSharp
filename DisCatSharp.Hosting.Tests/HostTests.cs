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
    public sealed class Bot : DiscordHostedService
    {
        public Bot(IConfiguration Config, ILogger<Bot> Logger, IServiceProvider Provider, IHostApplicationLifetime Lifetime) : base(Config, Logger, Provider, Lifetime)
        {
            this.Configure().GetAwaiter().GetResult();
            this.ConfigureExtensions().GetAwaiter().GetResult();
        }
    }

    public sealed class MyCustomBot : DiscordHostedService
    {
        public MyCustomBot(IConfiguration Config, ILogger<MyCustomBot> Logger, IServiceProvider Provider, IHostApplicationLifetime Lifetime) : base(Config, Logger, Provider, Lifetime, "MyCustomBot")
        {
            this.Configure().GetAwaiter().GetResult();
            this.ConfigureExtensions().GetAwaiter().GetResult();
        }
    }

    public interface IBotTwoService : IDiscordHostedService
    {
        string GiveMeAResponse();
    }


    public sealed class BotTwoService : DiscordHostedService, IBotTwoService
    {
        public BotTwoService(IConfiguration Config, ILogger<BotTwoService> Logger, IServiceProvider Provider, IHostApplicationLifetime Lifetime) : base(Config, Logger, Provider, Lifetime, "BotTwo")
        {
            this.Configure().GetAwaiter().GetResult();
            this.ConfigureExtensions().GetAwaiter().GetResult();
        }

        public string GiveMeAResponse() => "I'm working";
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

        public Dictionary<string, string> DiscordInteractivity() => new(this.DefaultDiscord())
        {
            { "DisCatSharp:Using", "[\"DisCatSharp.Interactivity\"]" },
        };

        public Dictionary<string, string> DiscordInteractivityAndLavalink() => new(this.DefaultDiscord())
        {
            { "DisCatSharp:Using", "[\"DisCatSharp.Interactivity\", \"DisCatSharp.Lavalink\"]" },
        };

        IHostBuilder Create(Dictionary<string, string> ConfigValues) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(Services => Services.AddSingleton<IDiscordHostedService, Bot>())
                .ConfigureHostConfiguration(Builder => Builder.AddInMemoryCollection(ConfigValues));

        IHostBuilder Create(string Filename) =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(Services => Services.AddSingleton<IDiscordHostedService, MyCustomBot>())
                .ConfigureHostConfiguration(Builder => Builder.AddJsonFile(Filename));

        IHostBuilder Create<TInterface, TBot>(string Filename)
            where TInterface : class, IDiscordHostedService
            where TBot : class, TInterface, IDiscordHostedService =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(Services => Services.AddSingleton<TInterface, TBot>())
                .ConfigureHostConfiguration(Builder => Builder.AddJsonFile(Filename));


        [Fact]
        public void TestBotCustomInterface()
        {
            IHost? host = null;

            try
            {
                host = this.Create<IBotTwoService, BotTwoService>("BotTwo.json").Build();
                var service = host.Services.GetRequiredService<IBotTwoService>();

                Assert.NotNull(service);

                var response = service.GiveMeAResponse();
                Assert.Equal("I'm working", response);
            }
            finally
            {
                host?.Dispose();
            }
        }

        [Fact]
        public void TestDifferentSection_InteractivityOnly()
        {
            IHost? host = null;

            try
            {
                host = this.Create("interactivity-different-section.json").Build();
                var service = host.Services.GetRequiredService<IDiscordHostedService>();

                Assert.NotNull(service);
                Assert.NotNull(service.Client);
                Assert.Null(service.Client.GetExtension<LavalinkExtension>());

                var intents = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
                              DiscordIntents.Guilds;
                Assert.Equal(intents, service.Client.Intents);


                var interactivity = service.Client.GetExtension<InteractivityExtension>();
                Assert.NotNull(interactivity);

                Assert.NotNull(host.Services);
                Assert.NotNull(service.Client.ServiceProvider);
            }
            finally
            {
                host?.Dispose();
            }
        }

        [Fact]
        public void TestDifferentSection_LavalinkOnly()
        {
            IHost? host = null;

            try
            {
                host = this.Create("lavalink-different-section.json").Build();
                var service = host.Services.GetRequiredService<IDiscordHostedService>();

                Assert.NotNull(service);
                Assert.NotNull(service.Client);
                Assert.NotNull(service.Client.GetExtension<LavalinkExtension>());
                Assert.Null(service.Client.GetExtension<InteractivityExtension>());

                var intents = DiscordIntents.Guilds;
                Assert.Equal(intents, service.Client.Intents);
                Assert.NotNull(service.Client.ServiceProvider);
            }
            finally
            {
                host?.Dispose();
            }
        }

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
                Assert.NotNull(service.Client.ServiceProvider);
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
                Assert.NotNull(service.Client.ServiceProvider);
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
                Assert.NotNull(service.Client.ServiceProvider);
            }
            finally
            {
                host?.Dispose();
            }
        }
    }
}
