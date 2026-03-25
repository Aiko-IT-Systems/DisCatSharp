---
uid: topics_workarounds
title: Workarounds
author: DisCatSharp Team
---

# Workarounds

Here is a collection of common workarounds for minor problems.

## Thread warns with _.. take too long to execute_

This warning can appear if you do long-running work directly inside an event handler. Gateway events are processed sequentially, so blocking handlers can delay later events.

If the work is not required to finish on the gateway thread, offload it:

```cs
discordClient.VoiceStateUpdated += (sender, args) =>
{
    _ = Task.Run(async () => await DiscordClientOnVoiceStateUpdated(sender, args));
    return Task.CompletedTask;
};
```

This allows the event handler itself to complete quickly while the heavy work continues in the background.

## Some cache lookups return missing or partial objects

Not every event payload can be resolved to a fully cached object. Depending on startup timing, cache settings, and what Discord included in the payload, some references may be missing from cache or represented only partially.

When handling gateway or audit-log data, prefer working with ids first and fetch additional data explicitly if your logic truly needs it.

```cs
discordClient.MessageDeleted += async (client, args) =>
{
    var guildId = args.Guild?.Id;
    var channelId = args.Channel.Id;
    var messageId = args.Message.Id;

    // Persist ids or do lightweight handling immediately.

    if (guildId is not null)
    {
        var guild = await client.GetGuildAsync(guildId.Value);
        // Only fetch more state if you actually need it.
    }
};
```

This makes handlers more resilient when Discord sends incomplete cache context.

## Audit log references can stay partial by design

Audit log parsing in DisCatSharp intentionally avoids implicit REST calls. If Discord does not include enough information to fully resolve a user, member, role, channel, or target object, the library may keep a partial synthetic object instead of fetching it behind your back.

If you need the full object, treat the audit log reference as a starting point and hydrate it yourself:

```cs
var entry = auditLog.Entries.First();
var actorId = entry.UserResponsible?.Id;

if (actorId is not null)
{
    var fullUser = await discordClient.GetUserAsync(actorId.Value);
    Console.WriteLine(fullUser.Username);
}
```

## A messages components field type is changed suddenly

Let me quote something about that:

> Going forward, you should be exceedingly prepared to handle new components not wrapped in action rows
> - Discord Team

We've had to introduce a breaking change due to this.
[DiscordMessage.Components](xref:DisCatSharp.Entities.DiscordMessage.Components) is now of the type `IReadOnlyList<`[DiscordComponent](xref:DisCatSharp.Entities.DiscordComponent)`>` instead of `IReadOnlyList<`[DiscordActionRowComponent](xref:DisCatSharp.Entities.DiscordActionRowComponent)`>`.

To get components in action rows (because bots can only create such component hierarchies) reliable, you'd have to do [DiscordMessage.Components.OfType<DiscordActionRowComponent>](xref:System.Linq.Enumerable.OfType*) to get a `IReadOnlyList<`[DiscordActionRowComponent](xref:DisCatSharp.Entities.DiscordActionRowComponent)`>` again.
