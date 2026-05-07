---
uid: modules_web_ingress_webhook_events
title: Webhook Events vs Incoming Webhooks
author: DisCatSharp Team
---

# Webhook Events vs Incoming Webhooks

The package exposes **two different webhook-related routes**. They are not interchangeable:

| Route | Use this when | How it works |
| --- | --- | --- |
| `/discord/webhooks/events` | Discord sends a signed webhook event defined by Discord | DisCatSharp validates the Discord signature and accepts Discord webhook event envelopes |
| `/discord/webhooks/incoming` | your app needs a generic webhook endpoint | DisCatSharp passes the request to registered [IDiscordIncomingWebhookHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks.IDiscordIncomingWebhookHandler) implementations |

## Webhook events

`/discord/webhooks/events` is the Discord-defined webhook event ingress route.

It validates Discord's request signature before acknowledging the payload.

For valid signed payloads:

- `PING` webhook envelopes are accepted
- parsed webhook events are acknowledged with `204 No Content`

For invalid or malformed requests:

- invalid signature -> `401 Unauthorized`
- malformed JSON -> `400 Bad Request`
- oversized body -> `413 Payload Too Large`

## Typed webhook events

The package exposes a singleton [DiscordWebhookEventDispatcher](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventDispatcher) that raises typed async events after signature validation and envelope parsing succeed.

The split is:

- `DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents` for the envelope models and dispatcher
- `DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook` for the event-args types you subscribe to

Resolve it from DI and subscribe during application startup:

```csharp
using DisCatSharp.Hosting.AspNetCore;

var dispatcher = app.Services.GetRequiredService<DiscordWebhookEventDispatcher>();
dispatcher.ApplicationAuthorized += async (_, eventArgs) =>
{
    Console.WriteLine($"Authorized by {eventArgs.Authorization.User?.Username}");
};
```

The dispatcher keeps the HTTP acknowledgement path fast by dispatching the webhook event asynchronously after ingress accepts the signed payload.

## Webhook event model types

The public webhook-event model types include:

- [DiscordWebhookEventEnvelope](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventEnvelope)
- [DiscordWebhookEventTypes](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventTypes)
- [DiscordWebhookEventNames](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventNames)
- [DiscordWebhookEventModelRegistry](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventModelRegistry)

These are useful if you build custom ingress or transport layers around the same concepts, or if you need to inspect the raw envelope from event handlers.

## Core-style async event surface

Webhook event args live under the `DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook` namespace and inherit from a shared [DiscordWebhookEventArgs](xref:DisCatSharp.Hosting.AspNetCore.EventArgs.Webhook.DiscordWebhookEventArgs) base.

Each event args instance carries:

- the transport-neutral ingress request
- the parsed [DiscordWebhookEventEnvelope](xref:DisCatSharp.Hosting.AspNetCore.Ingress.WebhookEvents.DiscordWebhookEventEnvelope)
- the typed payload for that event family

### Payload property map

The event args types expose the typed payload through these C# properties:

| Event | Event args | Read the payload from |
| --- | --- | --- |
| `ApplicationAuthorized` | `WebhookApplicationAuthorizedEventArgs` | `Authorization` |
| `ApplicationDeauthorized` | `WebhookApplicationDeauthorizedEventArgs` | `Deauthorization` |
| `EntitlementCreated` | `WebhookEntitlementCreateEventArgs` | `Entitlement` |
| `EntitlementUpdated` | `WebhookEntitlementUpdateEventArgs` | `Entitlement` |
| `EntitlementDeleted` | `WebhookEntitlementDeleteEventArgs` | `Entitlement` |
| `LobbyMessageCreated` | `WebhookLobbyMessageCreateEventArgs` | `Message` |
| `LobbyMessageUpdated` | `WebhookLobbyMessageUpdateEventArgs` | `Message` |
| `LobbyMessageDeleted` | `WebhookLobbyMessageDeleteEventArgs` | `Message` |
| `GameDirectMessageCreated` | `WebhookGameDirectMessageCreateEventArgs` | `Message` |
| `GameDirectMessageUpdated` | `WebhookGameDirectMessageUpdateEventArgs` | `Message` |
| `GameDirectMessageDeleted` | `WebhookGameDirectMessageDeleteEventArgs` | `Message` |
| `UnknownWebhookEventReceived` | `UnknownWebhookEventArgs` | `Envelope` and `Request` on the base type |

Prefixing the event-args names with `Webhook` keeps them visually grouped with the ingress package and avoids collisions with existing core event args such as `EntitlementCreateEventArgs`.

Example:

```csharp
dispatcher.ApplicationDeauthorized += async (_, eventArgs) =>
{
    Console.WriteLine(eventArgs.Deauthorization.User?.Id);
};

dispatcher.LobbyMessageCreated += async (_, eventArgs) =>
{
    Console.WriteLine($"{eventArgs.Message.LobbyId}: {eventArgs.Message.Content}");
};

dispatcher.GameDirectMessageCreated += async (_, eventArgs) =>
{
    Console.WriteLine(eventArgs.Message.RecipientId);
};
```

### Unknown/raw fallback behavior

- keep acknowledging any valid signed webhook payload, even when no typed mapping exists yet
- raise the matching typed async event when the event type is known and deserialization succeeds
- raise `UnknownWebhookEventReceived` only when the event type is unmapped or typed deserialization cannot be completed yet
- do not fire both a typed event and the unknown fallback for the same payload
- always include the original `DiscordWebhookEventEnvelope` on the event args so advanced callers can inspect raw JSON without opting out of the typed surface

## Incoming webhooks

`/discord/webhooks/incoming` is not the Discord webhook-events endpoint.

It is a generic inbound webhook surface built around transport-neutral request and response primitives. Your application decides what the webhook means by registering one or more handlers.

Register one or more [IDiscordIncomingWebhookHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks.IDiscordIncomingWebhookHandler) implementations:

```csharp
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks;

builder.Services
    .AddDisCatSharpAspNetCore()
    .AddDiscordIncomingWebhookHandler<MyWebhookHandler>();
```

Handlers receive a [DiscordIncomingWebhookContext](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IncomingWebhooks.DiscordIncomingWebhookContext) with the buffered body, headers, method, and request URI, and return a [DiscordIngressResponse](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordIngressResponse) when they want to claim the request.

If no registered handler returns a response, the route returns `501 Not Implemented`. Oversized bodies still produce `413 Payload Too Large`.

For the package-level architecture, see [Web Ingress Overview](overview.md).
