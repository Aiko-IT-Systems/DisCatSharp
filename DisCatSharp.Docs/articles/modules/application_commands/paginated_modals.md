---
uid: modules_application_commands_paginated_modals
title: Paginated Modals
---

# Paginated Modals

**The package `DisCatSharp.Interactivity` is required for this to work.**

You may need multi-step modals to collect a variety of information from a user. We implemented an easy way of doing this with paginated modals.
You simply construct all your modals, call `DiscordInteraction.CreatePaginatedModalResponseAsync` and you're good to go. After the user submitted all modals, you'll get back a `PaginatedModalResponse` which has a `TimedOut` bool, the `DiscordInteraction` that was used to submit the last modal and a `IReadOnlyDictionary<string, string>` with the component custom ids as key.

The code below shows an example application command on how this could look.

```cs
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
```

```cs
[SlashCommand("paginated-modals", "Paginated modals!")]
public async Task PaginatedModals(InteractionContext ctx)
{
    _ = Task.Run(async () =>
    {
        var responses = await ctx.Interaction.CreatePaginatedModalResponseAsync(
            new List<ModalPage>()
            {
                new ModalPage(new DiscordInteractionModalBuilder().WithTitle("First Title")
                    .AddLabelComponent(new DiscordLabelComponent().WithTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title", "Placeholder", 0, 250, false)))),
                new ModalPage(new DiscordInteractionModalBuilder().WithTitle("Second Title")
                    .AddLabelComponent(new DiscordLabelComponent().WithTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title_1", "Placeholder 1", 0, 250, false))),
                    .AddLabelComponent(new DiscordLabelComponent().WithTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "description_1", "Some bigger Placeholder here", required: false)))),
                new ModalPage(new DiscordInteractionModalBuilder().WithTitle("Third Title")
                    .AddLabelComponent(new DiscordLabelComponent().WithTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "title_2", "Placeholder 2", 0, 250, false)))
                    .AddLabelComponent(new DiscordLabelComponent().WithTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, "description_2", "Some placeholder here", required: false)))),
            });

        // If the user didn't submit all modals, TimedOut will be true. We return the command as there is nothing to handle.
        if (responses.TimedOut)
            return;

        // We simply throw all response into the Console, you can do whatever with this.
        foreach (var b in responses.Responses.Values)
            Console.WriteLine(b.ToString());

        /*
        // You can also receive select values
        foreach (var b in responses.SelectResponses.Values)
            Console.WriteLine(string.Join(", ", b.Select(x => x.ToString())));
        */

        // We use EditOriginalResponseAsync here because CreatePaginatedModalResponseAsync responds to the last modal with a thinking state.
        await responses.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
    });
}
```
