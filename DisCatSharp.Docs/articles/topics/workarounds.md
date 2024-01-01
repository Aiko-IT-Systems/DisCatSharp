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
