---
uid: modules_interactivity_modals
title: Modals
---

# Modals

Interactivity is responsible for the **waiting** side of modal flows.
You create the modal response first, then wait for the matching submission with `WaitForModalAsync()`.

For modal building reference, also read the application command guides:

- [Modals](xref:modules_application_commands_modals)
- [Paginated Modals](xref:modules_application_commands_paginated_modals)

## Waiting for a modal submission

```cs
var builder = new DiscordInteractionModalBuilder()
    .WithCustomId("feedback_modal")
    .WithTitle("Feedback")
    .AddLabelComponent(
        new DiscordLabelComponent("Feedback", "Tell us what you think")
            .WithTextComponent(new(TextComponentStyle.Paragraph, customId: "feedback", required: true)));

await ctx.CreateModalResponseAsync(builder);

var result = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromMinutes(2));

if (result.TimedOut)
{
    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Timed out waiting for the modal."));
    return;
}

await result.Result.Interaction.CreateResponseAsync(
    InteractionResponseType.ChannelMessageWithSource,
    new DiscordInteractionResponseBuilder().WithContent("Thanks for the feedback.").AsEphemeral());
```

## Reading submitted values

The submitted components are available through `result.Result.Interaction.Data.ModalComponents`.

```cs
var feedback = (result.Result.Interaction.Data.ModalComponents
    .OfType<DiscordLabelComponent>()
    .FirstOrDefault(x => x.Component is DiscordTextInputComponent input && input.CustomId == "feedback")
    ?.Component as DiscordTextInputComponent)?.Value;
```

## Paginated modals

For multi-step collection flows, interactivity also provides `CreatePaginatedModalResponseAsync()` on `DiscordInteraction`.
It opens a sequence of modals and returns a `PaginatedModalResponse` containing the captured values.

```cs
var response = await ctx.Interaction.CreatePaginatedModalResponseAsync(modalPages, TimeSpan.FromMinutes(5));

if (response.TimedOut)
    return;

var title = response.Responses["title"];
```

Use this when a single modal would become too dense or you want to collect data in several small steps instead.
