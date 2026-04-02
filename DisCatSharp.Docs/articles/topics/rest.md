---
uid: topics_rest
title: REST Client Architecture
author: DisCatSharp Team
---

# REST Client Architecture

## Overview

DisCatSharp sends every HTTP request to Discord through a per-bucket FIFO queue/worker system. Each Discord rate-limit bucket gets its own [BucketWorker](xref:DisCatSharp.Net.BucketWorker) that processes requests sequentially, enforcing rate limits without blocking other buckets. Buckets that target different endpoints execute independently and concurrently.

This design means you can fire requests at many different endpoints simultaneously without one slow or rate-limited endpoint stalling others.

## Architecture

```
Your Code
   │
   ▼
DiscordApiClient        (strongly-typed API methods)
   │
   ▼
RestClient              (FIFO dispatch, global rate-limit gate)
   │
   ▼
BucketRegistry          (route → hash → bucket → worker mapping)
   │
   ▼
BucketWorker(s)         (one per rate-limit bucket, independent queues)
   │
   ▼
HttpClient              (actual HTTP calls)
   │
   ▼
Discord API
```

1. **Your code** calls a high-level method like `channel.SendMessageAsync(...)`.
2. **DiscordApiClient** builds the REST request with the correct route, payload, and headers.
3. **RestClient** resolves the request's rate-limit bucket and hands it to the appropriate **BucketWorker**.
4. **BucketWorker** queues the request FIFO and sends it when the rate-limit window allows.
5. If Discord returns a new bucket hash, **BucketRegistry** transparently remaps future requests.

## Configuration

The REST system is configured through two levels on [DiscordConfiguration](xref:DisCatSharp.DiscordConfiguration):

- `DiscordConfiguration.Rest` — main REST settings like request timeout and rate-limit strategy.
- `DiscordConfiguration.Rest.Advanced` — low-level tuning via [RestAdvancedConfiguration](xref:DisCatSharp.RestAdvancedConfiguration).

### Full Example

```cs
var config = new DiscordConfiguration
{
    Token = "your-token",
    Rest =
    {
        RequestTimeout = TimeSpan.FromSeconds(20),
        UseRelativeRatelimit = true,
        Advanced =
        {
            QueueTimeout = TimeSpan.FromMinutes(5),
            QueueWarningThreshold = TimeSpan.FromMinutes(2),
            MaxRetries = 5,
            MaxQueueDepthPerBucket = 1000,
            RetryTransientErrors = true,
            CircuitBreakerThreshold = 10,
            CircuitBreakerResetTimeout = TimeSpan.FromSeconds(30)
        }
    }
};
```

### RestConfiguration Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `RequestTimeout` | `TimeSpan` | 20 seconds | Timeout for individual HTTP requests. Set to `Timeout.InfiniteTimeSpan` to disable. |
| `UseRelativeRatelimit` | `bool` | `true` | Use Discord's `X-Ratelimit-Reset-After` header instead of absolute reset timestamps. Recommended when the system clock may be out of sync. |
| `Advanced` | `RestAdvancedConfiguration` | *(defaults)* | Low-level tuning options (see below). |

### RestAdvancedConfiguration Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `QueueTimeout` | `TimeSpan` | 5 minutes | Maximum time a request waits in queue before failing with `RestQueueTimeoutException`. Set to `TimeSpan.Zero` for unlimited. |
| `QueueWarningThreshold` | `TimeSpan` | 2 minutes | Duration after which a warning is logged for slow-queued requests. Set to `TimeSpan.Zero` to disable. |
| `MaxRetries` | `int` | 5 | Maximum automatic retries for 429 and 5xx responses. Set to 0 to disable retries. |
| `MaxQueueDepthPerBucket` | `int` | 1000 | Maximum queued requests per bucket. 0 = unbounded. Exceeding throws `RestQueueFullException`. |
| `RetryTransientErrors` | `bool` | `true` | Whether DNS/socket/timeout errors retry with exponential backoff. `false` = fail immediately. |
| `CircuitBreakerThreshold` | `int` | 10 | Consecutive failures before the circuit opens. 0 = disabled. |
| `CircuitBreakerResetTimeout` | `TimeSpan` | 30 seconds | Time after which a tripped circuit allows a single probe request. |

> [!TIP]
> The defaults are tuned for typical Discord bots. You usually only need to adjust these if you're running a high-traffic bot or need tighter failure handling.

## Queue and Worker Behavior

Each Discord rate-limit bucket gets a dedicated `BucketWorker`:

