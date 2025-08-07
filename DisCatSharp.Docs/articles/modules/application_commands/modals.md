---
uid: modules_application_commands_modals
title: Modals
---

# Modals

**The package `DisCatSharp.Interactivity` is required for this to work.**

You probably heard about the modal feature in Discord.
It's a feature that allows you to create a popup window that can be used to ask for information from the user.
This is a great way to create a more interactive user experience.

The code below shows an example application command on how this could look.

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
```

```cs
[SlashCommand("cats_modal", "A modal with questions about cats!")]
public async Task SendCatsModalAsync(InteractionContext ctx)
{
	DiscordInteractionModalBuilder builder = new DiscordInteractionModalBuilder();
	builder.WithCustomId("cats_modal");
	builder.WithTitle("Cats");

    List<DiscordStringSelectComponentOption> cats = [new("Yes", "I love cats", isDefault: true), new("No", "I hate cats")];
	DiscordStringSelectComponent catSelect = new("Choose carefully..", cats);

	builder.AddLabelComponent(new DiscordLabelComponent("Cats", "Do you like cats?").WithStringSelectComponent(catSelect));
	builder.AddLabelComponent(new DiscordLabelComponent("Like", "What do you like about cats?").WithTextComponent(new(TextComponentStyle.Paragraph)));
	builder.AddLabelComponent(new DiscordLabelComponent("Dislike", "What do you dislike about cats?").WithTextComponent(new(TextComponentStyle.Small)));

    await ctx.CreateModalResponseAsync(builder);

	var res = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromMinutes(1));

	if (res.TimedOut)
		return;

    await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

	var catSelectChoice = (res.Result.Interaction.Data.ModalComponents
		.OfType<DiscordLabelComponent>()
		.Where(x => x.Component is DiscordStringSelectComponent)
		.FirstOrDefault()?.Component as DiscordStringSelectComponent)?.SelectedValues?.FirstOrDefault();

	var catLikeText = (res.Result.Interaction.Data.ModalComponents
		.OfType<DiscordLabelComponent>()
		.Where(x => x.Component is DiscordTextInputComponent y && y.Style is TextComponentStyle.Paragraph)
		.FirstOrDefault()?.Component as DiscordTextInputComponent)?.Value;

	var catDislikeText = (res.Result.Interaction.Data.ModalComponents
		.OfType<DiscordLabelComponent>()
		.Where(x => x.Component is DiscordTextInputComponent y && y.Style is TextComponentStyle.Small)
		.FirstOrDefault()?.Component as DiscordTextInputComponent)?.Value;

    var webhookBuilder = new DiscordWebhookBuilder().WithV2Components();
	var container = new DiscordContainerComponent([
		new DiscordTextDisplayComponent(builder.Title.Header2()),
		new DiscordTextDisplayComponent("Do you like cats?".Header3() + $"\n{(catSelectChoice ?? "No selection").Italic()}"),
		new DiscordTextDisplayComponent("What do you like about cats?".Header3() + $"\n{(catLikeText ?? "No input").BlockCode()}"),
		new DiscordTextDisplayComponent("What do you dislike about cats?".Header3() + $"\n{(catDislikeText ?? "No input").Italic()}"),
	]);
	webhookBuilder.AddComponents(container);
	await res.Result.Interaction.EditOriginalResponseAsync(webhookBuilder);
}
```
