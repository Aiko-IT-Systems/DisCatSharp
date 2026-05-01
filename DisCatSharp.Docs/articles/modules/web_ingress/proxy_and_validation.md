---
uid: modules_web_ingress_proxy_validation
title: Proxy and Validation Helpers
author: DisCatSharp Team
---

# Proxy and Validation Helpers

The ingress package includes helpers for computing public URLs, generating reverse-proxy snippets, and checking whether your local configuration matches your Discord application.

## Compute the public URLs

Use [DiscordIngressPublicUrls](xref:DisCatSharp.Hosting.AspNetCore.DiscordIngressPublicUrls) when you want to see the exact public surface produced by your configured route layout.

```cs
using DisCatSharp.Hosting.AspNetCore;

var urls = DiscordIngressPublicUrls.Create(
    new Uri("https://bot.example.com/base"),
    new DiscordAspNetCoreIngressOptions
    {
        RoutePrefix = "/discord-api",
        OAuthPath = "oauth2",
        OAuthCallbackPath = "complete",
        InteractionsPath = "interactions",
        WebhooksPath = "hooks",
        WebhookEventsPath = "events",
        IncomingWebhooksPath = "incoming"
    });

Console.WriteLine(urls.OAuthCallbackUrl);
Console.WriteLine(urls.InteractionsUrl);
Console.WriteLine(urls.WebhookEventsUrl);
Console.WriteLine(urls.IncomingWebhooksUrl);
```

## Generate proxy snippets

Use [DiscordIngressProxyHelpers](xref:DisCatSharp.Hosting.AspNetCore.DiscordIngressProxyHelpers) to generate starter reverse-proxy configs.

Supported helpers include:

- `CreateNginxConfig(...)`
- `CreateApacheConfig(...)`
- `CreateDockerNginxConfig(...)`
- `CreateTraefikDockerLabels(...)`
- `CreateDockerCaddyConfig(...)`

Example:

```cs
var nginx = DiscordIngressProxyHelpers.CreateNginxConfig(
    new Uri("https://bot.example.com/base"),
    new Uri("http://127.0.0.1:5005/internal"),
    new DiscordAspNetCoreIngressOptions
    {
        RoutePrefix = "/discord-api"
    });

Console.WriteLine(nginx);
```

These helpers are intended to give you a correct starting point for:

- forwarded headers
- path-prefix forwarding
- mapping the public ingress root to the internal ASP.NET Core listener

## Validate portal and local configuration

Use [DiscordIngressConfigurationValidator](xref:DisCatSharp.Hosting.AspNetCore.DiscordIngressConfigurationValidator) to compare:

- your public base URL
- your ingress route layout
- your verify key
- your OAuth redirect URI
- your current [DiscordApplication](xref:DisCatSharp.Entities.DiscordApplication)
- optionally your active [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client)

```cs
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

var report = DiscordIngressConfigurationValidator.Validate(new DiscordIngressValidationContext
{
    PublicBaseUrl = new Uri("https://bot.example.com/base"),
    AspNetCoreOptions = new DiscordAspNetCoreIngressOptions
    {
        RoutePrefix = "/discord-api"
    },
    WebOptions = new DiscordWebIngressOptions
    {
        ApplicationVerifyKey = "YOUR_VERIFY_KEY"
    },
    OAuthOptions = new DiscordOAuthIngressOptions
    {
        ClientId = 123456789012345678,
        ClientSecret = "YOUR_CLIENT_SECRET",
        RedirectUri = "https://bot.example.com/base/discord-api/oauth/callback"
    },
    Application = application
});

if (!report.IsValid)
{
    foreach (var issue in report.Issues)
        Console.WriteLine($"{issue.Severity}: {issue.Code} - {issue.Message}");
}
```

This is especially useful when checking:

- redirect URI mismatches
- interactions endpoint mismatches
- verify-key mismatches
- role-connections verification URL mismatches

## When to use these helpers

Use them when you are:

- standing up ingress for the first time
- moving behind a reverse proxy
- switching route prefixes
- checking production config against the Discord developer portal
