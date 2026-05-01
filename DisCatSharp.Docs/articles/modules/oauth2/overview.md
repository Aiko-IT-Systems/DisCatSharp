---
uid: modules_oauth2_overview
title: OAuth2 Overview
author: DisCatSharp Team
---

# OAuth2 Overview

DisCatSharp's OAuth2 story now has **two layers**:

1. [DiscordOAuth2Client](xref:DisCatSharp.DiscordOAuth2Client) is the **protocol client**
2. `DisCatSharp.Hosting.AspNetCore` is the **HTTP ingress and hosting integration**

> [!IMPORTANT]
> `DiscordOAuth2Client` is **not deprecated**.
> The deprecated component is `DisCatSharp.Extensions.OAuth2Web`.

## Which piece should I use?

| Need | Use |
| --- | --- |
| Generate OAuth URLs, validate state, exchange codes, call Discord OAuth APIs | `DiscordOAuth2Client` |
| Receive OAuth callbacks inside ASP.NET Core | `DisCatSharp.Hosting.AspNetCore` |
| Receive Discord interactions over HTTP | `DisCatSharp.Hosting.AspNetCore` |
| Receive signed Discord webhook events | `DisCatSharp.Hosting.AspNetCore` |
| Reuse both the protocol client and built-in callback processing | both together |

## Common setups

### Custom web app or custom framework

If you already own your callback route and want to handle the HTTP flow yourself, you can keep using `DiscordOAuth2Client` directly.

See [DiscordOAuth2Client](xref:modules_oauth2_oauth2_client).

### ASP.NET Core app

If your bot already runs inside ASP.NET Core, use `DisCatSharp.Hosting.AspNetCore` and map the ingress endpoints into your existing app.

See:

- `Web Ingress > Overview`
- `Web Ingress > OAuth Callbacks`

### Self-hosted ingress

If your bot does **not** already own an ASP.NET Core app, the same package also exposes a self-host mode.

See `Web Ingress > Overview`.

## Why the split exists

`DiscordOAuth2Client` should stay focused on Discord's OAuth2 protocol:

- generating authorize URLs
- validating state
- exchanging authorization codes
- refreshing or revoking tokens
- calling OAuth-protected Discord endpoints

The new ingress package exists to handle the HTTP-facing concerns that do **not** belong in the protocol client:

- route mapping
- request-body preservation for signature validation
- self-hosted ASP.NET Core bootstrapping
- proxy-aware public URL generation
- ingress validation against the current Discord application state

That split keeps the client reusable while making the hosted web experience much easier to adopt.
