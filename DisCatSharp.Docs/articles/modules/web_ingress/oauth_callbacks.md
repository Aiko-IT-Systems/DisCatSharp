---
uid: modules_web_ingress_oauth_callbacks
title: OAuth Callbacks
author: DisCatSharp Team
---

# OAuth Callbacks

The ingress package gives you the **callback endpoint** for Discord's authorization-code flow.

The typical flow is:

1. generate a secure `state`
2. create an OAuth URL with [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client)
3. store that pending request in [IDiscordIngressPendingStateStore](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordIngressPendingStateStore)
4. redirect the user to Discord
5. let `MapDisCatSharpIngress()` process the callback and exchange the code

## Register the services

```cs
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

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

Your app is responsible for starting the OAuth flow.

```cs
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

The stored `RequestUri` is important because the callback handler uses it to preserve useful metadata such as:

- requested scope
- `integration_type`
- redirect URI consistency

## Map the callback endpoint

```cs
app.MapDisCatSharpIngress();
```

That maps the callback endpoint and lets the package:

- validate the callback query
- consume the pending state
- verify the configured redirect URI
- exchange the code through Discord
- return a structured JSON response

## Default callback response

The built-in callback endpoint returns JSON describing the result.

Successful responses include data such as:

- `status`
- `requested_scope`
- `granted_scope`
- `integration_type`
- `incoming_webhook_available`
- callback and request URI metadata

Failures return structured JSON with the error classification and detail.

## Custom callback behavior

If you need a custom success page, redirect behavior, session creation, or persistence flow, you have two main options:

1. **replace** [IDiscordOAuthCallbackResponseFactory](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordOAuthCallbackResponseFactory) to change the HTTP response emitted by the built-in endpoint
2. call [IDiscordOAuthCallbackHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordOAuthCallbackHandler) yourself from a custom ASP.NET Core route and use the returned [DiscordOAuthCallbackResult](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordOAuthCallbackResult)

That keeps the exchange/validation logic reusable while letting your application own the final user experience.

## Existing app vs self-host

The callback flow is the same in both hosting modes.

- existing ASP.NET Core app: register services and call `app.MapDisCatSharpIngress()`
- self-host mode: use `AddDisCatSharpAspNetCoreSelfHost(...)`

The shared ingress services back both approaches.
