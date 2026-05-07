using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore;

internal sealed class DiscordAspNetCoreSelfHostService(
	IServiceProvider serviceProvider,
	IOptions<DiscordAspNetCoreSelfHostOptions> selfHostOptions,
	DiscordAspNetCoreSelfHostRuntime runtime,
	ILogger<DiscordAspNetCoreSelfHostService> logger
	) : IHostedService, IAsyncDisposable
{
	private const string StartingLogMessage = "Starting DisCatSharp self-hosted ingress on {ListenBaseUrl}.";
	private const string StartedLogMessage = "Started DisCatSharp self-hosted ingress on {ListenBaseUrl} with public base URL {PublicBaseUrl}.";
	private const string StoppingLogMessage = "Stopping DisCatSharp self-hosted ingress.";
	private const string StoppedLogMessage = "Stopped DisCatSharp self-hosted ingress.";

	private readonly ILogger<DiscordAspNetCoreSelfHostService> _logger = logger;
	private readonly DiscordAspNetCoreSelfHostRuntime _runtime = runtime;
	private readonly IOptions<DiscordAspNetCoreSelfHostOptions> _selfHostOptions = selfHostOptions;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private WebApplication? _application;

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		if (this._application is not null)
			return;

		var options = this._selfHostOptions.Value;
		var configuredListenBaseUrl = BuildListenBaseUrl(options);

		this._logger.LogInformation(StartingLogMessage, configuredListenBaseUrl);

		var builder = WebApplication.CreateBuilder([]);
		builder.Logging.ClearProviders();
		builder.WebHost.UseUrls(configuredListenBaseUrl.ToString());

		this.RegisterBridgedServices(builder.Services);

		var application = builder.Build();
		application.MapDisCatSharpIngress();

		await application.StartAsync(cancellationToken);

		var listenBaseUrl = ResolveListenBaseUrl(application, configuredListenBaseUrl);
		var publicBaseUrl = ResolvePublicBaseUrl(options, listenBaseUrl);

		this._runtime.SetRunning(listenBaseUrl, publicBaseUrl);
		this._application = application;

		this._logger.LogInformation(StartedLogMessage, listenBaseUrl, publicBaseUrl);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (this._application is null)
		{
			this._runtime.Reset();
			return;
		}

		this._logger.LogInformation(StoppingLogMessage);

		var application = this._application;
		this._application = null;
		try
		{
			await application.StopAsync(cancellationToken);
			await application.DisposeAsync();
		}
		finally
		{
			this._runtime.Reset();
		}

		this._logger.LogInformation(StoppedLogMessage);
	}

	public async ValueTask DisposeAsync()
	{
		if (this._application is null)
			return;

		await this.StopAsync(CancellationToken.None);
	}

	private void RegisterBridgedServices(IServiceCollection services)
	{
		services.AddRouting();
		services.AddSingleton(this._serviceProvider.GetRequiredService<IOptions<DiscordWebIngressOptions>>());
		services.AddSingleton(this._serviceProvider.GetRequiredService<IOptions<DiscordAspNetCoreIngressOptions>>());
		services.AddSingleton(this._serviceProvider.GetRequiredService<IOptions<DiscordOAuthIngressOptions>>());
		services.AddSingleton(this._serviceProvider.GetRequiredService<TimeProvider>());
		services.AddSingleton(this._serviceProvider.GetRequiredService<IDiscordIngressBodyReader>());
		services.AddSingleton(this._serviceProvider.GetRequiredService<IDiscordIngressPendingStateStore>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<DiscordWebhookEventIngressService>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<DiscordWebhookEventEndpointHandler>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<DiscordIncomingWebhookIngressService>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<DiscordIncomingWebhookEndpointHandler>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<IDiscordOAuthTokenExchangeService>());
		services.AddTransient(_ => this._serviceProvider.GetRequiredService<IDiscordOAuthCallbackHandler>());
		services.AddSingleton(_ => this._serviceProvider.GetRequiredService<IDiscordOAuthCallbackResponseFactory>());
		services.AddSingleton(this._runtime);

		foreach (var validator in this._serviceProvider.GetServices<IDiscordIngressSignatureValidator>())
			services.AddSingleton(validator);

		services.AddTransient(_ => this._serviceProvider.GetRequiredService<IDiscordIngressSignatureValidationService>());
	}

	private static Uri BuildListenBaseUrl(DiscordAspNetCoreSelfHostOptions options)
		=> NormalizeBaseUrl(new UriBuilder(options.Scheme.Trim(), options.ListenAddress.Trim(), options.ListenPort).Uri);

	private static Uri ResolveListenBaseUrl(WebApplication application, Uri fallbackListenBaseUrl)
	{
		var addresses = application.Services.GetService<IServer>()?
			.Features
			.Get<IServerAddressesFeature>()?
			.Addresses;

		var address = addresses?.FirstOrDefault();
		return address is not null && Uri.TryCreate(address, UriKind.Absolute, out var listenBaseUrl)
			? NormalizeBaseUrl(listenBaseUrl)
			: fallbackListenBaseUrl;
	}

	private static Uri ResolvePublicBaseUrl(DiscordAspNetCoreSelfHostOptions options, Uri listenBaseUrl)
		=> options.BaseUrl is null
			? listenBaseUrl
			: NormalizeBaseUrl(options.BaseUrl);

	private static Uri NormalizeBaseUrl(Uri uri)
	{
		var builder = new UriBuilder(uri)
		{
			Query = string.Empty,
			Fragment = string.Empty
		};

		return builder.Uri;
	}
}
