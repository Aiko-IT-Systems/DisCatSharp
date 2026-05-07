---
uid: modules_web_ingress_oauth_callbacks
title: OAuth Callbacks
author: DisCatSharp Team
---

# OAuth Callbacks

The ingress package gives you the **callback endpoint** for Discord's authorization-code flow.

Use the root namespace to register the route and the `DisCatSharp.Hosting.AspNetCore.Ingress.OAuth` namespace when you need the callback contracts and result types.

The package owns the **callback handling** step. Your application still owns the part that starts the OAuth flow.

## Flow summary

1. generate a secure `state`
2. create an authorize URL with [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client)
3. store the pending request in [IDiscordIngressPendingStateStore](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordIngressPendingStateStore)
4. redirect the user to Discord
5. let the ingress callback handler validate the query and exchange the code

## Register the services

```cs
using DisCatSharp;
using DisCatSharp.Hosting.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new DiscordOAuth2Client(
    123456789012345678,
    "YOUR_CLIENT_SECRET",
    "https://bot.example.com/discord/oauth/callback"));

builder.Services.AddDisCatSharpAspNetCore(
    configureOAuth: options =>
    {
        options.ClientId = 123456789012345678;
        options.ClientSecret = "YOUR_CLIENT_SECRET";
        options.RedirectUri = "https://bot.example.com/discord/oauth/callback";
    });

var app = builder.Build();
```

## Start the flow

Your app is responsible for issuing the redirect and storing the pending state:

```cs
using DisCatSharp;
using DisCatSharp.Hosting.AspNetCore.Ingress;
using DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

app.MapGet(
    "/login/discord",
    async (DiscordOAuth2Client oauthClient, IDiscordIngressPendingStateStore pendingStates, TimeProvider timeProvider) =>
    {
        var state = oauthClient.GenerateState();
        var authorizeUri = oauthClient.GenerateOAuth2Url("identify guilds", state);
        var now = timeProvider.GetUtcNow();

        await pendingStates.StoreAsync(new DiscordIngressPendingState
        {
            Key = state,
            Flow = DiscordOAuthIngressOptions.DefaultPendingStateFlow,
            RequestUri = authorizeUri,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(15)
        });

        return Results.Redirect(authorizeUri.AbsoluteUri);
    });
```

Persist the original `RequestUri`. The callback flow uses it to preserve the originally requested scope, the `integration_type`, and redirect URI consistency checks.

## Map the callback endpoint

```cs
app.MapDisCatSharpIngress();
```

Or, if you only want the OAuth surface:

```cs
app.MapDiscordOAuthIngress();
```

The callback endpoint then:

- validates the callback query
- consumes the pending state
- verifies the configured redirect URI
- exchanges the authorization code with Discord
- returns a structured ingress response

## Built-in callback response

The default callback endpoint emits JSON.

Successful payloads include fields such as:

- `status`
- `requested_scope`
- `granted_scope`
- `integration_type`
- `incoming_webhook_available`
- callback and redirect URI metadata

Failures keep the same structured shape and include the failure classification and detail.

If you need the full in-memory model instead of the HTTP payload, use [DiscordOAuthCallbackResult](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.DiscordOAuthCallbackResult).

## Custom callback behavior

If you need a custom success page, redirect, session sign-in, or persistence flow, choose one of these extension points:

1. replace [IDiscordOAuthCallbackResponseFactory](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.IDiscordOAuthCallbackResponseFactory) to keep the built-in route but change the emitted HTTP response
2. call [IDiscordOAuthCallbackHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.IDiscordOAuthCallbackHandler) yourself from a custom route and use the returned [DiscordOAuthCallbackResult](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.DiscordOAuthCallbackResult)

That keeps the validation and token-exchange logic reusable while letting your app own the final user experience.

## Existing app vs self-host

The callback logic is shared across both hosting modes.

- existing ASP.NET Core app: register the services, then map `MapDisCatSharpIngress()` or `MapDiscordOAuthIngress()`
- self-host mode: call `AddDisCatSharpAspNetCoreSelfHost(...)`, then configure [DiscordOAuthIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.Ingress.OAuth.DiscordOAuthIngressOptions) through DI before the host starts

The self-host wrapper only provides the internal ASP.NET Core app. The OAuth contracts, pending-state store, and callback handler are the same services either way.

For the full package map, see [Web Ingress Overview](overview.md).
