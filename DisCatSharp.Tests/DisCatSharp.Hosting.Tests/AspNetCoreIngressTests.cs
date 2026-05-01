using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;
using DisCatSharp.Hosting.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class AspNetCoreIngressTests
{
	private const string TestApplicationVerifyKey = "976b18761c2ba7bf877058100b7afd8bc8b822cfb82ed16bdb5f80c7a86909d7";
	private const string TestPingTimestamp = "1710000000";
	private const string TestPingBody = "{\"type\":0}";
	private const string TestPingSignature = "39529f764bfc129887ae3fb168d5dbacaaa97235331c835eb5fae3910586ef31dab73edd8b280e65c1be6404a6188cfb98107e57fc452c5a9347a27fccbd1500";
	private const string TestEventTimestamp = "1710000001";
	private const string TestEventBody = "{\"type\":42,\"application_id\":\"1234567890\",\"event\":{\"type\":\"MESSAGE_CREATE\"}}";
	private const string TestEventSignature = "f6393a5e602be84d093cb9b55e5b3ce70b8133b05cf083df913864f70397cd26e9def0050d6db63575736c33e1930c10a12b3fc8a30218de3a3795eb818de20a";
	private const string TestMalformedTimestamp = "1710000002";
	private const string TestMalformedBody = "{";
	private const string TestMalformedSignature = "434d91d1796bb1f88db74d3dbaa5373535fec991a51888abf946eaf24bc9e2290dfc00f17f4f7dbb87b3e94c423327ffdfe5db2bc70e6206dd1d2e1d31eab60a";

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
			},
			configureOAuth: options =>
			{
				options.ClientId = 734829134102410240;
				options.ClientSecret = "super-secret";
				options.RedirectUri = "https://example.com/discord/oauth/complete";
			});

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<DiscordWebIngressOptions>>().Value;
		var aspNetOptions = provider.GetRequiredService<IOptions<DiscordAspNetCoreIngressOptions>>().Value;
		var oauthOptions = provider.GetRequiredService<IOptions<DiscordOAuthIngressOptions>>().Value;

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
		Assert.Equal((ulong)734829134102410240, oauthOptions.ClientId);
		Assert.Equal("super-secret", oauthOptions.ClientSecret);
		Assert.Equal("https://example.com/discord/oauth/complete", oauthOptions.RedirectUri);
		Assert.True(oauthOptions.IsConfigured);
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressBodyReader>());
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressPendingStateStore>());
		Assert.NotNull(provider.GetRequiredService<IDiscordIngressSignatureValidationService>());
		Assert.Contains(provider.GetServices<IDiscordIngressSignatureValidator>(), static validator => validator is DiscordEd25519IngressSignatureValidator);
		Assert.NotNull(provider.GetRequiredService<DiscordWebhookEventIngressService>());
		Assert.NotNull(provider.GetRequiredService<DiscordWebhookEventEndpointHandler>());
		Assert.NotNull(provider.GetRequiredService<IDiscordOAuthTokenExchangeService>());
		Assert.NotNull(provider.GetRequiredService<IDiscordOAuthCallbackHandler>());
		Assert.NotNull(provider.GetRequiredService<IDiscordOAuthCallbackResponseFactory>());
		Assert.NotNull(provider.GetRequiredService<TimeProvider>());
	}

	[Fact]
	public void HostApplicationBuilderExtensions_CanComposeHostedBotAndIngressServices()
	{
		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();
		builder.Configuration.AddInMemoryCollection(CreateDiscordHostConfiguration());

		var returnedBuilder = builder
			.AddDiscordHostedService<Bot>()
			.AddDisCatSharpAspNetCore(
				configure: options => options.MaxRequestBodySize = 4096,
				configureAspNetCore: options => options.RoutePrefix = "/discord-api",
				configureOAuth: options =>
				{
					options.ClientId = 734829134102410240;
					options.ClientSecret = "super-secret";
					options.RedirectUri = "https://example.com/discord/oauth/callback";
				});

		using var host = builder.Build();
		var bot = host.Services.GetRequiredService<Bot>();
		var aspNetOptions = host.Services.GetRequiredService<IOptions<DiscordAspNetCoreIngressOptions>>().Value;
		var oauthOptions = host.Services.GetRequiredService<IOptions<DiscordOAuthIngressOptions>>().Value;

		Assert.Same(builder, returnedBuilder);
		Assert.NotNull(bot.Client);
		Assert.Equal("/discord-api", aspNetOptions.RoutePrefix);
		Assert.True(oauthOptions.IsConfigured);
		Assert.NotNull(host.Services.GetRequiredService<IDiscordIngressBodyReader>());
		Assert.NotNull(host.Services.GetRequiredService<IDiscordIngressSignatureValidationService>());
	}

	[Fact]
	public void HostBuilderExtensions_CanComposeHostedBotAndSelfHostedIngressServices()
	{
		using var host = Host.CreateDefaultBuilder()
			.ConfigureLogging(logging => logging.ClearProviders())
			.ConfigureHostConfiguration(builder => builder.AddInMemoryCollection(CreateDiscordHostConfiguration()))
			.AddDiscordHostedService<Bot>()
			.AddDisCatSharpAspNetCoreSelfHost(
				configureSelfHost: options =>
				{
					options.ListenAddress = "127.0.0.1";
					options.ListenPort = 0;
				},
				configureAspNetCore: options => options.RoutePrefix = "/discord-api")
			.Build();

		var bot = host.Services.GetRequiredService<Bot>();
		var aspNetOptions = host.Services.GetRequiredService<IOptions<DiscordAspNetCoreIngressOptions>>().Value;

		Assert.NotNull(bot.Client);
		Assert.Equal("/discord-api", aspNetOptions.RoutePrefix);
		Assert.NotNull(host.Services.GetRequiredService<DiscordAspNetCoreSelfHostRuntime>());
		Assert.Contains(host.Services.GetServices<IHostedService>(), service => string.Equals(service.GetType().FullName, "DisCatSharp.Hosting.AspNetCore.DiscordAspNetCoreSelfHostService", StringComparison.Ordinal));
	}

	[Fact]
	public void MapDisCatSharpIngress_MapsStableEndpoints()
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
		AssertEndpoint(endpoints, "/discord-api/oauth2/complete", DiscordIngressEndpointNames.OAuthCallback, "OAuth", "oauth2/complete", StatusCodes.Status200OK, StatusCodes.Status400BadRequest, StatusCodes.Status403Forbidden, StatusCodes.Status500InternalServerError, StatusCodes.Status502BadGateway);
		AssertEndpoint(endpoints, "/discord-api/gateway", DiscordIngressEndpointNames.Interactions, "Interactions", "gateway", StatusCodes.Status501NotImplemented);
		AssertEndpoint(endpoints, "/discord-api/hooks/events", DiscordIngressEndpointNames.WebhookEvents, "Webhooks", "hooks/events", StatusCodes.Status204NoContent, StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status413PayloadTooLarge);
		AssertEndpoint(endpoints, "/discord-api/hooks/incoming", DiscordIngressEndpointNames.IncomingWebhooks, "IncomingWebhooks", "hooks/incoming", StatusCodes.Status501NotImplemented);
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

		AssertEndpoint(endpoints, "/api/discord/oauth/callback", DiscordIngressEndpointNames.OAuthCallback, "OAuth", "oauth/callback", StatusCodes.Status200OK, StatusCodes.Status400BadRequest, StatusCodes.Status403Forbidden, StatusCodes.Status500InternalServerError, StatusCodes.Status502BadGateway);
		AssertEndpoint(endpoints, "/api/discord/interactions", DiscordIngressEndpointNames.Interactions, "Interactions", "interactions", StatusCodes.Status501NotImplemented);
		AssertEndpoint(endpoints, "/api/discord/webhooks/events", DiscordIngressEndpointNames.WebhookEvents, "Webhooks", "webhooks/events", StatusCodes.Status204NoContent, StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized, StatusCodes.Status413PayloadTooLarge);
		AssertEndpoint(endpoints, "/api/discord/webhooks/incoming", DiscordIngressEndpointNames.IncomingWebhooks, "IncomingWebhooks", "webhooks/incoming", StatusCodes.Status501NotImplemented);
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

		await Assert.ThrowsAsync<DiscordIngressBodyTooLargeException>(() => reader.ReadAsync(body).AsTask());
	}

	[Fact]
	public async Task SignatureValidationService_ValidatesDiscordEd25519Signatures()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.ApplicationVerifyKey = TestApplicationVerifyKey);
		var service = provider.GetRequiredService<IDiscordIngressSignatureValidationService>();

		var result = await service.ValidateAsync(CreateSignedRequest(TestPingBody, TestPingTimestamp, TestPingSignature));

		Assert.Equal(DiscordIngressSignatureValidationStatus.Valid, result.Status);
		Assert.Equal("discord-ed25519", result.ValidatorName);
	}

	[Fact]
	public async Task AspNetCoreIngressRequestExtensions_CaptureRawBodyAndHeaders()
	{
		using var provider = BuildProvider(TimeProvider.System);
		var bodyReader = provider.GetRequiredService<IDiscordIngressBodyReader>();
		var context = CreateWebhookHttpContext(TestPingBody, TestPingTimestamp, TestPingSignature);
		context.Request.QueryString = new QueryString("?delivery=1");

		var request = await context.Request.ToDiscordIngressRequestAsync(bodyReader);

		Assert.Equal(TestPingBody, request.Body.GetString());
		Assert.True(request.TryGetSingleHeaderValue(DiscordIngressHeaderNames.SignatureTimestamp, out var timestamp));
		Assert.Equal(TestPingTimestamp, timestamp);
		Assert.Equal(new Uri("https://example.com/discord/webhooks/events?delivery=1"), request.RequestUri);
		Assert.Equal(0, context.Request.Body.Position);
	}

	[Fact]
	public async Task WebhookEventIngressService_AcknowledgesSignedPingPayloads()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.ApplicationVerifyKey = TestApplicationVerifyKey);
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();

		var result = await service.HandleAsync(CreateSignedRequest(TestPingBody, TestPingTimestamp, TestPingSignature));

		Assert.Equal(StatusCodes.Status204NoContent, result.Response.StatusCode);
		Assert.Equal(DiscordIngressSignatureValidationStatus.Valid, result.SignatureValidation.Status);
		Assert.NotNull(result.Envelope);
		Assert.True(result.Envelope.IsPing);
		Assert.Equal(DiscordWebhookEventTypes.Ping, result.Envelope.Type);
	}

	[Fact]
	public async Task WebhookEventIngressService_AcknowledgesSignedEventPayloads()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.ApplicationVerifyKey = TestApplicationVerifyKey);
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();

		var result = await service.HandleAsync(CreateSignedRequest(TestEventBody, TestEventTimestamp, TestEventSignature));

		Assert.Equal(StatusCodes.Status204NoContent, result.Response.StatusCode);
		Assert.NotNull(result.Envelope);
		Assert.False(result.Envelope.IsPing);
		Assert.Equal(42, result.Envelope.Type);
		Assert.Equal("1234567890", result.Envelope.ApplicationId);
		Assert.Equal("MESSAGE_CREATE", result.Envelope.EventType);
	}

	[Fact]
	public async Task WebhookEventIngressService_RejectsInvalidSignatures()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.ApplicationVerifyKey = TestApplicationVerifyKey);
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();

		var result = await service.HandleAsync(CreateSignedRequest(TestPingBody, TestPingTimestamp, MutateHex(TestPingSignature)));

		Assert.Equal(StatusCodes.Status401Unauthorized, result.Response.StatusCode);
		Assert.Null(result.Envelope);
		Assert.Equal(DiscordIngressSignatureValidationStatus.Invalid, result.SignatureValidation.Status);
	}

	[Fact]
	public async Task WebhookEventIngressService_RejectsMalformedJsonAfterSuccessfulSignatureValidation()
	{
		using var provider = BuildProvider(TimeProvider.System, options => options.ApplicationVerifyKey = TestApplicationVerifyKey);
		var service = provider.GetRequiredService<DiscordWebhookEventIngressService>();

		var result = await service.HandleAsync(CreateSignedRequest(TestMalformedBody, TestMalformedTimestamp, TestMalformedSignature));

		Assert.Equal(StatusCodes.Status400BadRequest, result.Response.StatusCode);
		Assert.Null(result.Envelope);
		Assert.NotNull(result.FailureReason);
	}

	[Fact]
	public async Task WebhookEventEndpointHandler_ReturnsPayloadTooLargeWhenBodyLimitIsExceeded()
	{
		using var provider = BuildProvider(
			TimeProvider.System,
			options =>
			{
				options.ApplicationVerifyKey = TestApplicationVerifyKey;
				options.MaxRequestBodySize = 4;
			});

		var handler = provider.GetRequiredService<DiscordWebhookEventEndpointHandler>();
		var context = CreateWebhookHttpContext(TestPingBody, TestPingTimestamp, TestPingSignature);
		context.RequestServices = provider;

		var result = await handler.HandleAsync(context.Request);
		await result.ExecuteAsync(context);

		Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
	}

	[Fact]
	public async Task OAuthCallbackHandler_ConsumesPendingStateAndPreservesResultMetadata()
	{
		var exchangeService = new FakeOAuthTokenExchangeService(CreateAccessToken(includeWebhookPayload: true, scope: "identify webhook.incoming"));
		using var provider = BuildProvider(
			TimeProvider.System,
			configureOAuth: options =>
			{
				options.ClientId = 734829134102410240;
				options.ClientSecret = "super-secret";
				options.RedirectUri = "https://example.com/oauth/callback";
			},
			configureServices: services => services.Replace(ServiceDescriptor.Singleton<IDiscordOAuthTokenExchangeService>(exchangeService)));

		var store = provider.GetRequiredService<IDiscordIngressPendingStateStore>();
		await store.StoreAsync(new DiscordIngressPendingState
		{
			Key = "valid-state",
			RequestUri = new Uri("https://discord.com/oauth2/authorize?state=valid-state&scope=identify%20webhook.incoming&integration_type=1&redirect_uri=https%3A%2F%2Fexample.com%2Foauth%2Fcallback")
		});

		var handler = provider.GetRequiredService<IDiscordOAuthCallbackHandler>();
		var result = await handler.HandleAsync(new DiscordOAuthCallbackRequest(
			code: "auth-code",
			state: "valid-state",
			callbackUri: new Uri("https://example.com/oauth/callback?code=auth-code&state=valid-state&integration_type=1"),
			queryParameters: new Dictionary<string, string?>
			{
				["code"] = "auth-code",
				["state"] = "valid-state",
				["integration_type"] = "1"
			}));

		Assert.Equal(DiscordOAuthCallbackStatus.Success, result.Status);
		Assert.True(result.IsSuccess);
		Assert.Equal("auth-code", exchangeService.LastCode);
		Assert.Equal("identify webhook.incoming", result.RequestedScope);
		Assert.Equal("identify webhook.incoming", result.GrantedScope);
		Assert.Equal("1", result.IntegrationType);
		Assert.True(result.HasIncomingWebhookPayload);
		Assert.NotNull(result.IncomingWebhookPayload);
		Assert.Equal("https://example.com/oauth/callback", result.RedirectUri?.AbsoluteUri);
		Assert.Null(await store.GetAsync("valid-state"));
	}

	[Fact]
	public async Task OAuthCallbackHandler_ReturnsSecurityFailureWhenStoredRedirectUriDoesNotMatch()
	{
		var exchangeService = new FakeOAuthTokenExchangeService(CreateAccessToken());
		using var provider = BuildProvider(
			TimeProvider.System,
			configureOAuth: options =>
			{
				options.ClientId = 734829134102410240;
				options.ClientSecret = "super-secret";
				options.RedirectUri = "https://example.com/oauth/callback";
			},
			configureServices: services => services.Replace(ServiceDescriptor.Singleton<IDiscordOAuthTokenExchangeService>(exchangeService)));

		var store = provider.GetRequiredService<IDiscordIngressPendingStateStore>();
		await store.StoreAsync(new DiscordIngressPendingState
		{
			Key = "redirect-mismatch",
			RequestUri = new Uri("https://discord.com/oauth2/authorize?state=redirect-mismatch&redirect_uri=https%3A%2F%2Fmalicious.example%2Foauth%2Fcallback")
		});

		var handler = provider.GetRequiredService<IDiscordOAuthCallbackHandler>();
		var result = await handler.HandleAsync(new DiscordOAuthCallbackRequest(
			code: "auth-code",
			state: "redirect-mismatch",
			queryParameters: new Dictionary<string, string?>
			{
				["code"] = "auth-code",
				["state"] = "redirect-mismatch"
			}));

		Assert.Equal(DiscordOAuthCallbackStatus.SecurityFailure, result.Status);
		Assert.Null(exchangeService.LastCode);
		Assert.Equal("The stored OAuth redirect URI does not match the configured redirect URI.", result.Detail);
	}

	[Fact]
	public async Task OAuthCallbackHandler_ReturnsExchangeFailureWhenDiscordExchangeThrows()
	{
		var exchangeService = new FakeOAuthTokenExchangeService(exceptionToThrow: new InvalidOperationException("boom"));
		using var provider = BuildProvider(
			TimeProvider.System,
			configureOAuth: options =>
			{
				options.ClientId = 734829134102410240;
				options.ClientSecret = "super-secret";
				options.RedirectUri = "https://example.com/oauth/callback";
			},
			configureServices: services => services.Replace(ServiceDescriptor.Singleton<IDiscordOAuthTokenExchangeService>(exchangeService)));

		var store = provider.GetRequiredService<IDiscordIngressPendingStateStore>();
		await store.StoreAsync(new DiscordIngressPendingState
		{
			Key = "exchange-failure",
			RequestUri = new Uri("https://discord.com/oauth2/authorize?state=exchange-failure&redirect_uri=https%3A%2F%2Fexample.com%2Foauth%2Fcallback")
		});

		var handler = provider.GetRequiredService<IDiscordOAuthCallbackHandler>();
		var responseFactory = provider.GetRequiredService<IDiscordOAuthCallbackResponseFactory>();
		var result = await handler.HandleAsync(new DiscordOAuthCallbackRequest(
			code: "auth-code",
			state: "exchange-failure",
			queryParameters: new Dictionary<string, string?>
			{
				["code"] = "auth-code",
				["state"] = "exchange-failure"
			}));

		var response = responseFactory.CreateResponse(result);

		Assert.Equal(DiscordOAuthCallbackStatus.ExchangeFailure, result.Status);
		Assert.Equal(StatusCodes.Status502BadGateway, response.StatusCode);
		Assert.Equal("The Discord OAuth code exchange failed.", result.Detail);
	}

	[Fact]
	public async Task OAuthCallbackEndpoint_WritesSuccessJsonWithoutLeakingTokens()
	{
		var exchangeService = new FakeOAuthTokenExchangeService(CreateAccessToken(includeWebhookPayload: true, scope: "identify webhook.incoming"));
		using var app = BuildApp(
			configureOAuth: options =>
			{
				options.ClientId = 734829134102410240;
				options.ClientSecret = "super-secret";
				options.RedirectUri = "https://example.com/oauth/callback";
			},
			configureServices: services => services.Replace(ServiceDescriptor.Singleton<IDiscordOAuthTokenExchangeService>(exchangeService)));

		app.MapDiscordOAuthIngress();

		var store = app.Services.GetRequiredService<IDiscordIngressPendingStateStore>();
		await store.StoreAsync(new DiscordIngressPendingState
		{
			Key = "endpoint-success",
			RequestUri = new Uri("https://discord.com/oauth2/authorize?state=endpoint-success&scope=identify%20webhook.incoming&integration_type=1&redirect_uri=https%3A%2F%2Fexample.com%2Foauth%2Fcallback")
		});

		var endpoint = GetRouteEndpoint(app, "/oauth/callback");
		var context = CreateHttpContext(app.Services, "/oauth/callback?code=endpoint-code&state=endpoint-success&integration_type=1");
		context.SetEndpoint(endpoint);

		await endpoint.RequestDelegate!(context);

		var payload = await ReadResponseBodyAsync(context.Response);
		var document = JObject.Parse(payload);

		Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
		Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
		Assert.Equal("no-store, no-cache", context.Response.Headers["Cache-Control"].ToString());
		Assert.Equal("success", document["status"]?.Value<string>());
		Assert.True(document["incoming_webhook_available"]?.Value<bool>());
		Assert.Equal("identify webhook.incoming", document["granted_scope"]?.Value<string>());
		Assert.Null(document["access_token"]);
		Assert.Null(document["refresh_token"]);
	}

	[Fact]
	public async Task OAuthCallbackEndpoint_WritesInvalidStateResponse()
	{
		using var app = BuildApp(configureOAuth: options =>
		{
			options.ClientId = 734829134102410240;
			options.ClientSecret = "super-secret";
			options.RedirectUri = "https://example.com/oauth/callback";
		});

		app.MapDiscordOAuthIngress();

		var endpoint = GetRouteEndpoint(app, "/oauth/callback");
		var context = CreateHttpContext(app.Services, "/oauth/callback?code=endpoint-code&state=missing-state");
		context.SetEndpoint(endpoint);

		await endpoint.RequestDelegate!(context);

		var payload = await ReadResponseBodyAsync(context.Response);
		var document = JObject.Parse(payload);

		Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
		Assert.Equal("invalid_state", document["status"]?.Value<string>());
		Assert.Equal("missing-state", document["state"]?.Value<string>());
	}

	[Fact]
	public void AddDisCatSharpAspNetCoreSelfHost_RegistersSelfHostInfrastructure()
	{
		ServiceCollection services = [];

		services.AddDisCatSharpAspNetCoreSelfHost(
			configureSelfHost: options =>
			{
				options.ListenAddress = "0.0.0.0";
				options.ListenPort = 8443;
				options.Scheme = "https";
				options.BaseUrl = new Uri("https://bot.example.com/base");
			},
			configureAspNetCore: options => options.RoutePrefix = "/discord-api");

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<DiscordAspNetCoreSelfHostOptions>>().Value;

		Assert.Equal("0.0.0.0", options.ListenAddress);
		Assert.Equal(8443, options.ListenPort);
		Assert.Equal("https", options.Scheme);
		Assert.Equal(new Uri("https://bot.example.com/base"), options.BaseUrl);
		Assert.NotNull(provider.GetRequiredService<DiscordAspNetCoreSelfHostRuntime>());
		Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IHostedService)
			&& string.Equals(descriptor.ImplementationType?.FullName, "DisCatSharp.Hosting.AspNetCore.DiscordAspNetCoreSelfHostService", StringComparison.Ordinal));
	}

	[Fact]
	public async Task SelfHostMode_StartsInternalIngressAppAndTracksRuntimeAddresses()
	{
		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();
		builder.Services.AddDisCatSharpAspNetCoreSelfHost(
			configureSelfHost: options =>
			{
				options.ListenAddress = "127.0.0.1";
				options.ListenPort = 0;
				options.BaseUrl = new Uri("https://public.example.test/bot");
			},
			configureAspNetCore: options =>
			{
				options.RoutePrefix = "/discord-api";
				options.OAuthPath = "oauth2";
				options.OAuthCallbackPath = "complete";
			});

		using var host = builder.Build();
		var runtime = host.Services.GetRequiredService<DiscordAspNetCoreSelfHostRuntime>();

		await host.StartAsync();

		try
		{
			Assert.NotNull(runtime.ListenBaseUrl);
			Assert.Equal(new Uri("https://public.example.test/bot"), runtime.PublicBaseUrl);

			using HttpClient client = new();
			using var response = await client.GetAsync(new Uri(runtime.ListenBaseUrl!, "/discord-api/oauth2/complete"));

			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}
		finally
		{
			await host.StopAsync();
			Assert.Null(runtime.ListenBaseUrl);
			Assert.Null(runtime.PublicBaseUrl);
		}
	}

	[Fact]
	public async Task SelfHostMode_UsesListenBaseUrlAsPublicBaseUrlByDefault()
	{
		var builder = Host.CreateApplicationBuilder();
		builder.Logging.ClearProviders();
		builder.Services.AddDisCatSharpAspNetCoreSelfHost(
			configureSelfHost: options =>
			{
				options.ListenAddress = "127.0.0.1";
				options.ListenPort = 0;
			});

		using var host = builder.Build();
		var runtime = host.Services.GetRequiredService<DiscordAspNetCoreSelfHostRuntime>();

		await host.StartAsync();

		try
		{
			Assert.NotNull(runtime.ListenBaseUrl);
			Assert.Equal(runtime.ListenBaseUrl, runtime.PublicBaseUrl);
		}
		finally
		{
			await host.StopAsync();
		}
	}

	private static ServiceProvider BuildProvider(
		TimeProvider timeProvider,
		Action<DiscordWebIngressOptions>? configure = null,
		Action<DiscordOAuthIngressOptions>? configureOAuth = null,
		Action<IServiceCollection>? configureServices = null
	)
	{
		ServiceCollection services = [];
		services.AddSingleton<TimeProvider>(timeProvider);
		services.AddDisCatSharpAspNetCore(configure: configure, configureOAuth: configureOAuth);
		configureServices?.Invoke(services);
		return services.BuildServiceProvider();
	}

	private static Dictionary<string, string?> CreateDiscordHostConfiguration() =>
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

	private static WebApplication BuildApp(
		Action<DiscordAspNetCoreIngressOptions>? configureAspNetCore = null,
		Action<DiscordOAuthIngressOptions>? configureOAuth = null,
		Action<IServiceCollection>? configureServices = null
	)
	{
		var builder = WebApplication.CreateBuilder();
		builder.Services.AddDisCatSharpAspNetCore(configureAspNetCore: configureAspNetCore, configureOAuth: configureOAuth);
		configureServices?.Invoke(builder.Services);
		return builder.Build();
	}

	private static IReadOnlyList<RouteEndpoint> GetRouteEndpoints(IEndpointRouteBuilder endpoints)
		=> endpoints.DataSources
			.SelectMany(static source => source.Endpoints)
			.OfType<RouteEndpoint>()
			.ToArray();

	private static RouteEndpoint GetRouteEndpoint(IEndpointRouteBuilder endpoints, string pattern)
	{
		var normalizedPattern = NormalizePattern(pattern);
		return Assert.Single(GetRouteEndpoints(endpoints), endpoint => string.Equals(NormalizePattern(endpoint.RoutePattern.RawText), normalizedPattern, StringComparison.Ordinal));
	}

	private static void AssertEndpoint(
		IEnumerable<RouteEndpoint> endpoints,
		string pattern,
		string endpointName,
		string module,
		string relativePath,
		params int[] expectedStatusCodes
	)
	{
		var normalizedPattern = NormalizePattern(pattern);
		var endpoint = Assert.Single(endpoints, endpoint => string.Equals(NormalizePattern(endpoint.RoutePattern.RawText), normalizedPattern, StringComparison.Ordinal));
		var metadata = Assert.IsType<DiscordIngressEndpointMetadata>(endpoint.Metadata.GetMetadata<DiscordIngressEndpointMetadata>());

		Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
		Assert.Equal(module, metadata.Module);
		Assert.Equal(endpointName, metadata.EndpointName);
		Assert.Equal(relativePath, metadata.RelativePath);

		var producedStatusCodes = endpoint.Metadata
			.OfType<IProducesResponseTypeMetadata>()
			.Select(static metadata => metadata.StatusCode)
			.ToArray();
		foreach (var statusCode in expectedStatusCodes)
			Assert.Contains(statusCode, producedStatusCodes);
	}

	private static DefaultHttpContext CreateHttpContext(IServiceProvider services, string pathAndQuery)
	{
		DefaultHttpContext context = new()
		{
			RequestServices = services
		};
		context.Request.Method = HttpMethods.Get;
		context.Request.Scheme = "https";
		context.Request.Host = new HostString("example.com");
		context.Response.Body = new MemoryStream();

		var queryIndex = pathAndQuery.IndexOf('?', StringComparison.Ordinal);
		context.Request.Path = queryIndex < 0 ? pathAndQuery : pathAndQuery[..queryIndex];
		context.Request.QueryString = queryIndex < 0 ? QueryString.Empty : new QueryString(pathAndQuery[queryIndex..]);

		return context;
	}

	private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
	{
		response.Body.Position = 0;
		using StreamReader reader = new(response.Body, leaveOpen: true);
		return await reader.ReadToEndAsync();
	}

	private static DiscordAccessToken CreateAccessToken(bool includeWebhookPayload = false, string scope = "identify")
	{
		var json = includeWebhookPayload
			? $"{{\"access_token\":\"access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"refresh_token\":\"refresh-token\",\"scope\":\"{scope}\",\"webhook\":{{\"id\":\"1234567890\",\"type\":1,\"name\":\"incoming\",\"token\":\"webhook-token\"}}}}"
			: $"{{\"access_token\":\"access-token\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"refresh_token\":\"refresh-token\",\"scope\":\"{scope}\"}}";

		return DiscordAccessToken.FromJson(json);
	}

	private static string NormalizePattern(string? pattern)
	{
		if (string.IsNullOrWhiteSpace(pattern))
			return "/";

		return $"/{pattern.Trim().Trim('/')}";
	}

	private static DiscordIngressRequest CreateSignedRequest(string body, string timestamp, string signature)
		=> new(
			HttpMethods.Post,
			new Uri("https://example.com/discord/webhooks/events"),
			new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
			{
				[DiscordIngressHeaderNames.SignatureTimestamp] = timestamp,
				[DiscordIngressHeaderNames.SignatureEd25519] = signature
			},
			DiscordIngressPayload.FromString(body));

	private static DefaultHttpContext CreateWebhookHttpContext(string body, string timestamp, string signature)
	{
		DefaultHttpContext context = new();
		context.RequestServices = new ServiceCollection()
			.AddLogging()
			.BuildServiceProvider();
		context.Request.Method = HttpMethods.Post;
		context.Request.Scheme = "https";
		context.Request.Host = new HostString("example.com");
		context.Request.Path = "/discord/webhooks/events";
		context.Request.Headers[DiscordIngressHeaderNames.SignatureTimestamp] = timestamp;
		context.Request.Headers[DiscordIngressHeaderNames.SignatureEd25519] = signature;
		context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
		context.Response.Body = new MemoryStream();
		return context;
	}

	private static string MutateHex(string value)
		=> value[^1] == '0'
			? string.Concat(value.AsSpan(0, value.Length - 1), "1")
			: string.Concat(value.AsSpan(0, value.Length - 1), "0");

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

	private sealed class FakeOAuthTokenExchangeService : IDiscordOAuthTokenExchangeService
	{
		private readonly DiscordAccessToken? _accessToken;
		private readonly Exception? _exceptionToThrow;

		public FakeOAuthTokenExchangeService(DiscordAccessToken? accessToken = null, Exception? exceptionToThrow = null)
		{
			this._accessToken = accessToken;
			this._exceptionToThrow = exceptionToThrow;
		}

		public string? LastCode { get; private set; }

		public Task<DiscordAccessToken> ExchangeAccessTokenAsync(string code, System.Threading.CancellationToken cancellationToken = default)
		{
			this.LastCode = code;
			if (this._exceptionToThrow is not null)
				return Task.FromException<DiscordAccessToken>(this._exceptionToThrow);

			return Task.FromResult(this._accessToken ?? throw new InvalidOperationException("A fake access token must be provided."));
		}
	}
}
