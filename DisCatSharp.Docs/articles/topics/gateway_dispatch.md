---
uid: topics_gateway_dispatch
title: Gateway Dispatch Ordering
author: DisCatSharp Team
---

# Gateway Dispatch Ordering

DisCatSharp uses an ordered dispatch queue to process gateway events received from Discord. This page explains how the dispatch pipeline works and how to configure it.

## How It Works

When the Discord gateway sends a dispatch event (e.g., `MESSAGE_CREATE`, `GUILD_MEMBER_UPDATE`), DisCatSharp routes it through an internal `Channel<GatewayPayload>` queue before processing. This queue ensures events are **dequeued in the exact order they arrive** from the WebSocket connection.

A single consumer loop per client (per shard) reads from this queue and processes each event. The behavior after dequeuing depends on the configured **dispatch mode**.

## Dispatch Modes

The dispatch mode is configured via `GatewayAdvancedConfiguration.DispatchMode`:

### ConcurrentHandlers (Default)

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Gateway =
    {
        Advanced =
        {
            DispatchMode = GatewayDispatchMode.ConcurrentHandlers
        }
    }
};
```

Events are dequeued in order, but each event's processing (cache mutations + handler invocations) is fired concurrently. Multiple events may have their handlers running at the same time.

**Best for:** Most bots. Provides the highest throughput and is the default behavior.

### SequentialHandlers

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Gateway =
    {
        Advanced =
        {
            DispatchMode = GatewayDispatchMode.SequentialHandlers
        }
    }
};
```

Each event is fully awaited — including all cache mutations and user event handler invocations — before the next event is dequeued. This provides **total serialization** per shard.

**Best for:** Bots where handler logic depends on strict event ordering and cannot tolerate overlapping execution (e.g., state machines that track member joins/leaves in sequence).

**Trade-off:** Lower throughput. If a handler takes a long time, subsequent events will queue up.

## Queue Capacity

The dispatch queue has a bounded capacity to provide back-pressure. If events arrive faster than they are consumed, the queue will block the WebSocket reader until space is available.

```csharp
var config = new DiscordConfiguration
{
    Token = "your-token",
    Gateway =
    {
        Advanced =
        {
            // Default: 10,000 events
            DispatchQueueCapacity = 20_000,

            // Or unbounded (use with caution):
            // DispatchQueueCapacity = 0
        }
    }
};
```

> [!WARNING]
> If the queue is full, incoming dispatch events will be **dropped** and a warning will be logged. This prevents the WebSocket reader from stalling, which would block heartbeats and cause a gateway disconnect. Increase the capacity if you see drop warnings in your logs.

> [!NOTE]
> Setting the capacity to `0` creates an unbounded queue. This eliminates back-pressure but may consume significant memory under high load.

## Sharding

Each shard (each `DiscordClient` instance) has its own independent dispatch queue and consumer loop. The dispatch mode and queue capacity settings apply per shard.

When using `DiscordShardedClient`, the configuration is cloned for each shard, so the dispatch settings are consistent across all shards.

## Summary

| Feature | ConcurrentHandlers | SequentialHandlers |
|---|---|---|
| Dequeue order | FIFO ✓ | FIFO ✓ |
| Handler execution | Concurrent (fire-and-forget) | Sequential (awaited) |
| Throughput | High | Lower |
| Event handler overlap | Possible | Never |
| Default | ✓ | |
