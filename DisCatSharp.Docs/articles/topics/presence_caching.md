---
uid: topics_presence_caching
title: Presence Caching
author: DisCatSharp Team
---

# Presence Caching

## Overview

Presence data in Discord is not globally unique. The same user can appear with different presence data in different guilds, so DisCatSharp treats guild-scoped presence data as the authoritative source.

This means you should choose the API you read from based on what you are trying to answer:

- Use [DiscordGuild.Presences](xref:DisCatSharp.Entities.DiscordGuild.Presences) when you need the cached presences for one guild.
- Use [DiscordMember.Presence](xref:DisCatSharp.Entities.DiscordUser.Presence) when you already have a member object and want that member's guild-specific presence.
- Use [DiscordClient.GetPresences(System.UInt64)](xref:DisCatSharp.DiscordClient.GetPresences(System.UInt64)) when you want every cached presence for a user across all cached guilds.
- Use [DiscordClient.Presences](xref:DisCatSharp.DiscordClient.Presences) only as a client-wide aggregate compatibility view.

## How Presence Caching Works

DisCatSharp fills the presence cache from gateway payloads such as guild startup payloads, member chunk payloads, and presence updates.

Guild caches are authoritative:

- [DiscordGuild.Presences](xref:DisCatSharp.Entities.DiscordGuild.Presences) stores presences keyed by user id for that guild.
- [DiscordMember.Presence](xref:DisCatSharp.Entities.DiscordUser.Presence) resolves against the member's guild cache.

The client also keeps a compatibility cache:

- [DiscordClient.Presences](xref:DisCatSharp.DiscordClient.Presences) stores one aggregate presence per user id.
- Because that view is keyed only by user id, it cannot represent multiple simultaneous guild presences for the same user.

If a user is visible in more than one guild, [DiscordClient.GetPresences(System.UInt64)](xref:DisCatSharp.DiscordClient.GetPresences(System.UInt64)) is the correct API when you need every cached presence entry.

## Choosing the Right API

### I have a member and want their presence

Use the member directly:

```cs
DiscordPresence? presence = member.Presence;

if (presence is not null && presence.Status == UserStatus.Online)
{
    // The member is online in this guild context.
}
```

### I want all presences cached for one guild

Read the guild cache:

```cs
foreach (var (userId, presence) in guild.Presences)
{
    Console.WriteLine($"{userId}: {presence.Status}");
}
```

### I want all cached presences for one user across guilds

Use the client helper:

```cs
var cachedPresences = discord.GetPresences(userId);

foreach (var (guildId, presence) in cachedPresences)
{
    if (guildId == 0)
        Console.WriteLine($"Aggregate presence: {presence.Status}");
    else
        Console.WriteLine($"Guild {guildId}: {presence.Status}");
}
```

The `0` key is reserved for a non-guild aggregate presence entry when one exists.

## Aggregate Presence Cache Size

If you want to limit the size of the client-wide aggregate presence view, set [DiscordConfiguration.PresenceCacheSize](xref:DisCatSharp.DiscordConfiguration.PresenceCacheSize).

```cs
var config = new DiscordConfiguration
{
    Token = "token",
    TokenType = TokenType.Bot,
    PresenceCacheSize = 5000
};
```

Important details:

- This only limits [DiscordClient.Presences](xref:DisCatSharp.DiscordClient.Presences).
- Guild-scoped caches such as [DiscordGuild.Presences](xref:DisCatSharp.Entities.DiscordGuild.Presences) are not evicted by this setting.
- Set it to `0` to disable the aggregate cache size cap.

## Platform Status

Discord can report per-platform status information through [DiscordPresence.ClientStatus](xref:DisCatSharp.Entities.DiscordPresence.ClientStatus).

In addition to `desktop`, `mobile`, and `web`, Discord may also send:

- `embedded`
- `vr`

You can inspect them through [DiscordClientStatus](xref:DisCatSharp.Entities.DiscordClientStatus):

```cs
var clientStatus = member.Presence?.ClientStatus;

if (clientStatus?.Embedded.HasValue == true)
    Console.WriteLine($"Embedded status: {clientStatus.Embedded.Value}");

if (clientStatus?.Vr.HasValue == true)
    Console.WriteLine($"VR status: {clientStatus.Vr.Value}");
```

## Recommendations

- Prefer guild-scoped presence data whenever you have a guild or member context.
- Treat [DiscordClient.Presences](xref:DisCatSharp.DiscordClient.Presences) as a convenience view, not as the authoritative source for multi-guild users.
- If your bot does not need a large aggregate presence view, consider setting [DiscordConfiguration.PresenceCacheSize](xref:DisCatSharp.DiscordConfiguration.PresenceCacheSize) to a reasonable cap.
