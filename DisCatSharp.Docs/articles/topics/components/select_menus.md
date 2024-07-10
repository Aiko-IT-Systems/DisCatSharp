---
uid: topics_components_select_menus
title: Select Menus
author: DisCatSharp Team
---

# Introduction

The select menus, like the [buttons](xref:topics_components_buttons), are message components.
You will want to familarize yourself with the [message builder](xref:topics_messagebuilder) as it and similar builder objects will be used throughout this article.

A row can only have one select menu. An row containing a select menu cannot also contain buttons.
Since a message can have up to 5 rows, you can add up to 5 select menus to a message.

# Select Menus

> [!WARNING]
> Component Ids and option values should be unique, as this is what's sent back when a user selects one (or more) option.

Select menus consist of five parts:

-   Id
-   Placeholder
-   Options
-   MinOptions
-   MaxOptions
-   Disabled

The id of the select menu is a settable string, and is specified by the developer. Discord sends this id back in the [interaction object](https://discord.dev/interactions/slash-commands#interaction).

**Placeholder** is a settable string that appears in the select menu when nothing is selected.

**Options** is an array of options for the user to select. Their maximum number in one select menu is 25.
You can let users choose 1 or more options using **MinOptions** and **MaxOptions**.

Options consist of five parts:

-   Label
-   Value
-   Description
-   IsDefault
-   Emoji

Menu creation, for easier understanding, can be divided into two stages:

```cs
// First, create an array of options.
var options = new DiscordSelectComponentOption[]
{
    new DiscordSelectComponentOption("First option", "first_option", "This is the first option, you can add your description of it here.", false, new DiscordComponentEmoji("😀")),
    new DiscordSelectComponentOption("Second option", "second_option", "This is the second option, you can add your description of it here.", false, new DiscordComponentEmoji("😎"))
};

// Now let's create a select menu with the options created above.
var selectMenu = new DiscordSelectComponent("my_select_menu", "Please select one of the options", options);
```

This will create a select menu with two options and the text "Please select one of the options".
When a user select **one** option, `"my_select_menu"` will be sent back as the `Id` property on the event.
This is expanded on in the [how to respond to select menus](#responding-to-select-menus).

You can increase the maximum/minimum number of selections in the select menu constructor. You can also block the select menu, or options.

Description and emoji of options are optional. The label, value and description can be up to 100 characters in length.
The emoji of a option is a [partial emoji object](https://discord.dev/interactions/message-components#component-object), which means that **any valid emoji is usable**, even if your bot does not have access to it's origin server.

## Adding Select Menu

Adding a select menu is no different than adding a button.
We have already created the select menu above, now we will just create a new message builder add the select menu to it.

```cs
var builder = new DiscordMessageBuilder()
    .WithContent("This message has select menu! Pretty neat innit?")
    .AddComponents(selectMenu);
```

Now you have a message with a select menu. Congratulations! It's important to note that `.AddComponents()` will create a new row with each call, so **add everything you want on one row in one call!**

Lets also add a second row with select menu with the ability to choose any number of options.

```cs
var secondOptions = new DiscordSelectComponentOption[]
{
    new DiscordSelectComponentOption("First option", "first_option", "This is the first option, you can add your description of it here.", false, new DiscordComponentEmoji("😀")),
    new DiscordSelectComponentOption("Second option", "second_option", "This is the second option, you can add your description of it here.", false, new DiscordComponentEmoji("😎"))
    new DiscordSelectComponentOption("Third option", "third_option", "This is the third option, you can add your description of it here.", false, new DiscordComponentEmoji("😘"))
};

var secondSelectMenu = new DiscordSelectComponent("my_second_select_menu", "Please select up to 3 options", secondOptions, 1, 3);

builder.AddComponents(secondSelectMenu);
```

And you're done! The select menu will now be sent when the user closes the select menu with 1 to 3 options selected.

## Responding to select menus

When any select menu is pressed, it will fire the [ComponentInteractionCreated](xref:DisCatSharp.DiscordClient.ComponentInteractionCreated) event.

In the event args, `Id` will be the id of the select menu you specified. There's also an `Interaction` property, which contains the interaction the event created. It's important to respond to an interaction within 3 seconds, or it will time out. Responding after this period will throw a `NotFoundException`.

With select menus, there are two new response types: `DeferedMessageUpdate` and `UpdateMessage`.
using `DeferredMessageUpdate` lets you create followup messages via the [followup message builder](xref:DisCatSharp.Entities.DiscordFollowupMessageBuilder).

You have 15 minutes from that point to make followup messages. Responding to that interaction looks like this:

```cs
client.ComponentInteractionCreated += async (s, e) =>
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferedMessageUpdate);
    // Do things.. //
}
```

If you would like to update the message when an select menu option selected, however, you'd use `UpdateMessage` instead, and pass a `DiscordInteractionResponseBuilder` with the new content you'd like.

```cs
client.ComponentInteractionCreated += async (s, e) =>
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("No more select menu for you >:)"));
}
```

This will update the message, and without the infamous <sub>(edited)</sub> next to it. Nice.

# Interactivity

Along with the typical `WaitForMessageAsync` and `WaitForReactionAsync` methods provided by interactivity, there are also select menus implementations as well.

More information about how interactivity works can be found in [the interactivity article](xref:modules_interactivity_introduction)

Since select menus create interactions, there are also two additional properties in the configuration:

-   @DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior
-   @DisCatSharp.Interactivity.InteractivityConfiguration.ResponseMessage

@DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior is what interactivity will do when handling something that isn't a valid valid select menu, in the context of waiting for a specific select menu. It defaults to @DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Ignore, which will cause the interaction fail.

Alternatively, setting it to @DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Ack will acknowledge the select menu, and continue waiting.

@DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Respond will reply with an ephemeral message with the aforementioned response message.

@DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior only applies to the overload accepting a string id of the select menu to wait for.
