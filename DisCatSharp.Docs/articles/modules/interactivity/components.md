---
uid: modules_interactivity_components
title: Components
---

# Components

Interactivity can wait for buttons and select menus after you send a component message.
This is the easiest way to build confirmation dialogs, menu-driven flows, and guided message UIs.

> [!IMPORTANT]
> The message you wait on must be created by your application.
> Discord only sends the interaction event back to the application that owns the component.

For component construction details, also read:

- [Buttons](xref:topics_components_buttons)
- [Select Menus](xref:topics_components_select_menus)

## Waiting for a button press

You can wait for any button, a specific button id, a specific user, or a custom predicate.

```cs
var builder = new DiscordMessageBuilder()
    .WithContent("Confirm the action.")
    .AddComponents(
    [
        new DiscordButtonComponent(ButtonStyle.Success, "confirm", "Confirm"),
        new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel")
    ]);

var message = await ctx.Channel.SendMessageAsync(builder);

var result = await message.WaitForButtonAsync(ctx.User, TimeSpan.FromSeconds(30));

if (result.TimedOut)
{
    await ctx.RespondAsync("No answer received.");
    return;
}

await result.Result.Interaction.CreateResponseAsync(
    InteractionResponseType.UpdateMessage,
    new DiscordInteractionResponseBuilder().WithContent($"You pressed `{result.Result.Id}`."));
```

If you only want one specific button, wait by custom id:

```cs
var result = await message.WaitForButtonAsync("confirm", TimeSpan.FromSeconds(30));
```

## Waiting for a select menu

Select waits follow the same idea, but you must also tell interactivity which select component type to expect.

```cs
var select = new DiscordStringSelectComponent(
    "color_select",
    "Pick a color",
    [
        new DiscordSelectComponentOption("Pink", "pink"),
        new DiscordSelectComponentOption("Purple", "purple"),
        new DiscordSelectComponentOption("Black", "black")
    ]);

var message = await ctx.Channel.SendMessageAsync(
    new DiscordMessageBuilder()
        .WithContent("Choose one.")
        .AddComponents(select));

var result = await message.WaitForSelectAsync(
    ctx.User,
    "color_select",
    ComponentType.StringSelect,
    TimeSpan.FromSeconds(30));

if (result.TimedOut)
    return;

var choice = result.Result.Values.FirstOrDefault();
await result.Result.Interaction.CreateResponseAsync(
    InteractionResponseType.ChannelMessageWithSource,
    new DiscordInteractionResponseBuilder().WithContent($"You picked `{choice}`.").AsEphemeral());
```

## Available wait styles

Both button waits and select waits support the same practical patterns:

| Pattern | Example |
| --- | --- |
| First matching interaction | `await message.WaitForButtonAsync()` |
| Specific custom id | `await message.WaitForButtonAsync("confirm")` |
| Specific user | `await message.WaitForButtonAsync(ctx.User)` |
| Custom predicate | `await message.WaitForButtonAsync(args => args.User.Id == ctx.User.Id)` |

Select waits additionally require a `ComponentType`, such as `ComponentType.StringSelect`.

## Invalid interaction behavior

When a different user presses a component you are waiting on, `InteractivityConfiguration.ResponseBehavior` controls how interactivity should react:

- `Ignore` leaves the interaction unanswered
- `Ack` acknowledges it without doing anything else
- `Respond` sends the configured `ResponseMessage`

Use this when you want locked controls that clearly tell other users the component is not for them.
