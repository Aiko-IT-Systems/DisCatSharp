---
uid: topics_presence_caching
title: Presences & Activities
author: DisCatSharp Team
---

# Presences & Activities

## Overview

Presence data in Discord is guild-scoped. The same user can appear with a different status, different activities, and different platform clients in each guild they share with the bot. DisCatSharp stores presences in a centralized store keyed by user ID and guild ID to reflect this.

> [!IMPORTANT]
> The `GUILD_PRESENCES` privileged intent is required to receive presence updates. Without it, the cache will be empty after the initial READY payload.

## Quick Reference

| I want to … | API |
|---|---|
| Get a member's presence in a guild | `member.Presence` |
| Get all presences cached for a guild | `guild.Presences` |
| Get all cached presences for a user across guilds | `client.GetPresences(userId)` |
| Get the bot's own presence | `client.CurrentPresence` |
| Listen for presence changes | `client.PresenceUpdated` event |

## How the Presence Cache Works

DisCatSharp fills the presence cache from three gateway sources:

1. **Guild startup payloads** — when the bot connects and receives guild data.
2. **Member chunk responses** — when you request guild members via `RequestMembersAsync`.
3. **PRESENCE_UPDATE events** — real-time updates from Discord.

Internally, the cache is a two-level `ConcurrentDictionary<ulong userId, ConcurrentDictionary<ulong guildId, DiscordPresence>>`. This means:

- Each presence entry is keyed by **(userId, guildId)**.
- A user can have different presences in different guilds.
- Guild-scoped access is always authoritative.

> [!NOTE]
> Presence updates are processed on a dedicated fast-path channel with coalescing. If a user's presence changes rapidly, intermediate states may be merged — only the latest state is guaranteed to be cached.

## Choosing the Right API

### I have a member and want their presence

```cs
DiscordPresence? presence = member.Presence;

if (presence is not null && presence.Status == UserStatus.Online)
{
    // The member is online in this guild.
}
```

### I want all presences cached for one guild

```cs
foreach (var (userId, presence) in guild.Presences)
{
    Console.WriteLine($"{userId}: {presence.Status}");
}
```

### I want all cached presences for one user across guilds

```cs
IReadOnlyDictionary<ulong, DiscordPresence> presences = client.GetPresences(userId);

foreach (var (guildId, presence) in presences)
{
    Console.WriteLine($"Guild {guildId}: {presence.Status} — {presence.Activity?.Name}");
}
```

### I want the bot's own presence

Bots do not receive `PRESENCE_UPDATE` for themselves. Use [DiscordClient.CurrentPresence](xref:DisCatSharp.DiscordClient.CurrentPresence) instead:

```cs
DiscordPresence? botPresence = client.CurrentPresence;

Console.WriteLine($"Bot status: {botPresence?.Status}");
Console.WriteLine($"Bot activity: {botPresence?.Activity?.Name}");
```

This is automatically kept in sync when you call `UpdateStatusAsync`.

> [!TIP]
> `DiscordMember.Presence` also returns `CurrentPresence` when the member is the bot itself, so you don't need special-case logic in code that iterates members.

## Activities

A user can have **multiple activities at the same time**. For example, a user might have a custom status, be playing a game, and be listening to Spotify simultaneously.

### Single Activity vs. Activity List

[DiscordPresence](xref:DisCatSharp.Entities.DiscordPresence) exposes both:

| Property | Type | Description |
|---|---|---|
| `Activity` | `DiscordActivity?` | The first (primary) activity — convenience accessor |
| `Activities` | `IReadOnlyList<DiscordActivity>?` | All activities — the complete list |

> [!WARNING]
> If you only use `Activity`, you will miss secondary activities. Always use `Activities` when you need the full picture.

```cs
var presence = member.Presence;

if (presence?.Activities is not null)
{
    foreach (var activity in presence.Activities)
    {
        Console.WriteLine($"[{activity.ActivityType}] {activity.Name}");
    }
}
```

### Activity Types

Each [DiscordActivity](xref:DisCatSharp.Entities.DiscordActivity) has an `ActivityType`:

| Type | Value | Display | Example |
|---|---|---|---|
| `Playing` | 0 | "Playing {name}" | Playing Visual Studio Code |
| `Streaming` | 1 | "Streaming {name}" | Streaming on Twitch |
| `ListeningTo` | 2 | "Listening to {name}" | Listening to Spotify |
| `Watching` | 3 | "Watching {name}" | Watching YouTube Together |
| `Custom` | 4 | "{emoji} {state}" | 🎮 Taking a break |
| `Competing` | 5 | "Competing in {name}" | Competing in Arena World Championship |

### Custom Status

When `ActivityType` is `Custom`, the activity represents a user-set custom status with an optional emoji:

```cs
var customStatus = presence?.Activities?
    .FirstOrDefault(a => a.ActivityType == ActivityType.Custom);

if (customStatus is not null)
{
    Console.WriteLine($"Emoji: {customStatus.Emoji?.Name}");
    Console.WriteLine($"Text: {customStatus.State}");

    // Or use the structured accessor:
    var structured = customStatus.CustomStatus;
    Console.WriteLine($"Custom status: {structured?.Emoji?.Name} {structured?.State}");
}
```

### Rich Presence