- **Workers spawn on first request** — no worker exists until a request targets that bucket.
- **Workers shut down after an idle period** (30 seconds by default) to avoid holding resources.
- **Requests are processed FIFO** within each bucket — order is preserved.
- **Different buckets execute independently** — a rate-limited `POST /channels/123/messages` bucket does not block `GET /guilds/456`.
- **Queue timeout** — requests that sit in the queue longer than `QueueTimeout` are failed with [RestQueueTimeoutException](xref:DisCatSharp.Exceptions.RestQueueTimeoutException).

> [!NOTE]
> Worker lifecycle is fully automatic. You don't need to manage, start, or stop workers manually. The `BucketRegistry` handles creation and cleanup.

## Rate Limiting

DisCatSharp's rate limiting operates at multiple levels:

### Preemptive Rate Limiting

The remaining request count is tracked from Discord's response headers. When the remaining count reaches zero, the worker waits for the reset window *before* sending the next request — avoiding unnecessary 429s.

### Per-Bucket Tracking

Each bucket independently tracks its own remaining count and reset time. A rate limit on one bucket has no effect on other buckets.

### Global Rate Limit

Discord's global rate limit applies across all endpoints simultaneously. When a global 429 is received, a shared gate blocks **all** bucket workers until the global reset elapses.

### Hash Remapping

Discord may return a different bucket hash than expected for a route. When this happens, the `BucketRegistry` atomically remaps the route to the new bucket and migrates queued work — this is transparent to your code.

> [!WARNING]
> If `UseRelativeRatelimit` is set to `false` and your system clock is out of sync with Discord's servers, you may experience unexpected rate-limit hits. Keep it set to `true` unless you're certain your clock is NTP-synced.

## Retry Behavior

The REST client automatically retries failed requests in three categories:

### Rate Limits (429)

When Discord returns a 429 (Too Many Requests), the client reads the `Retry-After` header and waits for exactly that duration before retrying.

### Server Errors (5xx)

Responses with status codes 500, 502, 503, or 504 are retried with exponential backoff: 1 second, 2 seconds, 4 seconds, and so on.

### Transient Network Errors

DNS failures, socket errors, and connection timeouts are retried with exponential backoff — but only if `RetryTransientErrors` is `true` (the default). Permanent errors like 404 or 403 always fail immediately.

All three categories respect the `MaxRetries` limit. After exhausting retries, the original error propagates to the caller.

```
Attempt 1 → 429 (Retry-After: 2s)  → wait 2s
Attempt 2 → 502                     → wait 1s (exponential backoff)
Attempt 3 → 502                     → wait 2s
Attempt 4 → 200 OK ✓
```

> [!NOTE]
> Setting `MaxRetries = 0` disables all automatic retries. Every failure will propagate immediately.

## Circuit Breaker

The circuit breaker protects against cascading failures on a per-bucket basis.

### How It Works

1. **Closed** (normal) — requests flow through. Each failure increments a consecutive-failure counter.
2. **Open** — after `CircuitBreakerThreshold` consecutive failures, the circuit opens. New requests fail immediately with [RestCircuitBrokenException](xref:DisCatSharp.Exceptions.RestCircuitBrokenException) without making HTTP calls.
3. **Half-Open** — after `CircuitBreakerResetTimeout` elapses, one probe request is allowed through.
4. **Probe succeeds** → circuit resets to Closed, counter resets to zero.
5. **Probe fails** → circuit stays Open, timeout restarts.

### Disabling the Circuit Breaker

```cs
Advanced =
{
    CircuitBreakerThreshold = 0
}
```

> [!WARNING]
> Disabling the circuit breaker means a failing endpoint will keep sending HTTP requests indefinitely (up to `MaxRetries` per request). This can amplify load on an already struggling Discord endpoint.

## Queue Depth Limits

The `MaxQueueDepthPerBucket` setting protects against out-of-memory conditions from runaway request loops.

When a bucket's queue reaches the configured limit, new requests are immediately rejected with [RestQueueFullException](xref:DisCatSharp.Exceptions.RestQueueFullException) — no queueing, no waiting.

```cs
// A tight limit for safety
Advanced =
{
    MaxQueueDepthPerBucket = 200
}
```

Set to `0` for an unbounded queue.

> [!WARNING]
> Unbounded queues (`MaxQueueDepthPerBucket = 0`) risk OOM in production if a code path enters a request loop. The default of 1000 is generous for most bots.

## Diagnostics API

The [IRestDiagnostics](xref:DisCatSharp.Net.IRestDiagnostics) interface exposes runtime metrics for the REST subsystem. Access it via `client.RestDiagnostics`.

### Usage

