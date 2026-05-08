---
uid: modules_interactivity_messages
title: Messages, Reactions, and Polls
---

# Messages, Reactions, and Polls

This part of interactivity covers follow-up chat input, reaction-based confirmation, typing indicators, and lightweight polls.

## Waiting for the next message

The quickest way to continue a conversation is waiting for the next message in the same channel.
When you already have the user's original message, `GetNextMessageAsync()` is usually the most convenient entry point.

```cs
await ctx.RespondAsync("Reply with `confirm` to continue.");

var result = await ctx.Message.GetNextMessageAsync(
    m => string.Equals(m.Content, "confirm", StringComparison.OrdinalIgnoreCase),
    TimeSpan.FromSeconds(30));

if (result.TimedOut)
{
    await ctx.RespondAsync("You took too long.");
    return;
}

await ctx.RespondAsync("Confirmed.");
```

You can also wait from a channel directly:

```cs
var result = await ctx.Channel.GetNextMessageAsync(ctx.User, TimeSpan.FromSeconds(30));
```

## Waiting for reactions

`WaitForReactionAsync()` is useful for small yes/no prompts or emoji-based confirmations.

```cs
var emoji = DiscordEmoji.FromUnicode("👌");
var message = await ctx.RespondAsync($"React with {emoji} to continue.");

var result = await message.WaitForReactionAsync(ctx.User, emoji, TimeSpan.FromSeconds(30));

if (!result.TimedOut)
    await ctx.RespondAsync("Thanks.");
```

Reaction waits require the appropriate reaction intents to be enabled.

## Collecting reactions

If you want the final totals instead of the first valid input, use `CollectReactionsAsync()`:

```cs
var message = await ctx.RespondAsync("Vote with reactions now.");
var reactions = await message.CollectReactionsAsync(TimeSpan.FromSeconds(20));

foreach (var reaction in reactions)
    await ctx.Channel.SendMessageAsync($"{reaction.Emoji}: {reaction.Total}");
```

## Running a simple poll

`DoPollAsync()` adds the supplied emojis, waits until timeout, and returns the final counts.

```cs
var message = await ctx.RespondAsync("Which option do you want?");
var results = await message.DoPollAsync(
[
    DiscordEmoji.FromUnicode("1️⃣"),
    DiscordEmoji.FromUnicode("2️⃣"),
    DiscordEmoji.FromUnicode("3️⃣")
],
timeoutOverride: TimeSpan.FromSeconds(30));

foreach (var result in results)
    await ctx.Channel.SendMessageAsync($"{result.Emoji}: {result.Total}");
```

By default, poll reactions are deleted after the poll ends.
Change `InteractivityConfiguration.PollBehaviour` to `KeepEmojis` if you want the reactions to stay visible.

## Waiting for typing

Typing waits are useful when you want to know whether a user has started responding before the actual message arrives.

```cs
var typing = await ctx.Channel.WaitForUserTypingAsync(ctx.User, TimeSpan.FromSeconds(15));

if (!typing.TimedOut)
    await ctx.RespondAsync("I can see you typing...");
```

This feature requires typing intents.

## Choosing the right entry point

| Method | Best for |
| --- | --- |
| `GetNextMessageAsync()` | Chat-style follow-up prompts |
| `WaitForReactionAsync()` | One reaction from one user |
| `CollectReactionsAsync()` | Totals across a whole voting window |
| `DoPollAsync()` | Quick reaction polls with automatic setup |
| `WaitForUserTypingAsync()` | Detecting that a reply has started |
