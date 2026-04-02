---
uid: topics_async_disposal
title: Async Disposal Migration
---

# Migrating to Async Disposal

Starting with **DisCatSharp 10.7.0**, all client types implement `IAsyncDisposable` and prefer async disposal over sync.

This guide covers what changed and how to update your code.

---

## What changed

| Type | Before | After |
|------|--------|-------|
| `DiscordClient` | `IDisposable` only | `IDisposable` + `IAsyncDisposable` |
| `DiscordShardedClient` | `IDisposable` only | `IDisposable` + `IAsyncDisposable` |
| `DiscordOAuth2Client` | `IDisposable` only | `IDisposable` + `IAsyncDisposable` |
| `DiscordWebhookClient` | `IDisposable` only | `IDisposable` + `IAsyncDisposable` |

The sync `Dispose()` method still works but internally blocks on `DisposeAsync()`. Prefer the async path to avoid potential deadlocks and thread pool starvation.

---

## Migration steps

### 1. Replace `using` with `await using`

```csharp
// Before
using var client = new DiscordClient(new DiscordConfiguration { Token = "..." });

// After
await using var client = new DiscordClient(new DiscordConfiguration { Token = "..." });
```

Block-scoped `using` statements follow the same pattern:

```csharp
// Before
using (var client = new DiscordClient(config))
{
    // ...
}

// After
await using (var client = new DiscordClient(config))
{
    // ...
}
```

### 2. Replace `.Dispose()` with `await .DisposeAsync()`

```csharp
// Before
client.Dispose();

// After
await client.DisposeAsync();
```

### 3. Ensure the enclosing method is `async`

Both `await using` and `await client.DisposeAsync()` require the method to be `async`. If your entry point is synchronous, convert it:

```csharp
// Before
static void Main(string[] args)
{
    using var client = new DiscordClient(config);
    client.ConnectAsync().GetAwaiter().GetResult();
    // ...
    client.Dispose();
}

// After
static async Task Main(string[] args)
{
    await using var client = new DiscordClient(config);
    await client.ConnectAsync();
    // ...
    // DisposeAsync called automatically at end of scope
}
```

---

## Analyzer support

Two new analyzer rules help catch sync disposal patterns:

| Rule | Severity | Description |
|------|----------|-------------|
| [DCS1301](xref:vs_analyzer_dcs_1301) | Warning | `using` should be `await using` on DisCatSharp client types |
| [DCS1302](xref:vs_analyzer_dcs_1302) | Warning | `.Dispose()` should be `await .DisposeAsync()` on DisCatSharp client types |

Both include code fixes that apply automatically in Visual Studio and other supported IDEs.

---

## Sync compatibility

The sync `Dispose()` path is preserved for backwards compatibility. It delegates to `DisposeAsync()` internally:

```csharp
// This still works but is not recommended
client.Dispose(); // internally calls DisposeAsync().GetAwaiter().GetResult()
```

Both sync and async disposal are **idempotent** — calling either method multiple times is safe and has no effect after the first call.

---

## Hosted service integration

If you use `DisCatSharp.Hosting` with ASP.NET Core or the Generic Host, disposal is handled automatically by the host's `IAsyncDisposable` support. No code changes are needed.

For manual hosted service implementations, override `StopAsync` and dispose the client there:

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
{
    await Client.DisconnectAsync();
    await Client.DisposeAsync();
    await base.StopAsync(cancellationToken);
}
```

---

## FAQ

**Q: Do I have to migrate immediately?**
No. Sync `Dispose()` continues to work. The analyzer warnings are informational — they guide you toward the preferred pattern but nothing breaks if you don't migrate right away.

**Q: Can I mix sync and async disposal?**
Yes. Calling `Dispose()` after `DisposeAsync()` (or vice versa) is safe due to the idempotency guard. Only the first call performs cleanup.

**Q: Does this affect `DisconnectAsync()`?**
No. `DisconnectAsync()` is separate from disposal — it gracefully closes the gateway connection. Disposal releases all remaining resources (HTTP clients, timers, internal state). You can disconnect and reconnect without disposing.