```cs
IRestDiagnostics diag = client.RestDiagnostics;

// High-level overview
Console.WriteLine($"Active workers: {diag.ActiveWorkerCount}");
Console.WriteLine($"Total queued:   {diag.TotalQueuedRequests}");

// Per-bucket details
foreach (var bucket in diag.GetBucketSnapshots())
{
    Console.WriteLine(
        $"[{bucket.BucketId}] Queue: {bucket.QueueLength}, " +
        $"Processed: {bucket.Processed}, Retried: {bucket.Retried}, " +
        $"TimedOut: {bucket.TimedOut}, Cancelled: {bucket.Cancelled}, " +
        $"Failures: {bucket.ConsecutiveFailures}, " +
        $"Alive: {bucket.IsAlive}, Faulted: {bucket.IsFaulted}");
}
```

### BucketDiagnostics Fields

| Field | Type | Description |
|---|---|---|
| `BucketId` | `string` | The bucket identifier string. |
| `QueueLength` | `int` | Current number of queued requests. |
| `Processed` | `long` | Total requests processed (completed + faulted). |
| `Retried` | `long` | Total retry attempts. |
| `TimedOut` | `long` | Total requests that timed out in queue. |
| `Cancelled` | `long` | Total requests cancelled before execution. |
| `ConsecutiveFailures` | `int` | Current consecutive failure count (circuit breaker). |
| `IsAlive` | `bool` | Whether the worker loop task is still running. |
| `IsFaulted` | `bool` | Whether the worker loop task has faulted. |

> [!TIP]
> For high-traffic bots, consider logging `GetBucketSnapshots()` periodically (e.g. every 60 seconds) to catch queue buildup before it becomes a problem.

## Emergency Controls

The `CancelAllPendingRequests` method drains all bucket queues and fails every pending request with `OperationCanceledException`:

```cs
client.ApiClient.Rest.CancelAllPendingRequests("Emergency shutdown");
```

**When to use:**

- Bot shutdown — cancel remaining work before disposing the client.
- Emergency stop — a stuck queue or cascading failures need immediate clearing.
- Recovery — after resolving a systemic issue, cancel stale requests and let fresh ones flow.

> [!WARNING]
> This cancels **all** pending requests across **all** buckets. Callers awaiting those requests will receive `OperationCanceledException`. Use it only when you genuinely need to discard all in-flight work.

## Exception Reference

| Exception | When | Resolution |
|---|---|---|
| `RestQueueTimeoutException` | Request waited in queue longer than `QueueTimeout`. | Increase `QueueTimeout` or reduce request volume. |
| `RestQueueFullException` | Bucket queue exceeded `MaxQueueDepthPerBucket`. | Throttle request rate or increase the limit. |
| `RestCircuitBrokenException` | Circuit breaker tripped after consecutive failures. | Wait for the reset timeout or investigate endpoint health. |
| `RateLimitException` | Discord returned 429 after exhausting retries. | Reduce request rate; check for missing rate-limit intents. |

### Exception Properties

Each REST exception carries contextual information to help diagnose the issue:

**RestQueueTimeoutException:**
- `Route` — the API route (e.g., `POST:/channels/channel_id/messages`)
- `BucketId` — the rate-limit bucket identifier
- `WaitedDuration` — how long the request waited
- `QueueLength` — requests still queued at the time of timeout
- `GlobalGateActive` — whether the global rate-limit gate was blocking

**RestQueueFullException:**
- `Route` — the API route
- `BucketId` — the rate-limit bucket identifier
- `QueueDepth` — current queue depth at rejection
- `MaxDepth` — the configured maximum

**RestCircuitBrokenException:**
- `Route` — the API route
- `BucketId` — the rate-limit bucket identifier
- `ConsecutiveFailures` — number of failures that tripped the circuit
- `OpenSince` — when the circuit was opened

## Best Practices

> [!TIP]
> **Don't set `QueueTimeout` to zero in production.** A zero timeout disables the safety net — requests can queue indefinitely if something goes wrong. The 5-minute default catches stuck queues while allowing normal rate-limit back-off.

> [!TIP]
> **Monitor diagnostics in high-traffic bots.** Log `RestDiagnostics.GetBucketSnapshots()` periodically to catch queue buildup, rising failure counts, or faulted workers before they impact users.

> [!TIP]
> **Keep the circuit breaker enabled.** It catches cascading failures early by stopping requests to broken endpoints. The default threshold of 10 is forgiving enough to tolerate transient blips.

> [!TIP]
> **Use `MaxQueueDepthPerBucket` to prevent OOM.** The default of 1000 is generous for most bots. If you're running a very high-traffic bot, consider lowering it and handling `RestQueueFullException` gracefully.

> [!NOTE]
> If you're seeing frequent `RestQueueTimeoutException` or `RestQueueFullException`, it's usually a sign that your bot is sending too many requests to a single endpoint. Consider batching operations, adding application-level throttling, or spreading work across multiple endpoints.
