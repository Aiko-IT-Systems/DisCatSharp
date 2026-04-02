---
uid: topics_timeouts
title: Timeout Configuration
author: DisCatSharp Team
---

# Timeout Configuration

DisCatSharp exposes timeout boundaries at multiple levels — REST, gateway, and interactivity — so you can tune behavior for your specific workload. This article documents every configurable timeout, its default, valid range, and what it controls.

## REST Timeouts

REST timeouts live on `RestConfiguration` and `RestAdvancedConfiguration`, accessible via `DiscordConfiguration.Rest`.

| Property | Location | Default | Range | Description |
|---|---|---|---|---|
| `RequestTimeout` | `RestConfiguration` | 20 s | Positive or `Timeout.InfiniteTimeSpan` | HTTP request timeout for individual Discord API calls. |
| `QueueTimeout` | `RestAdvancedConfiguration` | 5 min | ≥ 0 (`Zero` = disabled) | Maximum time a request waits in a bucket queue before failing with `RestQueueTimeoutException`. |
| `QueueWarningThreshold` | `RestAdvancedConfiguration` | 2 min | ≥ 0 (`Zero` = disabled) | Duration after which a queued request emits a warning log. |
| `MaxRetries` | `RestAdvancedConfiguration` | 5 | ≥ 0 (0 = disabled) | Maximum automatic retries for 429/5xx responses. |
| `MaxQueueDepthPerBucket` | `RestAdvancedConfiguration` | 1000 | ≥ 0 (0 = unbounded) | Maximum queued requests per bucket before `RestQueueFullException`. |
| `CircuitBreakerThreshold` | `RestAdvancedConfiguration` | 10 | ≥ 0 (0 = disabled) | Consecutive failures before the circuit breaker opens. |
| `CircuitBreakerResetTimeout` | `RestAdvancedConfiguration` | 30 s | Positive | Duration before a tripped circuit breaker transitions to half-open. |

## Gateway Timeouts

Gateway timeouts live on `GatewayAdvancedConfiguration`, accessible via `DiscordConfiguration.Gateway.Advanced`.

| Property | Default | Range | Description |
|---|---|---|---|
| `SocketLockTimeout` | 30 s | Positive | Maximum time to hold the socket lock during IDENTIFY. |
| `ReconnectDelay` | 6 s | Positive | Base delay before attempting a gateway reconnection. |
| `HeartbeatZombieThreshold` | 5 | ≥ 1 | Missed heartbeat ACKs before the connection is considered zombied. |

## Interactivity Timeouts

Interactivity timeouts live on `InteractivityConfiguration`, set when registering the interactivity extension.

| Property | Default | Range | Description |
|---|---|---|---|
| `Timeout` | 1 min | Any `TimeSpan` | Default timeout for interactive actions (button waits, message collectors, etc.). |

## Intentionally Internal Timings

Several timing constants are **not** exposed as configuration because they are implementation details, follow standard protocols, or are security-critical:

| Timing | Value | Why It's Internal |
|---|---|---|
| BucketWorker idle grace period | 30 s | Implementation detail of the REST worker lifecycle. Changing it would not improve behavior and could cause resource leaks. |
| Exponential backoff formula | 1s × 2^attempt | Follows standard exponential backoff practice. Users control the retry *count* via `MaxRetries`, not the backoff curve. |
| Voice UDP discovery timeout | 3 s | Protocol-level timing dictated by Discord's voice infrastructure. Changing it would break IP discovery. |
| DAVE cryptor expiry | 10 s | Security-critical timing for the DAVE end-to-end encryption protocol. Must not be user-tunable to maintain security guarantees. |

## Configuration Examples

### Adjusting REST Timeouts

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token-here",
    Rest = new RestConfiguration
    {
        RequestTimeout = TimeSpan.FromSeconds(30),
        Advanced = new RestAdvancedConfiguration
        {
            QueueTimeout = TimeSpan.FromMinutes(10),
            QueueWarningThreshold = TimeSpan.FromMinutes(3),
            MaxRetries = 3,
            CircuitBreakerThreshold = 5,
            CircuitBreakerResetTimeout = TimeSpan.FromSeconds(15)
        }
    }
};
```

### Tuning Gateway Reconnection

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token-here",
    Gateway = new GatewayConfiguration
    {
        Advanced = new GatewayAdvancedConfiguration
        {
            SocketLockTimeout = TimeSpan.FromMinutes(1),
            ReconnectDelay = TimeSpan.FromSeconds(10),
            HeartbeatZombieThreshold = 3
        }
    }
};
```

### Disabling Timeouts

```csharp
// Disable queue timeout (wait forever for rate limits)
var restAdvanced = new RestAdvancedConfiguration
{
    QueueTimeout = TimeSpan.Zero
};

// Disable HTTP request timeout
var rest = new RestConfiguration
{
    RequestTimeout = System.Threading.Timeout.InfiniteTimeSpan
};

// Disable circuit breaker
var advanced = new RestAdvancedConfiguration
{
    CircuitBreakerThreshold = 0
};
```
