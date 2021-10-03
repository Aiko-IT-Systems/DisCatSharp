using System;
using System.Collections.Generic;
using DisCatSharp.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DisCatSharp.Configuration.Tests
{
    public class ConfigurationExtensionTests
    {
        private IConfiguration BasicDiscordConfiguration() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"DisCatSharp:Discord:Token", "1234567890"},
                {"DisCatSharp:Discord:TokenType", "Bot" },
                {"DisCatSharp:Discord:MinimumLogLevel", "Information"},
                {"DisCatSharp:Discord:UseRelativeRateLimit", "true"},
                {"DisCatSharp:Discord:LogTimestampFormat", "yyyy-MM-dd HH:mm:ss zzz"},
                {"DisCatSharp:Discord:LargeThreshold", "250"},
                {"DisCatSharp:Discord:AutoReconnect", "true"},
                {"DisCatSharp:Discord:ShardId", "123123"},
                {"DisCatSharp:Discord:GatewayCompressionLevel", "Stream"},
                {"DisCatSharp:Discord:MessageCacheSize", "1024"},
                {"DisCatSharp:Discord:HttpTimeout", "00:00:20"},
                {"DisCatSharp:Discord:ReconnectIndefinitely", "false"},
                {"DisCatSharp:Discord:AlwaysCacheMembers", "true" },
                {"DisCatSharp:Discord:DiscordIntents", "AllUnprivileged"},
                {"DisCatSharp:Discord:MobileStatus", "false"},
                {"DisCatSharp:Discord:UseCanary", "false"},
                {"DisCatSharp:Discord:AutoRefreshChannelCache", "false"},
                {"DisCatSharp:Discord:Intents", "AllUnprivileged"}
            })
            .Build();

        private IConfiguration DiscordIntentsConfig() => new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"DisCatSharp:Discord:Intents", "GuildEmojisAndStickers,GuildMembers,GuildInvites,GuildMessageReactions"}
                })
                .Build();

        private IConfiguration DiscordHaphazardConfig() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "DisCatSharp:Discord:Intents", "GuildEmojisAndStickers,GuildMembers,Guilds" },
                { "DisCatSharp:Discord:MobileStatus", "true" },
                { "DisCatSharp:Discord:LargeThreshold", "1000" },
                { "DisCatSharp:Discord:HttpTimeout", "10:00:00" }
            })
            .Build();

        private IConfiguration SampleConfig() => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Sample:Amount", "200" },
                { "Sample:Email", "test@gmail.com" }
            })
            .Build();

        [Fact]
        public void TestExtractDiscordConfig_Intents()
        {
            var source = this.DiscordIntentsConfig();

            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");

            var expected = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
                           DiscordIntents.GuildInvites | DiscordIntents.GuildMessageReactions;

            Assert.Equal(expected, config.Intents);
        }

        [Fact]
        public void TestExtractDiscordConfig_Haphzard()
        {
            var source = this.DiscordHaphazardConfig();

            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");
            var expectedIntents = DiscordIntents.GuildEmojisAndStickers | DiscordIntents.GuildMembers |
                                  DiscordIntents.Guilds;

            Assert.Equal(expectedIntents, config.Intents);
            Assert.True(config.MobileStatus);
            Assert.Equal(1000, config.LargeThreshold);

            var expectedTimeout = TimeSpan.FromHours(10);
            Assert.Equal(expectedTimeout, config.HttpTimeout);
        }

        [Fact]
        public void TestExtractDiscordConfig_Default()
        {
            var source = this.BasicDiscordConfiguration();
            DiscordConfiguration config = source.ExtractConfig<DiscordConfiguration>("Discord");

            Assert.Equal("1234567890", config.Token);
            Assert.Equal(TokenType.Bot, config.TokenType);
            Assert.Equal(LogLevel.Information, config.MinimumLogLevel);
            Assert.True(config.UseRelativeRatelimit);
            Assert.Equal("yyyy-MM-dd HH:mm:ss zzz", config.LogTimestampFormat);
            Assert.Equal(250, config.LargeThreshold);
            Assert.True(config.AutoReconnect);
            Assert.Equal(123123, config.ShardId);
            Assert.Equal(GatewayCompressionLevel.Stream, config.GatewayCompressionLevel);
            Assert.Equal(1024, config.MessageCacheSize);

            TimeSpan timeout = TimeSpan.FromSeconds(20);
            Assert.Equal(timeout, config.HttpTimeout);
            Assert.False(config.ReconnectIndefinitely);
            Assert.True(config.AlwaysCacheMembers);
            Assert.Equal(DiscordIntents.AllUnprivileged, config.Intents);
            Assert.False(config.MobileStatus);
            Assert.False(config.UseCanary);
            Assert.False(config.AutoRefreshChannelCache);
        }

        class SampleClass
        {
            public int Amount { get; set; }
            public string Email { get; set; }
        }

        [Fact]
        public void TestSection()
        {
            var source = this.SampleConfig();
            SampleClass config = source.ExtractConfig<SampleClass>("Sample", null);

            Assert.Equal(200, config.Amount);
            Assert.Equal("test@gmail.com", config.Email);
        }
    }
}

