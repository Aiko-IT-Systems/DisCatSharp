using System;
using System.Collections.Generic;

using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Lavalink;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class Bot : DiscordHostedService
{
	public Bot(IConfiguration config, ILogger<Bot> logger, IServiceProvider provider, IHostApplicationLifetime lifetime)
		: base(config, logger, provider, lifetime)
	{
		this.ConfigureAsync().GetAwaiter().GetResult();
		this.ConfigureExtensionsAsync().GetAwaiter().GetResult();
	}
}

public sealed class MyCustomBot : DiscordHostedService
{
	public MyCustomBot(IConfiguration config, ILogger<MyCustomBot> logger, IServiceProvider provider, IHostApplicationLifetime lifetime)
		: base(config, logger, provider, lifetime, "MyCustomBot")
	{
		this.ConfigureAsync().GetAwaiter().GetResult();
		this.ConfigureExtensionsAsync().GetAwaiter().GetResult();
	}
}

public interface IBotTwoService : IDiscordHostedService
{
	string GiveMeAResponse();
}

public sealed class BotTwoService : DiscordHostedService, IBotTwoService
{
	public BotTwoService(IConfiguration config, ILogger<BotTwoService> logger, IServiceProvider provider, IHostApplicationLifetime lifetime)
		: base(config, logger, provider, lifetime, "BotTwo")
	{
		this.ConfigureAsync().GetAwaiter().GetResult();
		this.ConfigureExtensionsAsync().GetAwaiter().GetResult();
	}

	public string GiveMeAResponse() => "I'm working";
}

public class HostTests
{
	private Dictionary<string, string?> DefaultDiscord() =>
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

	public Dictionary<string, string?> DiscordInteractivity() => new(this.DefaultDiscord())
	{
		{ "DisCatSharp:Using", "[\"DisCatSharp.Interactivity\"]" }
	};

	public Dictionary<string, string?> DiscordInteractivityAndLavalink() => new(this.DefaultDiscord())
	{
		{ "DisCatSharp:Using", "[\"DisCatSharp.Interactivity\", \"DisCatSharp.Lavalink\"]" }
	};

	private IHostBuilder Create(Dictionary<string, string?> configValues) =>
		Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddSingleton<IDiscordHostedService, Bot>())
			.ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(configValues));

	private IHostBuilder Create(string filename) =>
		Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddSingleton<IDiscordHostedService, MyCustomBot>())
			.ConfigureHostConfiguration(builder => builder.AddJsonFile(filename));

	private IHostBuilder Create<TInterface, TBot>(string filename)
		where TInterface : class, IDiscordHostedService
		where TBot : class, TInterface, IDiscordHostedService =>
		Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddSingleton<TInterface, TBot>())
			.ConfigureHostConfiguration(builder => builder.AddJsonFile(filename));

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
