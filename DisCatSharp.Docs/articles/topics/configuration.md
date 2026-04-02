---
uid: topics_configuration
title: Configuration Layout
author: DisCatSharp Team
---

# Configuration Layout

Starting with **DisCatSharp 10.7.0**, `DiscordConfiguration` uses a nested layout that groups related settings into focused sub-objects. This replaces the previous flat class with 40+ root properties.

This guide explains the new structure, lists every property, and shows how to migrate.

---

## Overview

```
DiscordConfiguration
├── Token / TokenType / Intents / ServiceProvider / Proxy
├── Api            → API protocol settings
├── Gateway        → Connection, sharding, compression
│   └── Advanced   → Dispatch mode, reconnect tuning
├── Rest           → HTTP timeout, rate-limit strategy
│   └── Advanced   → Queue limits, retries, circuit breaker
├── Cache          → Message/presence cache, auto-fetch
├── Logging        → Log level, timestamp format, logger factory
├── Diagnostics    → Payload events, update checks
│   └── UpdateChecks → Version check mode and options
└── Telemetry      → Sentry error reporting
```

---

## Root Properties

These live directly on `DiscordConfiguration` and are **not** deprecated:

| Property | Type | Default | Description |
|---|---|---|---|
| `Token` | `string?` | `null` | Bot or bearer token. |
| `TokenType` | `TokenType` | `Bot` | Authentication type. |
| `Intents` | `DiscordIntents` | `AllUnprivileged` | Gateway intents filter. |
| `ServiceProvider` | `IServiceProvider` | Empty | Dependency injection container. |
| `Proxy` | `IWebProxy?` | `null` | HTTP and WebSocket proxy. |

---

## Api

