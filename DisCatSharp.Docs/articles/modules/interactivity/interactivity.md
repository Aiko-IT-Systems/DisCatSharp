---
uid: modules_interactivity_introduction
title: Interactivity Overview
---

# Interactivity Overview

`DisCatSharp.Interactivity` helps you build multi-step flows without manually wiring every event handler yourself.
You can wait for follow-up messages, reactions, typing events, button presses, select menus, modal submissions, and paginated navigation with one extension.

Install the `DisCatSharp.Interactivity` package from NuGet before continuing.

## What this module covers

This section is split into practical guides:

| Guide | Covers |
| --- | --- |
| [Setup and configuration](xref:modules_interactivity_setup) | Enabling interactivity, retrieving the extension, and tuning defaults |
| [Messages, reactions, and polls](xref:modules_interactivity_messages) | Waiting for follow-up messages, reactions, typing indicators, and reaction-based polls |
| [Components](xref:modules_interactivity_components) | Waiting for buttons and select menus on your own messages |
| [Modals](xref:modules_interactivity_modals) | Waiting for modal submissions and working with paginated modals |
| [Pagination](xref:modules_interactivity_pagination) | Generating pages, button pagination, reaction pagination, and interaction responses |

## When to use it

Interactivity is a good fit when a command needs to pause and wait for the user to do something next, for example:

- confirm an action with a button
- collect follow-up text in chat
- run a quick emoji poll
- page through long output
- open a modal and wait for its submission

If you only need to **construct** components, also read the dedicated component topic guides:

- [Buttons](xref:topics_components_buttons)
- [Select Menus](xref:topics_components_select_menus)

If you only need to **build** modals, the application command docs remain useful reference material:

- [Modals](xref:modules_application_commands_modals)
- [Paginated Modals](xref:modules_application_commands_paginated_modals)

## How interactivity is exposed

Interactivity is available through:

- extension methods on `DiscordChannel`, `DiscordMessage`, and `DiscordInteraction`
- instance methods on <xref:DisCatSharp.Interactivity.InteractivityExtension>

The extension methods are the easiest starting point for most bots, while the extension instance exposes helpers such as page generation and lower-level wait methods.

## Common result pattern

Most wait methods return an `InteractivityResult<T>`.
Always check whether the operation timed out before using the result:

```cs
var result = await ctx.Message.GetNextMessageAsync(TimeSpan.FromSeconds(30));

if (result.TimedOut)
{
    await ctx.RespondAsync("Timed out waiting for a reply.");
    return;
}
```

Continue with [setup and configuration](xref:modules_interactivity_setup) to register the extension and choose sensible defaults for your bot.
