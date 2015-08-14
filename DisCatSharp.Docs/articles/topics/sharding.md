---
uid: topics_sharding
title: Sharding
author: DisCatSharp Team
---

# Sharding

As your bot joins more guilds, your poor `DiscordClient` will be hit with an increasing number of events.
Thankfully, Discord allows you to establish multiple connections to split the event workload; this is called _sharding_ and each individual connection is referred to as a _shard_.
Each shard handles a separate set of servers and will _only_ receive events from those servers. However, all direct messages will be handled by your first shard.

Sharding is recommended once you reach 1,000 servers, and is a _requirement_ when you hit 2,500 servers.

## Automated Sharding

DisCatSharp provides a built-in sharding solution: `DiscordShardedClient`.
This client will _automatically_ spawn shards for you and manage their events.
Each DisCatSharp extension (e.g. CommandsNext, Interactivity) also supplies an extension method to register themselves automatically on each shard.

```cs
var discord = new DiscordShardedClient(new DiscordConfiguration
{
    Token = "My First Token",
    TokenType = TokenType.Bot
});

await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
{
    StringPrefixes = new[] { "!" }
});
```

## Manual Sharding

For most looking to shard, the built-in `DiscordShardedClient` will work well enough.
However, those looking for more control over the sharding process may want to handle it manually.

This would involve creating new `DiscordClient` instances, assigning each one an appropriate shard ID number, and handling the events from each instance.
Considering the potential complexity imposed by this process, you should only do this if you have a valid reason to do so and _know what you are doing_.
