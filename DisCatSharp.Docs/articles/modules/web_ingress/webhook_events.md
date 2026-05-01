---
uid: modules_web_ingress_webhook_events
title: Webhook Events and Incoming Webhooks
author: DisCatSharp Team
---

# Webhook Events and Incoming Webhooks

The package currently exposes **two webhook-related routes**:

| Route | Purpose | Current state |
| --- | --- | --- |
| `/discord/webhooks/events` | signed Discord webhook event ingress | implemented |
| `/discord/webhooks/incoming` | generic incoming webhook surface | implemented with app-defined handlers |

## Webhook events

The webhook events endpoint validates Discord's request signature before acknowledging the payload.

For valid signed payloads:

- `PING` webhook envelopes are accepted
- parsed webhook events are acknowledged with `204 No Content`

For invalid or malformed requests:

- invalid signature -> `401 Unauthorized`
- malformed JSON -> `400 Bad Request`
- oversized body -> `413 Payload Too Large`

## What the package exposes today

The public webhook-event model types include:

- [DiscordWebhookEventEnvelope](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordWebhookEventEnvelope)
- [DiscordWebhookEventTypes](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordWebhookEventTypes)

These are useful if you build custom ingress or transport layers around the same concepts.

## Incoming webhooks

The `incoming webhooks` route is now a real inbound webhook surface built around transport-neutral request and response primitives.

Register one or more [IDiscordIncomingWebhookHandler](xref:DisCatSharp.Hosting.AspNetCore.Ingress.IDiscordIncomingWebhookHandler) implementations:

```csharp
using DisCatSharp.Hosting.AspNetCore;
using DisCatSharp.Hosting.AspNetCore.Ingress;

builder.Services
    .AddDisCatSharpAspNetCore()
    .AddDiscordIncomingWebhookHandler<MyWebhookHandler>();
```

Handlers receive a [DiscordIncomingWebhookContext](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordIncomingWebhookContext) with the buffered body, headers, method, and request URI, and return a [DiscordIngressResponse](xref:DisCatSharp.Hosting.AspNetCore.Ingress.DiscordIngressResponse) when they want to claim the request.

If no registered handler returns a response, the route returns `501 Not Implemented`. Oversized bodies still produce `413 Payload Too Large`.
