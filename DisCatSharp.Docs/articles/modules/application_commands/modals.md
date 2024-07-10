---
uid: modules_application_commands_modals
title: Modals
---

# Modals

**The package `DisCatSharp.Interactivity` is required for this to work.**

You probably heard about the modal feature in Discord. It's a new feature that allows you to create a popup window that can be used to ask for information from the user. This is a great way to create a more interactive user experience.

The code below shows an example application command on how this could look.

```cs
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
```

```cs
[SlashCommand("modals", "A modal!")]
public async Task SendModalAsync(InteractionContext ctx)
{
	DiscordInteractionModalBuilder builder = new DiscordInteractionModalBuilder();
	builder.WithCustomId("modal_test");
	builder.WithTitle("Modal Test");
	builder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Paragraph, label: "Some input", required: false)));

    await ctx.CreateModalResponseAsync(builder);
	var res = await ctx.Client.GetInteractivity().WaitForModalAsync(builder.CustomId, TimeSpan.FromMinutes(1));

	if (res.TimedOut)
		return;

	await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordWebhookBuilder().WithContent(res.Result.Interaction.Data.Components?.First()?.Value ?? "Nothing was submitted."));
}
```