API protocol settings. Access via `config.Api`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Api = new ApiConfiguration
    {
        Version = "10",
        Channel = ApiChannel.Stable,
        Locale = DiscordLocales.AMERICAN_ENGLISH
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `Version` | `string` | `"10"` | Discord API version. |
| `Channel` | `ApiChannel` | `Stable` | API channel (Stable, Canary, PTB). |
| `Override` | `string?` | `null` | Developer-only URL override. |
| `Locale` | `string` | `AMERICAN_ENGLISH` | API locale for responses. |
| `Timezone` | `string?` | `null` | User timezone preference. |

---

## Gateway

Connection and sharding settings. Access via `config.Gateway`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Gateway = new GatewayConfiguration
    {
        AutoReconnect = true,
        ShardCount = 4,
        LargeThreshold = 250,
        CompressionLevel = GatewayCompressionLevel.Stream
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `AutoReconnect` | `bool` | `true` | Reconnect automatically on connection loss. |
| `ReconnectIndefinitely` | `bool` | `false` | Retry forever (not recommended for production). |
| `ShardId` | `int` | `0` | Current shard ID (≥ 0, < ShardCount). |
| `ShardCount` | `int` | `1` | Total shard count (> 0). |
| `CompressionLevel` | `GatewayCompressionLevel` | `Stream` | WebSocket compression (None, Payload, Stream). |
| `LargeThreshold` | `int` | `250` | Guild large-member threshold (clamped 50–250). |
| `Capabilities` | `GatewayCapabilities` | `None` | Gateway capability flags. |
| `MobileStatus` | `bool` | `false` | Show mobile status indicator. |
| `WebSocketClientFactory` | `WebSocketClientFactoryDelegate` | Built-in | WebSocket implementation factory. |
| `UdpClientFactory` | `UdpClientFactoryDelegate` | Built-in | UDP implementation factory. |
| `Advanced` | `GatewayAdvancedConfiguration` | *(defaults)* | Low-level transport tuning. |

### Gateway.Advanced

Access via `config.Gateway.Advanced`.

| Property | Type | Default | Description |
|---|---|---|---|
| `DispatchMode` | `GatewayDispatchMode` | `ConcurrentHandlers` | How user event handlers are scheduled. See [Gateway Dispatch](xref:topics_gateway_dispatch). |
| `DispatchQueueCapacity` | `int` | `10,000` | Per-shard dispatch queue size (0 = unbounded). |
| `SocketLockTimeout` | `TimeSpan` | 30 s | Max wait for IDENTIFY response. |
| `ReconnectDelay` | `TimeSpan` | 6 s | Delay before reconnection attempt. |
| `HeartbeatZombieThreshold` | `int` | `5` | Missed ACKs before connection is considered dead. |

---

## Rest

HTTP client and rate-limit settings. Access via `config.Rest`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Rest = new RestConfiguration
    {
        RequestTimeout = TimeSpan.FromSeconds(30),
        UseRelativeRatelimit = true,
        Advanced = new RestAdvancedConfiguration
        {
            MaxRetries = 3,
            CircuitBreakerThreshold = 5
        }
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `RequestTimeout` | `TimeSpan` | 20 s | HTTP request timeout. `Timeout.InfiniteTimeSpan` to disable. |
| `UseRelativeRatelimit` | `bool` | `true` | Use `X-Ratelimit-Reset-After` instead of absolute timestamps. |
| `Advanced` | `RestAdvancedConfiguration` | *(defaults)* | Queue, retry, and circuit breaker tuning. |

### Rest.Advanced

Access via `config.Rest.Advanced`. For full documentation, see [REST Client Architecture](xref:topics_rest) and [Timeout Configuration](xref:topics_timeouts).

| Property | Type | Default | Description |
|---|---|---|---|
| `QueueTimeout` | `TimeSpan` | 5 min | Max wait in bucket queue (0 = unlimited). |
| `QueueWarningThreshold` | `TimeSpan` | 2 min | Log warning after this wait (0 = disabled). |
| `MaxRetries` | `int` | `5` | Retries for 429/5xx responses (0 = disabled). |
| `MaxQueueDepthPerBucket` | `int` | `1000` | Max queued requests per bucket (0 = unbounded). |
| `RetryTransientErrors` | `bool` | `true` | Retry DNS/socket/timeout errors with backoff. |
| `CircuitBreakerThreshold` | `int` | `10` | Failures before circuit opens (0 = disabled). |
| `CircuitBreakerResetTimeout` | `TimeSpan` | 30 s | Cooldown before half-open probe. |

---

## Cache

Caching behavior. Access via `config.Cache`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Cache = new CacheConfiguration
    {
        MessageCacheSize = 2048,
        AlwaysCacheMembers = true,
        AutoRefreshChannelCache = true
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `MessageCacheSize` | `int` | `1024` | Global message cache size (0 = disabled). |
| `PresenceCacheSize` | `int` | `0` | Max cached presences (0 = uncapped). |
| `AlwaysCacheMembers` | `bool` | `true` | Cache members even without GuildMembers intent. |
| `AutoRefreshChannelCache` | `bool` | `false` | Refresh full guild channel cache on startup. |
| `AutoFetchApplicationEmojis` | `bool` | `false` | Fetch application emojis on startup. |
| `AutoFetchSkuIds` | `bool` | `false` | Auto-fetch SKU IDs (monetized apps). |
| `SkuId` | `ulong?` | `null` | Manual SKU ID (mutually exclusive with AutoFetchSkuIds). |

---

## Logging

Logging behavior. Access via `config.Logging`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Logging = new LoggingConfiguration
    {
        MinimumLogLevel = LogLevel.Debug,
        LogTimestampFormat = "HH:mm:ss"
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `MinimumLogLevel` | `LogLevel` | `Information` | Minimum severity to log. |
| `LogTimestampFormat` | `string` | `"yyyy-MM-dd HH:mm:ss zzz"` | Timestamp format for the built-in logger. |
| `LoggerFactory` | `ILoggerFactory` | Built-in | Custom logger factory. See [Logging](xref:topics_logging_default). |

---

## Diagnostics

Debugging and diagnostic features. Access via `config.Diagnostics`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Diagnostics = new DiagnosticsConfiguration
    {
        EnablePayloadReceivedEvent = true,
        UpdateChecks = new UpdateCheckConfiguration
        {
            Mode = VersionCheckMode.NuGet,
            IncludePrerelease = false
        }
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `EnablePayloadReceivedEvent` | `bool` | `false` | Fire `PayloadReceived` event for raw gateway packets. |
| `UpdateChecks` | `UpdateCheckConfiguration` | *(defaults)* | Library version checking options. |

### Diagnostics.UpdateChecks

| Property | Type | Default | Description |
|---|---|---|---|
| `Disabled` | `bool` | `false` | Disable update checks entirely. |
| `Mode` | `VersionCheckMode` | `NuGet` | Check against NuGet or GitHub. |
| `IncludePrerelease` | `bool` | `true` | Include pre-releases in the version check. |
| `GitHubToken` | `string?` | `null` | GitHub token for private repos. |
| `ShowReleaseNotes` | `bool` | `false` | Display release notes in the check result. |

---

## Telemetry

Sentry error reporting and telemetry. Access via `config.Telemetry`.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Telemetry = new TelemetryConfiguration
    {
        EnableSentry = true,
        AttachUserInfo = true,
        FeedbackEmail = "me@example.com"
    }
};
```

| Property | Type | Default | Description |
|---|---|---|---|
| `EnableSentry` | `bool` | `false` | Enable Sentry error reporting. |
| `AttachRecentLogEntries` | `bool` | `false` | Include recent log entries in Sentry reports. |
| `AttachUserInfo` | `bool` | `false` | Include bot username/ID in reports. |
| `FeedbackEmail` | `string?` | `null` | Developer contact email (requires AttachUserInfo). |
| `DeveloperUserId` | `ulong?` | `null` | Developer Discord user ID (requires AttachUserInfo). |
| `EnableDiscordIdScrubber` | `bool` | `false` | Replace Discord IDs with `{DISCORD_ID}` in reports. |

---

## Full Example

A complete configuration using the nested layout:

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,

    Api =
    {
        Channel = ApiChannel.Stable,
        Locale = DiscordLocales.AMERICAN_ENGLISH
    },

    Gateway =
    {
        AutoReconnect = true,
        ShardCount = 1,
        CompressionLevel = GatewayCompressionLevel.Stream,
        Advanced =
        {
            DispatchMode = GatewayDispatchMode.ConcurrentHandlers,
            HeartbeatZombieThreshold = 5
        }
    },

    Rest =
    {
        RequestTimeout = TimeSpan.FromSeconds(20),
        Advanced =
        {
            MaxRetries = 5,
            CircuitBreakerThreshold = 10
        }
    },

    Cache =
    {
        MessageCacheSize = 1024,
        AlwaysCacheMembers = true
    },

    Logging =
    {
        MinimumLogLevel = LogLevel.Information
    },

    Diagnostics =
    {
        UpdateChecks =
        {
            Mode = VersionCheckMode.NuGet
        }
    },

    Telemetry =
    {
        EnableSentry = true,
        AttachUserInfo = true
    }
};
```

---

## Migration from Flat Properties

The old flat properties (e.g., `config.HttpTimeout`, `config.ApiChannel`, `config.MessageCacheSize`) still work via `[Deprecated]` forwarding but will be removed in a future release.

### Before (flat)

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    HttpTimeout = TimeSpan.FromSeconds(30),
    ApiChannel = ApiChannel.Canary,
    AutoReconnect = true,
    MessageCacheSize = 2048,
    MinimumLogLevel = LogLevel.Debug,
    EnableSentry = true
};
```

### After (nested)

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Api = { Channel = ApiChannel.Canary },
    Gateway = { AutoReconnect = true },
    Rest = { RequestTimeout = TimeSpan.FromSeconds(30) },
    Cache = { MessageCacheSize = 2048 },
    Logging = { MinimumLogLevel = LogLevel.Debug },
    Telemetry = { EnableSentry = true }
};
```

### Mapping Reference

| Old Property | New Location |
|---|---|
| `ApiVersion` | `Api.Version` |
| `ApiChannel` | `Api.Channel` |
| `Override` | `Api.Override` |
| `Locale` | `Api.Locale` |
| `Timezone` | `Api.Timezone` |
| `AutoReconnect` | `Gateway.AutoReconnect` |
| `ReconnectIndefinitely` | `Gateway.ReconnectIndefinitely` |
| `ShardId` | `Gateway.ShardId` |
| `ShardCount` | `Gateway.ShardCount` |
| `GatewayCompressionLevel` | `Gateway.CompressionLevel` |
| `LargeThreshold` | `Gateway.LargeThreshold` |
| `Capabilities` | `Gateway.Capabilities` |
| `MobileStatus` | `Gateway.MobileStatus` |
| `WebSocketClientFactory` | `Gateway.WebSocketClientFactory` |
| `UdpClientFactory` | `Gateway.UdpClientFactory` |
| `HttpTimeout` | `Rest.RequestTimeout` |
| `UseRelativeRatelimit` | `Rest.UseRelativeRatelimit` |
| `MessageCacheSize` | `Cache.MessageCacheSize` |
| `PresenceCacheSize` | `Cache.PresenceCacheSize` |
| `AlwaysCacheMembers` | `Cache.AlwaysCacheMembers` |
| `AutoRefreshChannelCache` | `Cache.AutoRefreshChannelCache` |
| `AutoFetchApplicationEmojis` | `Cache.AutoFetchApplicationEmojis` |
| `AutoFetchSkuIds` | `Cache.AutoFetchSkuIds` |
| `SkuId` | `Cache.SkuId` |
| `MinimumLogLevel` | `Logging.MinimumLogLevel` |
| `LogTimestampFormat` | `Logging.LogTimestampFormat` |
| `LoggerFactory` | `Logging.LoggerFactory` |
| `EnablePayloadReceivedEvent` | `Diagnostics.EnablePayloadReceivedEvent` |
| `DisableUpdateCheck` | `Diagnostics.UpdateChecks.Disabled` |
| `UpdateCheckMode` | `Diagnostics.UpdateChecks.Mode` |
| `IncludePrereleaseInUpdateCheck` | `Diagnostics.UpdateChecks.IncludePrerelease` |
| `UpdateCheckGitHubToken` | `Diagnostics.UpdateChecks.GitHubToken` |
| `ShowReleaseNotesInUpdateCheck` | `Diagnostics.UpdateChecks.ShowReleaseNotes` |
| `EnableSentry` | `Telemetry.EnableSentry` |
| `AttachRecentLogEntries` | `Telemetry.AttachRecentLogEntries` |
| `AttachUserInfo` | `Telemetry.AttachUserInfo` |
| `FeedbackEmail` | `Telemetry.FeedbackEmail` |
| `DeveloperUserId` | `Telemetry.DeveloperUserId` |
| `EnableDiscordIdScrubber` | `Telemetry.EnableDiscordIdScrubber` |

---

## Analyzer Support

The **DCS1201** analyzer rule detects usages of deprecated flat properties and offers one-click code fixes:

- **Single-property fix** — rewrites one property access to its nested path (e.g., `config.ApiChannel` → `config.Api.Channel`).
- **Batch fix** (initializers only) — rewrites an entire `new DiscordConfiguration { ... }` initializer at once, grouping properties by section.

Enable the `DisCatSharp.Analyzer` package to see warnings in your IDE. The code fixes work in Visual Studio, Rider, and VS Code with the C# extension.

> [!TIP]
> Use the batch fix on object initializers to migrate all flat properties in one click. The analyzer groups them by section automatically.
