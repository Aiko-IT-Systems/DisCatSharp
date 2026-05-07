---
uid: modules_web_ingress_overview
title: Web Ingress Overview
author: DisCatSharp Team
---

# Web Ingress Overview

`DisCatSharp.Hosting.AspNetCore` is the first-party ASP.NET Core ingress package for DisCatSharp.

The XML docs are the canonical API reference. This article is the short map: what lives where, which surfaces are meant to be composed together, and when to use the package inside an existing app versus the built-in self-host.

## What the package owns

- Discord OAuth callback ingress
- Discord interactions delivered over HTTP
- signed Discord webhook events
- app-defined incoming webhooks
- linked-roles helpers
- public URL, reverse-proxy, and validation helpers
- optional self-hosted ASP.NET Core infrastructure

## Namespace and package layout

| Area | Namespace | Responsibility |
| --- | --- | --- |
| Registration and route mapping | `DisCatSharp.Hosting.AspNetCore` | `AddDisCatSharpAspNetCore(...)`, `AddDisCatSharpAspNetCoreSelfHost(...)`, `MapDisCatSharpIngress()`, route options |
| Shared ingress primitives | `DisCatSharp.Hosting.AspNetCore.Ingress` | transport-neutral requests/responses, request buffering, pending OAuth state, shared web options |
| OAuth callback flow | `DisCatSharp.Hosting.AspNetCore.Ingress.OAuth` | callback handler, callback result, callback response factory, OAuth options |
| HTTP interactions | `DisCatSharp.Hosting.AspNetCore.Ingress.Interactions` | interaction handler contracts and inline response helpers |
| Webhook events | `DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents` and `DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook` | signed Discord webhook parsing plus typed async events |
| Generic incoming webhooks | `DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks` | app-defined webhook handlers |
| Linked roles | `DisCatSharp.Hosting.AspNetCore.LinkedRoles` | verification URL, metadata sync, OAuth role-connection publishing |
| URL / proxy / validation helpers | `DisCatSharp.Hosting.AspNetCore.Routing`, `DisCatSharp.Hosting.AspNetCore.Deployment`, `DisCatSharp.Hosting.AspNetCore.Validation` | computed public URLs, proxy snippets, offline configuration checks |

In short: use the root namespace to register and map the package, then drop into feature namespaces only when you are customizing a specific ingress surface.

## Install

Add the packages you actually need:

- `DisCatSharp`
- `DisCatSharp.Hosting.AspNetCore`
- optionally `DisCatSharp.Hosting.DependencyInjection` when you also compose the bot with generic-host helpers

## Choose a hosting model

### Existing ASP.NET Core app

Use this when you already own the ASP.NET Core pipeline and want full control over middleware order, auth, route groups, or partial endpoint mapping.

```cs
using DisCatSharp.Hosting.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDisCatSharpAspNetCore(
    configure: options =>
    {
        options.ApplicationVerifyKey = "YOUR_DISCORD_VERIFY_KEY";
    },
    configureOAuth: options =>
    {
        options.ClientId = 123456789012345678;
        options.ClientSecret = "YOUR_CLIENT_SECRET";
        options.RedirectUri = "https://bot.example.com/discord/oauth/callback";
    });

var app = builder.Build();

app.MapDisCatSharpIngress();

app.Run();
```

### Self-hosted ingress

Use this when you only need the ingress surface and do not already have an ASP.NET Core app. The hosted service spins up an internal `WebApplication` and maps the default ingress routes for you.

```cs
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddDisCatSharpAspNetCoreSelfHost(
    configureSelfHost: options =>
    {
        options.ListenAddress = "127.0.0.1";
        options.ListenPort = 8080;
        options.BaseUrl = new Uri("https://bot.example.com");
    },
    configure: options =>
    {
        options.ApplicationVerifyKey = "YOUR_DISCORD_VERIFY_KEY";
    });

builder.Services.Configure<DiscordOAuthIngressOptions>(options =>
{
    options.ClientId = 123456789012345678;
    options.ClientSecret = "YOUR_CLIENT_SECRET";
    options.RedirectUri = "https://bot.example.com/discord/oauth/callback";
});
```

## Default route layout

By default, the mapped ingress surface is:

| Endpoint | Default path |
| --- | --- |
| OAuth callback | `/discord/oauth/callback` |
| Interactions | `/discord/interactions` |
| Webhook events | `/discord/webhooks/events` |
| Incoming webhooks | `/discord/webhooks/incoming` |

Those routes are shaped by:

- [DiscordAspNetCoreIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.DiscordAspNetCoreIngressOptions)
- [DiscordWebIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordWebIngressOptions)
- [DiscordOAuthIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.DiscordOAuthIngressOptions)

If you only want part of the ingress surface, map the feature groups individually:

- `MapDiscordOAuthIngress()`
- `MapDiscordInteractionIngress()`
- `MapDiscordWebhookIngress()`

These can be nested under your own `MapGroup(...)`.

## Extension points

The package is intentionally small at the edge. Common customization points are:

- [IDiscordIngressPendingStateStore](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordIngressPendingStateStore) for persisted OAuth state
- [IDiscordOAuthCallbackHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.IDiscordOAuthCallbackHandler) and [IDiscordOAuthCallbackResponseFactory](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.IDiscordOAuthCallbackResponseFactory) for custom OAuth completion flows
- [IDiscordInteractionIngressHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.Interactions.IDiscordInteractionIngressHandler) for HTTP interactions
- [IDiscordIncomingWebhookHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks.IDiscordIncomingWebhookHandler) for app-defined webhooks
- [DiscordWebhookEventDispatcher](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventDispatcher) for signed Discord webhook events
- [IDiscordLinkedRolesMetadataProvider](xref:DisCatSharp.Hosting.AspNetCore.LinkedRoles.IDiscordLinkedRolesMetadataProvider) for linked-roles metadata sync
- [IDiscordIngressSignatureValidator](xref:DisCatSharp.Hosting.AspNetCore.Ingress.Security.IDiscordIngressSignatureValidator) when you need extra ingress signature validation

## Feature boundaries

- OAuth, interactions, and signed webhook events are Discord-defined surfaces.
- incoming webhooks are **your** app's generic webhook surface, not Discord's signed event protocol.
- linked roles are helpers around verification URLs, metadata, and role-connection publishing; they do not create your verification page for you.
- the ASP.NET Core package does **not** automatically bridge HTTP interactions into `ApplicationCommandsExtension`; treat HTTP ingress as a separate delivery surface.

## Next steps

- [OAuth Callbacks](oauth_callbacks.md)
- [HTTP Interactions](interactions_http.md)
- [Linked Roles](linked_roles.md)
- [Webhook Events vs Incoming Webhooks](webhook_events.md)
- [Proxy and Validation Helpers](proxy_and_validation.md)
