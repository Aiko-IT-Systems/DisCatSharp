---
uid: modules_web_ingress_migration_oauth2web
title: Migrating from OAuth2Web
author: DisCatSharp Team
---

# Migrating from OAuth2Web

`DisCatSharp.Extensions.OAuth2Web` is deprecated in favor of `DisCatSharp.Hosting.AspNetCore`.

> [!IMPORTANT]
> `DiscordOAuth2Client` is still the supported protocol client.
> The migration is about moving away from the old **web extension architecture**, not about removing OAuth2 support.

## What changed

The old extension was:

- extension-based
- self-host-first
- shard-oriented
- OAuth-focused

The new package is:

- first-party
- ASP.NET-Core-first
- app-level by default
- broader than OAuth, covering interactions and signed webhook events too

## Old API to new API

| Old | New |
| --- | --- |
| `UseOAuth2Web(...)` | `AddDisCatSharpAspNetCore(...)` |
| `UseOAuth2WebAsync(...)` | `AddDisCatSharpAspNetCore(...)` or `AddDisCatSharpAspNetCoreSelfHost(...)` |
| `Start()` / `StopAsync()` | `app.MapDisCatSharpIngress()` or host-managed self-hosting |
| Apache-only proxy helper | `DiscordIngressProxyHelpers` |
| redirect-uri checks on the extension | `DiscordIngressConfigurationValidator` |
| per-client/per-shard web extension model | app-level ingress services |

## Existing ASP.NET Core app

If your application already owns an ASP.NET Core app, the migration is usually:

1. remove the old extension registration
2. register `AddDisCatSharpAspNetCore(...)`
3. map `app.MapDisCatSharpIngress()`
4. update your public URL and portal config

## Self-hosted bot

If you were relying on the extension's internal web server, use:

`AddDisCatSharpAspNetCoreSelfHost(...)`

This keeps the convenience of self-hosting while moving the architecture into the main repo and package family.

## OAuth request initiation still belongs to your app

The new package handles the callback leg, but your application still starts the flow by:

- generating a `state`
- generating the authorize URL
- storing the pending state entry
- redirecting the user to Discord

That is intentional: it keeps the protocol client reusable and the HTTP ingress layer focused.

## New capability you get during migration

Migrating also unlocks related ingress features in the same package:

- HTTP interactions
- signed webhook events
- public URL helpers
- proxy generation
- validation against current Discord application settings
