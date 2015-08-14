using System;
using System.Threading.Tasks;

using DisCatSharp.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisCatSharp.Hosting;

/// <summary>
/// Simple implementation for <see cref="DiscordClient"/> to work as a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/>
/// </summary>
public abstract class DiscordHostedService : BaseHostedService, IDiscordHostedService
{
	/// <inheritdoc/>
	public DiscordClient Client { get; protected set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordHostedService"/> class.
	/// </summary>
	/// <param name="config">IConfiguration provided via Dependency Injection. Aggregate method to access configuration files </param>
	/// <param name="logger">An ILogger to work with, provided via Dependency Injection</param>
	/// <param name="serviceProvider">ServiceProvider reference which contains all items currently registered for Dependency Injection</param>
	/// <param name="applicationLifetime">Contains the appropriate methods for disposing / stopping BackgroundServices during runtime</param>
	/// <param name="configBotSection">The name of the JSON/Config Key which contains the configuration for this Discord Service</param>
	protected DiscordHostedService(
		IConfiguration config,
		ILogger<DiscordHostedService> logger,
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
