---
uid: topics_workarounds
title: Workarounds
author: DisCatSharp Team
---

# Workarounds

Here is a collection of common workarounds for minor problems.

## Thread warns with _.. take too long to execute_

This warning happens from time to time if you're doing big tasks on events.
A quick workaround for this warning is the following method:

```cs
discordClient.VoiceStateUpdated += (sender, args) =>
{
    Task.Run(async () => await DiscordClientOnVoiceStateUpdated(sender, args));
    return Task.CompletedTask;
};
```

With this you start a new non-blocking thread.

Another alternative is:

```cs
new Thread(Method).Start()
```

## A messages components field type is changed suddenly

Let me quote something about that:

> Going forward, you should be exceedingly prepared to handle new components not wrapped in action rows
> - Discord Team

We've had to introduce a breaking change due to this.
[DiscordMessage.Components](xref:DisCatSharp.Entities.DiscordMessage.Components) is now of the type `IReadOnlyList<`[DiscordComponent](xref:DisCatSharp.Entities.DiscordComponent)`>` instead of `IReadOnlyList<`[DiscordActionRowComponent](xref:DisCatSharp.Entities.DiscordActionRowComponent)`>`.

To get components in action rows (because bots can only create such component hierarchies) reliable, you'd have to do [DiscordMessage.Components.OfType<DiscordActionRowComponent>](xref:System.Linq.Enumerable.OfType*) to get a `IReadOnlyList<`[DiscordActionRowComponent](xref:DisCatSharp.Entities.DiscordActionRowComponent)`>` again.
