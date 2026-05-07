using System;
using System.Linq;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Deployment;
using DisCatSharp.Hosting.AspNetCore.Ingress;
using DisCatSharp.Hosting.AspNetCore.Routing;

using Xunit;

namespace DisCatSharp.Hosting.Tests;

public sealed class AspNetCoreIngressHelperTests
{
	[Fact]
	public void PublicUrls_Create_ComposesExpectedIngressEndpoints()
	{
		DiscordAspNetCoreIngressOptions options = new()
		{
			RoutePrefix = "/discord-api",
			OAuthPath = "oauth2",
			OAuthCallbackPath = "complete",
			InteractionsPath = "interactions",
			WebhooksPath = "hooks",
			WebhookEventsPath = "events",
			IncomingWebhooksPath = "incoming"
		};

		var urls = DiscordIngressPublicUrls.Create(new Uri("https://bot.example.com/base"), options);

		Assert.Equal("/base/discord-api", urls.IngressRootPath);
		Assert.Equal("https://bot.example.com/base/discord-api", urls.IngressRootUrl.AbsoluteUri);
		Assert.Equal("https://bot.example.com/base/discord-api/oauth2/complete", urls.OAuthCallbackUrl.AbsoluteUri);
		Assert.Equal("https://bot.example.com/base/discord-api/interactions", urls.InteractionsUrl.AbsoluteUri);
		Assert.Equal("https://bot.example.com/base/discord-api/hooks/events", urls.WebhookEventsUrl.AbsoluteUri);
		Assert.Equal("https://bot.example.com/base/discord-api/hooks/incoming", urls.IncomingWebhooksUrl.AbsoluteUri);
	}

	[Fact]
	public void PublicUrls_Create_ComposesLinkedRolesVerificationUrlWhenConfigured()
	{
		var urls = DiscordIngressPublicUrls.Create(
			new Uri("https://bot.example.com/base"),
			new DiscordAspNetCoreIngressOptions(),
			new DiscordLinkedRolesOptions
			{
				VerificationPath = "role-connections/verify"
			});

		Assert.Equal("https://bot.example.com/base/role-connections/verify", urls.RoleConnectionsVerificationUrl?.AbsoluteUri);
	}

	[Fact]
	public void ProxyHelpers_GenerateExpectedReverseProxySnippets()
	{
		DiscordAspNetCoreIngressOptions options = new()
		{
			RoutePrefix = "/discord-api"
		};

		var nginx = DiscordIngressProxyHelpers.CreateNginxConfig(
			new Uri("https://bot.example.com/base"),
			new Uri("http://127.0.0.1:5005/internal"),
			options);
		var apache = DiscordIngressProxyHelpers.CreateApacheConfig(
			new Uri("https://bot.example.com/base"),
			new Uri("http://127.0.0.1:5005/internal"),
			options);
		var dockerNginx = DiscordIngressProxyHelpers.CreateDockerNginxConfig(
			new Uri("https://bot.example.com/base"),
			"discatsharp-app",
			8080,
			options);
		var traefik = DiscordIngressProxyHelpers.CreateTraefikDockerLabels(
			new Uri("https://bot.example.com/base"),
			"discatsharp-ingress",
			8080,
			options);
		var caddy = DiscordIngressProxyHelpers.CreateDockerCaddyConfig(
			new Uri("https://bot.example.com/base"),
			"discatsharp-app",
			8080,
			options);

		Assert.Contains("location = /base/discord-api", nginx, StringComparison.Ordinal);
		Assert.Contains("proxy_pass http://127.0.0.1:5005/internal/discord-api/;", nginx, StringComparison.Ordinal);
		Assert.Contains("ProxyPass \"/base/discord-api\" \"http://127.0.0.1:5005/internal/discord-api\"", apache, StringComparison.Ordinal);
		Assert.Contains("proxy_pass http://discatsharp-app:8080/discord-api/;", dockerNginx, StringComparison.Ordinal);
		Assert.Equal("Host(`bot.example.com`) && PathPrefix(`/base/discord-api`)", traefik["traefik.http.routers.discatsharp-ingress.rule"]);
		Assert.Equal("^/base/discord-api(/.*)?$", traefik["traefik.http.middlewares.discatsharp-ingress-rewrite.replacepathregex.regex"]);
		Assert.Equal("/discord-api$1", traefik["traefik.http.middlewares.discatsharp-ingress-rewrite.replacepathregex.replacement"]);
		Assert.Contains("uri strip_prefix /base/discord-api", caddy, StringComparison.Ordinal);
		Assert.Contains("reverse_proxy discatsharp-app:8080", caddy, StringComparison.Ordinal);
	}

