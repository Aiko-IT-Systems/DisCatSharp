using System;
using System.Threading.Tasks;

using DisCatSharp.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting;

/// <summary>
/// Simple Implementation for <see cref="DiscordShardedClient"/> to work as a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/>
/// </summary>
public abstract class DiscordShardedHostedService : BaseHostedService, IDiscordHostedShardService
{
	public DiscordShardedClient ShardedClient { get; protected set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordShardedHostedService"/> class.
	/// </summary>
	/// <param name="config">The config.</param>
	/// <param name="logger">The logger.</param>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="applicationLifetime">The application lifetime.</param>
	/// <param name="configBotSection">The config bot section.</param>
	protected DiscordShardedHostedService(
		IConfiguration config,
		ILogger<DiscordShardedHostedService> logger,
		IServiceProvider serviceProvider,
		IHostApplicationLifetime applicationLifetime,
		string configBotSection = DisCatSharp.Configuration.ConfigurationExtensions.DEFAULT_ROOT_LIB
	)
		: base(config, logger, serviceProvider, applicationLifetime, configBotSection)
	{ }

	protected override Task ConfigureAsync()
	{
		try
		{
			var config = this.Configuration.ExtractConfig<DiscordConfiguration>(this.ServiceProvider, "Discord", this.BotSection);
			this.ShardedClient = new(config);
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
			this.InitializeExtensions(client);

		return Task.CompletedTask;
	}
}
