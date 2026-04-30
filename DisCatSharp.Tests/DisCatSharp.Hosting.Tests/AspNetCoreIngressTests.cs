using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class AspNetCoreIngressTests
{
	[Fact]
	public void AddDisCatSharpAspNetCore_RegistersIngressCoreServices()
	{
		ServiceCollection services = [];
		services.AddDisCatSharpAspNetCore(
			options =>
			{
				options.MaxRequestBodySize = 1024;
				options.PendingStateLifetime = TimeSpan.FromMinutes(2);
				options.PendingStateCleanupInterval = TimeSpan.FromSeconds(30);
			},
			options =>
			{
				options.RoutePrefix = "/discord-api";
				options.OAuthPath = "oauth2";
				options.OAuthCallbackPath = "complete";
				options.InteractionsPath = "gateway";
				options.WebhooksPath = "hooks";
				options.WebhookEventsPath = "events";
				options.IncomingWebhooksPath = "incoming";
			});

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<DiscordWebIngressOptions>>().Value;
		var aspNetOptions = provider.GetRequiredService<IOptions<DiscordAspNetCoreIngressOptions>>().Value;

		Assert.Equal(1024, options.MaxRequestBodySize);
		Assert.Equal(TimeSpan.FromMinutes(2), options.PendingStateLifetime);
		Assert.Equal(TimeSpan.FromSeconds(30), options.PendingStateCleanupInterval);
		Assert.Equal("/discord-api", aspNetOptions.RoutePrefix);
		Assert.Equal("oauth2", aspNetOptions.OAuthPath);
		Assert.Equal("complete", aspNetOptions.OAuthCallbackPath);
		Assert.Equal("gateway", aspNetOptions.InteractionsPath);
		Assert.Equal("hooks", aspNetOptions.WebhooksPath);
		Assert.Equal("events", aspNetOptions.WebhookEventsPath);
		Assert.Equal("incoming", aspNetOptions.IncomingWebhooksPath);
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressBodyReader>());
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressPendingStateStore>());
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressSignatureValidationService>());
		Assert.NotNull(provider.GetRequiredService<TimeProvider>());
	}

	[Fact]
	public void MapDisCatSharpIngress_MapsStablePlaceholderEndpoints()
	{
		using var app = BuildApp(options =>
		{
			options.RoutePrefix = "/discord-api";
			options.OAuthPath = "oauth2";
			options.OAuthCallbackPath = "complete";
			options.InteractionsPath = "gateway";
			options.WebhooksPath = "hooks";
			options.WebhookEventsPath = "events";
			options.IncomingWebhooksPath = "incoming";
		});

		var group = app.MapDisCatSharpIngress();
		var endpoints = GetRouteEndpoints(app);

		Assert.NotNull(group);
		AssertEndpoint(endpoints, "/discord-api/oauth2/complete", DiscordIngressEndpointNames.OAuthCallback, "OAuth", "oauth2/complete");
		AssertEndpoint(endpoints, "/discord-api/gateway", DiscordIngressEndpointNames.Interactions, "Interactions", "gateway");
		AssertEndpoint(endpoints, "/discord-api/hooks/events", DiscordIngressEndpointNames.WebhookEvents, "Webhooks", "hooks/events");
		AssertEndpoint(endpoints, "/discord-api/hooks/incoming", DiscordIngressEndpointNames.IncomingWebhooks, "IncomingWebhooks", "hooks/incoming");
	}

	[Fact]
	public void MapDiscordIngressModules_CanBeNestedUnderCustomRouteGroups()
	{
		using var app = BuildApp();

		var group = app.MapGroup("/api/discord");
		group.MapDiscordOAuthIngress();
		group.MapDiscordInteractionIngress();
		group.MapDiscordWebhookIngress();

		var endpoints = GetRouteEndpoints(app);

		AssertEndpoint(endpoints, "/api/discord/oauth/callback", DiscordIngressEndpointNames.OAuthCallback, "OAuth", "oauth/callback");
		AssertEndpoint(endpoints, "/api/discord/interactions", DiscordIngressEndpointNames.Interactions, "Interactions", "interactions");
		AssertEndpoint(endpoints, "/api/discord/webhooks/events", DiscordIngressEndpointNames.WebhookEvents, "Webhooks", "webhooks/events");
		AssertEndpoint(endpoints, "/api/discord/webhooks/incoming", DiscordIngressEndpointNames.IncomingWebhooks, "IncomingWebhooks", "webhooks/incoming");
	}

	[Fact]
	public async Task PendingStateStore_StoresAndConsumesEntries()
	{
		var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 03, 26, 12, 00, 00, TimeSpan.Zero));
		using var provider = BuildProvider(timeProvider, options =>
		{
			options.PendingStateLifetime = TimeSpan.FromMinutes(1);
			options.PendingStateCleanupInterval = TimeSpan.FromSeconds(30);
		});

		var store = provider.GetRequiredService<IDiscordIngressPendingStateStore>();
		var requestUri = new Uri("https://discord.com/oauth2/authorize?state=pending-state");

		await store.StoreAsync(new DiscordIngressPendingState
		{
			Key = "pending-state",
			Flow = "oauth",
			RequestUri = requestUri,
			Properties = new Dictionary<string, string?> { ["scope"] = "identify" }
		});

		var stored = await store.GetAsync("pending-state");
		var consumed = await store.ConsumeAsync("pending-state");

		Assert.NotNull(stored);
		Assert.Equal(requestUri, stored.RequestUri);
		Assert.Equal(timeProvider.GetUtcNow().AddMinutes(1), stored.ExpiresAt);
		Assert.Equal("identify", stored.Properties["scope"]);
		Assert.NotNull(consumed);
		Assert.Null(await store.GetAsync("pending-state"));
	}

	[Fact]
	public async Task PendingStateStore_ExpiresEntries()
	{
		var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 03, 26, 12, 00, 00, TimeSpan.Zero));
		using var provider = BuildProvider(timeProvider, options =>
		{
			options.PendingStateLifetime = TimeSpan.FromSeconds(5);
			options.PendingStateCleanupInterval = TimeSpan.FromSeconds(1);
		});

		var store = provider.GetRequiredService<IDiscordIngressPendingStateStore>();
		await store.StoreAsync(new DiscordIngressPendingState { Key = "expired-state" });

		timeProvider.Advance(TimeSpan.FromSeconds(10));

		Assert.Null(await store.GetAsync("expired-state"));
	}

	[Fact]
	public async Task SignatureValidationService_ReturnsFirstSuccessfulResult()
	{
		ServiceCollection services = [];
		services.AddDisCatSharpAspNetCore();
		services.AddDiscordIngressSignatureValidator<NeverMatchesSignatureValidator>();
		services.AddDiscordIngressSignatureValidator<InvalidSignatureValidator>();
		services.AddDiscordIngressSignatureValidator<ValidSignatureValidator>();

		using var provider = services.BuildServiceProvider();
		var service = provider.GetRequiredService<IDiscordIngressSignatureValidationService>();
		var request = new DiscordIngressRequest("POST", new Uri("https://example.com/interactions"), body: DiscordIngressPayload.FromString("{}"));

		var result = await service.ValidateAsync(request);

		Assert.Equal(DiscordIngressSignatureValidationStatus.Valid, result.Status);
		Assert.Equal("valid-test", result.ValidatorName);
	}

	[Fact]
	public async Task BodyReader_RejectsBodiesThatExceedTheConfiguredLimit()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.MaxRequestBodySize = 4);
		var reader = provider.GetRequiredService<IDiscordIngressBodyReader>();
		using MemoryStream body = new([1, 2, 3, 4, 5]);

		await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(body).AsTask());
	}

	private static ServiceProvider BuildProvider(TimeProvider timeProvider, Action<DiscordWebIngressOptions>? configure = null)
	{
		ServiceCollection services = [];
		services.AddSingleton<TimeProvider>(timeProvider);
		services.AddDisCatSharpAspNetCore(configure);
		return services.BuildServiceProvider();
	}

	private static WebApplication BuildApp(Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null)
	{
		var builder = WebApplication.CreateBuilder();
		builder.Services.AddDisCatSharpAspNetCore(configureAspNetCore: configureAspNetCore);
		return builder.Build();
	}

	private static IReadOnlyList<RouteEndpoint> GetRouteEndpoints(IEndpointRouteBuilder endpoints)
		=> endpoints.DataSources
			.SelectMany(static source => source.Endpoints)
			.OfType<RouteEndpoint>()
			.ToArray();

	private static void AssertEndpoint(
		IEnumerable<RouteEndpoint> endpoints,
		string pattern,
		string endpointName,
		string module,
		string relativePath
	)
	{
		var normalizedPattern = NormalizePattern(pattern);
		var endpoint = Assert.Single(endpoints, endpoint => string.Equals(NormalizePattern(endpoint.RoutePattern.RawText), normalizedPattern, StringComparison.Ordinal));
		var metadata = Assert.IsType<DiscordIngressEndpointMetadata>(endpoint.Metadata.GetMetadata<DiscordIngressEndpointMetadata>());

		Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
		Assert.Equal(module, metadata.Module);
		Assert.Equal(endpointName, metadata.EndpointName);
		Assert.Equal(relativePath, metadata.RelativePath);
		Assert.Contains(StatusCodes.Status501NotImplemented, endpoint.Metadata
			.OfType<IProducesResponseTypeMetadata>()
			.Select(static metadata => metadata.StatusCode));
	}

	private static string NormalizePattern(string? pattern)
	{
		if (string.IsNullOrWhiteSpace(pattern))
			return "/";

		return $"/{pattern.Trim().Trim('/')}";
	}

	private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
	{
		private DateTimeOffset _utcNow = utcNow;

		public override DateTimeOffset GetUtcNow() => this._utcNow;

		public void Advance(TimeSpan delta) => this._utcNow = this._utcNow.Add(delta);
	}

	private sealed class NeverMatchesSignatureValidator : IDiscordIngressSignatureValidator
	{
		public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, System.Threading.CancellationToken cancellationToken = default)
			=> new(DiscordIngressSignatureValidationResult.NotValidated("no-match"));
	}

	private sealed class InvalidSignatureValidator : IDiscordIngressSignatureValidator
	{
		public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, System.Threading.CancellationToken cancellationToken = default)
			=> new(DiscordIngressSignatureValidationResult.Invalid("invalid-test", "Signature mismatch"));
	}

	private sealed class ValidSignatureValidator : IDiscordIngressSignatureValidator
	{
		public ValueTask<DiscordIngressSignatureValidationResult> ValidateAsync(DiscordIngressRequest request, System.Threading.CancellationToken cancellationToken = default)
			=> new(DiscordIngressSignatureValidationResult.Valid("valid-test"));
	}
}