Activities of type `Playing` may include rich presence data with game details, images, party info, and timestamps:

```cs
var gameActivity = presence?.Activities?
    .FirstOrDefault(a => a.ActivityType == ActivityType.Playing && a.RichPresence is not null);

if (gameActivity?.RichPresence is { } rp)
{
    Console.WriteLine($"Game: {gameActivity.Name}");
    Console.WriteLine($"Details: {rp.Details}");
    Console.WriteLine($"State: {rp.State}");
    Console.WriteLine($"Party: {rp.CurrentPartySize}/{rp.MaximumPartySize}");

    if (rp.StartTimestamp.HasValue)
        Console.WriteLine($"Elapsed: {DateTimeOffset.UtcNow - rp.StartTimestamp.Value}");

    if (rp.LargeImage is not null)
        Console.WriteLine($"Large image: {rp.LargeImageText}");
}
```

### Spotify / Listening Activity

Spotify activities have type `ListeningTo` and include sync and session IDs for tracking playback:

```cs
var spotify = presence?.Activities?
    .FirstOrDefault(a => a.ActivityType == ActivityType.ListeningTo);

if (spotify is not null)
{
    Console.WriteLine($"Listening to: {spotify.RichPresence?.Details}");      // Track name
    Console.WriteLine($"By: {spotify.RichPresence?.State}");                  // Artist(s)
    Console.WriteLine($"Album: {spotify.RichPresence?.LargeImageText}");      // Album name
    Console.WriteLine($"Sync ID: {spotify.SyncId}");                          // Spotify track ID
    Console.WriteLine($"Session: {spotify.SessionId}");                       // Spotify session

    if (spotify.RichPresence is { StartTimestamp: { } start, EndTimestamp: { } end })
    {
        var progress = DateTimeOffset.UtcNow - start;
        var duration = end - start;
        Console.WriteLine($"Progress: {progress:mm\\:ss} / {duration:mm\\:ss}");
    }
}
```

## Platform Status

Discord reports which platforms a user is active on through [DiscordClientStatus](xref:DisCatSharp.Entities.DiscordClientStatus):

| Property | Platforms |
|---|---|
| `Desktop` | Windows, Linux, macOS |
| `Mobile` | iOS, Android |
| `Web` | Browser, bot accounts |
| `Embedded` | Xbox, PlayStation, other embedded devices |
| `Vr` | VR headsets |

Each property is `Optional<UserStatus>` — it only has a value when the user is active on that platform.

```cs
var clientStatus = member.Presence?.ClientStatus;

if (clientStatus is null)
    return;

if (clientStatus.Desktop.HasValue)
    Console.WriteLine($"Desktop: {clientStatus.Desktop.Value}");

if (clientStatus.Mobile.HasValue)
    Console.WriteLine($"Mobile: {clientStatus.Mobile.Value}");

if (clientStatus.Web.HasValue)
    Console.WriteLine($"Web: {clientStatus.Web.Value}");

if (clientStatus.Embedded.HasValue)
    Console.WriteLine($"Embedded: {clientStatus.Embedded.Value}");

if (clientStatus.Vr.HasValue)
    Console.WriteLine($"VR: {clientStatus.Vr.Value}");
```

> [!NOTE]
> The `embedded` and `vr` platform statuses are not documented by Discord but are sent by the gateway. DisCatSharp supports them.

## Listening for Presence Changes

Use the `PresenceUpdated` event to react to presence changes in real time:

```cs
client.PresenceUpdated += (sender, e) =>
{
    Console.WriteLine($"User {e.User.Username} changed status: {e.PresenceBefore?.Status} → {e.Status}");

    if (e.Activities is not null)
    {
        foreach (var activity in e.Activities)
            Console.WriteLine($"  [{activity.ActivityType}] {activity.Name}");
    }

    return Task.CompletedTask;
};
```

[PresenceUpdateEventArgs](xref:DisCatSharp.EventArgs.User.PresenceUpdateEventArgs) provides:

| Property | Description |
|---|---|
| `User` | The user whose presence changed |
| `Status` | The new overall status |
| `Activity` | The new primary activity |
| `Activities` | The complete list of new activities |
| `PresenceBefore` | The previous presence state (may be null on first update) |
| `PresenceAfter` | The new presence state |

## Status Values

The [UserStatus](xref:DisCatSharp.Entities.UserStatus) enum defines:

| Status | Value | Gateway String |
|---|---|---|
| `Offline` | 0 | `"offline"` |
| `Online` | 1 | `"online"` |
| `Idle` | 2 | `"idle"` |
| `DoNotDisturb` | 4 | `"dnd"` |
| `Invisible` | 5 | `"invisible"` |
| `Streaming` | 6 | `"streaming"` |

## Recommendations

- Always use **guild-scoped** access (`member.Presence` or `guild.Presences`) when you have a guild context.
- Use `Activities` (plural) instead of `Activity` (singular) when you need the full picture — users commonly have 2–3 activities at once.
- Use `client.CurrentPresence` to read the bot's own presence. Do not try to look it up in the presence store.
- Use `client.GetPresences(userId)` when you need to see a user's status across multiple guilds.
- Remember that the `GUILD_PRESENCES` intent is privileged and must be enabled in the Discord Developer Portal.
