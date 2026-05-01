---
uid: modules_web_ingress_interactions_http
title: HTTP Interactions
author: DisCatSharp Team
---

# HTTP Interactions

`DisCatSharp.Hosting.AspNetCore` can receive Discord interactions over incoming HTTP.

The package:

- preserves the raw body for signature verification
- validates `X-Signature-Ed25519` and `X-Signature-Timestamp`
- answers `PING` automatically
- dispatches application interactions through registered [IDiscordInteractionIngressHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordInteractionIngressHandler) implementations

## Configure the verify key

```cs
builder.Services.AddDisCatSharpAspNetCore(configure: options =>
{
    options.ApplicationVerifyKey = "YOUR_DISCORD_APPLICATION_VERIFY_KEY";
});
```

Your Discord application verify key comes from the developer portal or the current application payload.

## Register a handler

```cs
using DisCatSharp.Entities;
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

builder.Services.AddDiscordInteractionIngressHandler<HelloInteractionHandler>();

public sealed class HelloInteractionHandler : IDiscordInteractionIngressHandler
{
    public ValueTask<DiscordInteractionIngressResponse?> HandleAsync(
        DiscordInteractionIngressContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Interaction.Type != InteractionType.ApplicationCommand)
            return ValueTask.FromResult<DiscordInteractionIngressResponse?>(null);

        return ValueTask.FromResult<DiscordInteractionIngressResponse?>(
            DiscordInteractionIngressResponse.ChannelMessageWithSource(
                new DiscordInteractionResponseBuilder().WithContent("Hello from HTTP ingress")));
    }
}
```

Handlers can return:

- a response to stop processing
- `null` to let another registered handler try

## Deferred responses

Discord expects the initial interaction response in roughly **three seconds**.

If your handler needs more work, respond with a deferred callback:

```cs
return ValueTask.FromResult<DiscordInteractionIngressResponse?>(
    DiscordInteractionIngressResponse.DeferredChannelMessageWithSource(ephemeral: true));
```

Then finish the interaction through Discord's normal follow-up webhook APIs before the interaction token expires.

## Map the endpoint

```cs
app.MapDisCatSharpIngress();
```

By default the interactions endpoint is:

`/discord/interactions`

You can override the route through [DiscordAspNetCoreIngressOptions](xref:DisCatSharp.Hosting.AspNetCore.DiscordAspNetCoreIngressOptions).

## Current inline response limitations

The inline helpers intentionally stay small and fast.

Today they do **not** support inline:

- file uploads
- attachment metadata payloads
- poll payloads
- modal and iframe callback helpers

If you need those, defer the initial response and complete the interaction through the normal outbound follow-up APIs.

## Relationship to gateway interactions

This feature is specifically for **incoming HTTP** interaction delivery.

It does not replace the existing gateway interaction model automatically.
Use it when your Discord application is configured to deliver interactions to an HTTP endpoint instead of relying only on the gateway path.
