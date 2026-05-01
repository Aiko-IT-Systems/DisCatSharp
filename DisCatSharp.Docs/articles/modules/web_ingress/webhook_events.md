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
| `/discord/webhooks/incoming` | generic incoming webhook surface | placeholder (`501`) |

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

## Current limitation

> [!IMPORTANT]
> The webhook-events endpoint is implemented, but there is not yet a full public customization surface for application-level webhook event handling comparable to `IDiscordInteractionIngressHandler`.

So right now this article should be read as:

- what routes exist
- what security behavior the package provides
- what payload model types exist
- what the current limitation is

## Incoming webhooks

The `incoming webhooks` route is currently reserved for a future, more complete inbound webhook surface.

Today it returns `501 Not Implemented`.

That means:

- do **not** build production incoming webhook receivers on that route yet
- do use the package today for signed webhook events, interactions, and OAuth callbacks

The remaining work for that route is to add:

- a request model
- a public handler abstraction
- response shaping
- examples for real application usage
