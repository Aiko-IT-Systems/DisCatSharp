---
uid: modules_web_ingress_overview
title: Web Ingress Overview
author: DisCatSharp Team
---

# Web Ingress Overview

`DisCatSharp.Hosting.AspNetCore` is the first-party HTTP ingress package for DisCatSharp.

It adds:

- OAuth callback handling
- Discord interactions over incoming HTTP
- signed Discord webhook event handling
- existing ASP.NET Core app integration
- optional self-hosted ASP.NET Core ingress
- proxy helpers and portal/config validation helpers

## Install

Add the following packages as needed:

- `DisCatSharp`
- `DisCatSharp.Hosting.AspNetCore`
- optionally `DisCatSharp.Hosting.DependencyInjection` when you also host your bot with the generic host helpers

## Existing ASP.NET Core app

Register the ingress services and map the endpoints into your app:

```cs
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

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

## Self-hosted ingress

If you do not already own an ASP.NET Core app, use the self-hosted mode:

```cs
using DisCatSharp.Hosting.AspNetCore;

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
    },
    configureAspNetCore: options =>
    {
        options.RoutePrefix = "/discord";
    });
```

## Default route layout

By default, `MapDisCatSharpIngress()` exposes:

| Endpoint | Default path |
| --- | --- |
| OAuth callback | `/discord/oauth/callback` |
| Interactions | `/discord/interactions` |
| Webhook events | `/discord/webhooks/events` |
| Incoming webhooks | `/discord/webhooks/incoming` |

These values come from:

- [DiscordAspNetCoreIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.DiscordAspNetCoreIngressOptions)
- [DiscordWebIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordWebIngressOptions)
- [DiscordOAuthIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordOAuthIngressOptions)

## Endpoint groups

If you prefer to map only part of the ingress surface, use the more specific methods:

- `MapDiscordOAuthIngress()`
- `MapDiscordInteractionIngress()`
- `MapDiscordWebhookIngress()`

These can also be nested under a custom `MapGroup(...)`.

## Important current limitation

> [!IMPORTANT]
> The signed **webhook events** endpoint is implemented.
> The more generic **incoming webhooks** endpoint is still a placeholder and currently returns `501 Not Implemented`.

So today, treat the package as production-ready for:

- OAuth callbacks
- HTTP interactions
- signed webhook events

but **not yet** as a finished generic incoming webhook receiver.