	[Fact]
	public void ValidationReport_PassesWhenPortalAndLocalStateMatch()
	{
		DiscordAspNetCoreIngressOptions aspNetOptions = new()
		{
			RoutePrefix = "/discord-api",
			OAuthPath = "oauth2",
			OAuthCallbackPath = "complete"
		};
		DiscordWebIngressOptions webOptions = new()
		{
			ApplicationVerifyKey = "976B18761C2BA7BF877058100B7AFD8BC8B822CFB82ED16BDB5F80C7A86909D7"
		};
		DiscordOAuthIngressOptions oauthOptions = new()
		{
			ClientId = 734829134102410240,
			ClientSecret = "super-secret",
			RedirectUri = "https://bot.example.com/base/discord-api/oauth2/complete"
		};
		DiscordApplication application = new()
		{
			Id = 734829134102410240,
			VerifyKey = "976b18761c2ba7bf877058100b7afd8bc8b822cfb82ed16bdb5f80c7a86909d7",
			InteractionsEndpointUrl = "https://bot.example.com/base/discord-api/interactions",
			RoleConnectionsVerificationUrl = "https://bot.example.com/role-connections/verify",
			RedirectUris =
			[
				"https://bot.example.com/base/discord-api/oauth2/complete",
				"https://bot.example.com/other"
			]
		};

		var report = DiscordIngressConfigurationValidator.Validate(new DiscordIngressValidationContext
		{
			PublicBaseUrl = new Uri("https://bot.example.com/base"),
			ExpectedRoleConnectionsVerificationUrl = new Uri("https://bot.example.com/role-connections/verify"),
			AspNetCoreOptions = aspNetOptions,
			WebOptions = webOptions,
			OAuthOptions = oauthOptions,
			Application = application
		});

		Assert.True(report.IsValid);
		Assert.NotNull(report.PublicUrls);
		Assert.Empty(report.Issues);
	}

	[Fact]
	public void ValidationReport_FlagsPortalAndClientMismatches()
	{
		DiscordAspNetCoreIngressOptions aspNetOptions = new()
		{
			RoutePrefix = "/discord-api"
		};
		DiscordWebIngressOptions webOptions = new()
		{
			ApplicationVerifyKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
		};
		DiscordOAuthIngressOptions oauthOptions = new()
		{
			ClientId = 734829134102410240,
			ClientSecret = "super-secret",
			RedirectUri = "https://bot.example.com/base/discord-api/oauth/callback"
		};
		DiscordApplication application = new()
		{
			Id = 734829134102410241,
			VerifyKey = "976b18761c2ba7bf877058100b7afd8bc8b822cfb82ed16bdb5f80c7a86909d7",
			InteractionsEndpointUrl = "https://bot.example.com/discord-api/wrong",
			RoleConnectionsVerificationUrl = "https://bot.example.com/role-connections/portal",
			RedirectUris =
			[
				"https://bot.example.com/base/discord-api/oauth/callback/other"
			]
		};

		using var oauthClient = new DiscordOAuth2Client(
			734829134102410240,
			"another-secret",
			"https://bot.example.com/base/discord-api/oauth/callback/other");

		var report = DiscordIngressConfigurationValidator.Validate(new DiscordIngressValidationContext
		{
			PublicBaseUrl = new Uri("https://bot.example.com/base"),
			ExpectedRoleConnectionsVerificationUrl = new Uri("https://bot.example.com/role-connections/local"),
			AspNetCoreOptions = aspNetOptions,
			WebOptions = webOptions,
			OAuthOptions = oauthOptions,
			Application = application,
			OAuthClient = oauthClient
		});

		Assert.False(report.IsValid);
		Assert.Contains(report.Issues, static issue => issue.Code == "redirect-uri-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "application-client-id-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "verify-key-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "interactions-endpoint-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "role-connections-url-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "oauth-client-secret-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Code == "oauth-client-redirect-mismatch" && issue.Severity is DiscordIngressValidationSeverity.Error);
		Assert.Contains(report.Issues, static issue => issue.Severity is DiscordIngressValidationSeverity.Error);
	}

	[Fact]
	public void ValidationReport_CanComputeExpectedLinkedRolesVerificationUrlFromOptions()
	{
		DiscordApplication application = new()
		{
			RoleConnectionsVerificationUrl = "https://bot.example.com/base/role-connections/verify"
		};

		var report = DiscordIngressConfigurationValidator.Validate(new DiscordIngressValidationContext
		{
			PublicBaseUrl = new Uri("https://bot.example.com/base"),
			LinkedRolesOptions = new DiscordLinkedRolesOptions
			{
				VerificationPath = "role-connections/verify"
			},
			Application = application
		});

		Assert.True(report.IsValid);
		Assert.Equal("https://bot.example.com/base/role-connections/verify", report.PublicUrls?.RoleConnectionsVerificationUrl?.AbsoluteUri);
	}
}
