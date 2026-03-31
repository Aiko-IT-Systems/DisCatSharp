---
uid: topics_gateway_dispatch
title: Gateway Dispatch Ordering
author: DisCatSharp Team
---

# Gateway Dispatch Ordering

DisCatSharp uses an ordered dispatch queue to process gateway events received from Discord. This page explains how the dispatch pipeline works and how to configure it.

## How It Works

When the Discord gateway sends a dispatch event (e.g., `MESSAGE_CREATE`, `GUILD_MEMBER_UPDATE`), DisCatSharp routes it through an internal `Channel<GatewayPayload>` queue before processing. This queue ensures events are **dequeued in the exact order they arrive** from the WebSocket connection.

A single consumer loop per client (per shard) reads from this queue and processes each event **sequentially**. This guarantees that **internal cache and state mutations always happen in FIFO order**, regardless of the configured dispatch mode.

After the internal processing (cache updates, entity backfills, etc.) completes for an event, the user-facing event handlers (`Client.MessageCreated += …`) are invoked. The configured **dispatch mode** controls whether those handlers are awaited inline or fired concurrently.

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

Internal cache mutations are processed sequentially (ordered). After each event's cache work completes, user event handlers are fired concurrently (fire-and-forget). Multiple events may have their user handlers running at the same time, but cache state is always consistent.

**Best for:** Most bots. Provides the highest throughput while maintaining cache integrity.

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

Each event is fully awaited — including all cache mutations **and** user event handler invocations — before the next event is dequeued. This provides **total serialization** per shard.

**Best for:** Bots where handler logic depends on strict event ordering and cannot tolerate overlapping execution (e.g., state machines that track member joins/leaves in sequence).

**Trade-off:** Lower throughput. If a handler takes a long time, subsequent events will queue up.

## Queue Capacity

The dispatch queue has a bounded capacity. If events arrive faster than they are consumed, overflow events are **dropped** and a warning is logged. This design ensures the WebSocket reader is never blocked — heartbeats, reconnects, and other control opcodes are always processed immediately.

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
> If the queue is full, incoming dispatch events will be **dropped** and a warning will be logged. Increase the capacity if you see drop warnings in your logs.

> [!NOTE]
> Setting the capacity to `0` creates an unbounded queue. This eliminates drops entirely but may consume significant memory under sustained high load.

## Sharding

Each shard (each `DiscordClient` instance) has its own independent dispatch queue and consumer loop. The dispatch mode and queue capacity settings apply per shard.

When using `DiscordShardedClient`, the configuration is cloned for each shard, so the dispatch settings are consistent across all shards.

## Summary

| Feature | ConcurrentHandlers | SequentialHandlers |
|---|---|---|
| Dequeue order | FIFO ✓ | FIFO ✓ |
| Cache mutation order | Sequential ✓ | Sequential ✓ |
| User handler execution | Concurrent (fire-and-forget) | Sequential (awaited) |
| User handler overlap | Possible | Never |
| Throughput | High | Lower |
| Default | ✓ | |
