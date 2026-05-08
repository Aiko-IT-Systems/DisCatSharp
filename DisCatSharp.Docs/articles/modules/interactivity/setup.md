---
uid: modules_interactivity_setup
title: Setup and Configuration
---

# Setup and Configuration

Start by installing the `DisCatSharp.Interactivity` package, then register it on your client.

```cs
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;

var discord = new DiscordClient(configuration);

discord.UseInteractivity(new InteractivityConfiguration
{
    Timeout = TimeSpan.FromSeconds(45),
    PollBehaviour = PollBehaviour.KeepEmojis,
    ResponseBehavior = InteractionResponseBehavior.Respond,
    ResponseMessage = "This interaction was not meant for you."
});
```

If you do not pass a configuration object, interactivity uses its built-in defaults.

## Retrieving the extension

You can access the registered extension at any time with `GetInteractivity()`:

```cs
var interactivity = ctx.Client.GetInteractivity();
```

That instance exposes helpers such as page generation and lower-level wait methods.

## Sharded clients

For sharded bots, use `UseInteractivityAsync()` to enable the extension for every shard:

```cs
var extensions = await shardedClient.UseInteractivityAsync();
```

## Common configuration options

| Property | Default | What it changes |
| --- | --- | --- |
| `Timeout` | `1 minute` | Default timeout for waits and pagination |
| `PollBehaviour` | `DeleteEmojis` | Whether poll reactions are cleaned up when the poll ends |
| `PaginationBehaviour` | `WrapAround` | Whether moving past the end loops around or is ignored |
| `PaginationDeletion` | `DeleteEmojis` | What happens to reaction pagination controls on timeout |
| `ButtonBehavior` | `Disable` | What happens to button pagination controls on timeout |
| `PaginationEmojis` | `⏮ ◀ ⏹ ▶ ⏭` | The default reaction controls |
| `PaginationButtons` | `⏮ ◀ ⏹ ▶ ⏭` | The default button controls |
| `AckPaginationButtons` | `false` | Whether pagination button presses are acknowledged immediately |
| `ResponseBehavior` | `Ignore` | How invalid component interactions should be handled |
| `ResponseMessage` | unset | Message used when `ResponseBehavior` is `Respond` |

> [!IMPORTANT]
> `ResponseMessage` must be set when `ResponseBehavior` is `Respond`.

## Intents and prerequisites

Different interactivity features rely on different gateway events:

| Feature | What you need |
| --- | --- |
| Message waits | Message intents |
| Reaction waits, reaction collection, and reaction pagination | Reaction intents |
| Typing waits | Typing intents |
| Buttons, select menus, and modals | No extra gateway intent, but the waited-on component message must belong to your application |

For component waits, interactivity only listens for interactions on messages created by your bot.
Trying to wait on someone else's component message throws an exception.

## Where to go next

- For chat-based flows, continue with [messages, reactions, and polls](xref:modules_interactivity_messages).
- For buttons and selects, continue with [components](xref:modules_interactivity_components).
- For modal submissions, continue with [modals](xref:modules_interactivity_modals).
- For long output and page navigation, continue with [pagination](xref:modules_interactivity_pagination).
