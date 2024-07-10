---
uid: topics_components_buttons
title: Buttons
author: DisCatSharp Team
hasDiscordComponents: true
---

# Introduction

Buttons are a feature in Discord based on the interaction framework appended to the bottom of a message which come in several colors.
You will want to familarize yourself with the [message builder](xref:topics_messagebuilder) as it and similar builder objects will be used throughout this article.

With buttons, you can have up to five buttons in a row, and up to five (5) rows of buttons, for a maximum for 25 buttons per message.
Furthermore, buttons come in two types: regular, and link. Link buttons contain a Url field, and are always grey.

# Buttons Continued

> [!WARNING]
> Component (Button) Ids on buttons should be unique, as this is what's sent back when a user presses a button.
>
> Link buttons do **not** have a custom id and do **not** send interactions when pressed.

Buttons consist of five parts:

-   Id
-   Style
-   Label
-   Emoji
-   Disabled

The id of the button is a settable string on buttons, and is specified by the developer. Discord sends this id back in the [interaction object](https://discord.dev/interactions/slash-commands#interaction).

Non-link buttons come in four colors, which are known as styles: Blurple, Grey, Green, and Red. Or as their styles are named: Primary, Secondary, Success, and Danger respectively.

How does one construct a button? It's simple, buttons support constructor and object initialization like so:

```cs
var myButton = new DiscordButtonComponent()
{
    CustomId = "my_very_cool_button",
    Style = ButtonStyle.Primary,
    Label = "Very cool button!",
    Emoji = new DiscordComponentEmoji("😀")
};
```

This will create a blurple button with the text that reads "Very cool button!". When a user pushes it, `"my_very_cool_button"` will be sent back as the `Id` property on the event. This is expanded on in the [how to respond to buttons](#responding-to-button-presses).

The label of a button is optional _if_ an emoji is specified. The label can be up to 80 characters in length.
The emoji of a button is a [partial emoji object](https://discord.dev/interactions/message-components#component-object), which means that **any valid emoji is usable**, even if your bot does not have access to it's origin server.

The disabled field of a button is rather self explanatory. If this is set to true, the user will see a greyed out button which they cannot interact with.

## Adding buttons

> [!NOTE]
> This article will use underscores in button ids for consistency and styling, but spaces are also usable.

Adding buttons to a message is relatively simple. Simply make a builder, and sprinkle some content and the buttons you'd like.

```cs
var builder = new DiscordMessageBuilder();
builder.WithContent("This message has buttons! Pretty neat innit?");
```

Well, there's a builder, but no buttons. What now? Simply make a new button object (`DiscordButtonComponent`) and call `.AddComponents()` on the MessageBuilder.

```cs
var myButton = new DiscordButtonComponent
{
    CustomId = "my_custom_id",
    Label = "This is a button!",
    Style = ButtonStyle.Primary,
};

var builder = new DiscordMessageBuilder()
    .WithContent("This message has buttons! Pretty neat innit?")
    .AddComponents(myButton);
```

Now you have a message with a button. Congratulations! It's important to note that `.AddComponents()` will create a new row with each call, so **add everything you want on one row in one call!**

Buttons can be added in any order you fancy. Lets add 5 to demonstrate each color, and a link button for good measure.

```cs
var builder = new DiscordMessageBuilder()
    .WithContent("This message has buttons! Pretty neat innit?")
    .AddComponents(new DiscordComponent[]
    {
        new DiscordButtonComponent(ButtonStyle.Primary, "1_top", "Primary"),
        new DiscordButtonComponent(ButtonStyle.Secondary, "2_top", "Secondary"),
        new DiscordButtonComponent(ButtonStyle.Success, "3_top", "Success"),
        new DiscordButtonComponent(ButtonStyle.Danger, "4_top", "Danger"),
        new DiscordLinkButtonComponent("https://some-super-cool.site", "Link")
    });
```

As promised, not too complicated. Links however are `DiscordLinkButtonComponent`, which takes a URL as it's first parameter, and the label. Link buttons can also have an emoji, like regular buttons.

Lets also add a second row of buttons, but disable them, so the user can't push them all willy-nilly.

```cs
builder.AddComponents(new DiscordComponent[]
{
    new DiscordButtonComponent(ButtonStyle.Primary, "1_top_d", "Primary", true),
    new DiscordButtonComponent(ButtonStyle.Secondary, "2_top_d", "Secondary", true),
    new DiscordButtonComponent(ButtonStyle.Success, "3_top_d", "Success", true),
    new DiscordButtonComponent(ButtonStyle.Danger, "4_top_d", "Danger", true),
    new DiscordLinkButtonComponent("https://some-super-cool.site", "Link", true)
});
```

Practically identical, but now with `true` as an extra parameter. This is the `Disabled` property.

Produces a message like such:

<discord-messages>
    <discord-message profile="dcs">
    Omg buttons! Poggers :3
        <discord-attachments slot="components">
            <discord-action-row>
                <discord-button type="primary">Primary</discord-button>
                <discord-button type="secondary">Secondary</discord-button>
                <discord-button type="success">Success</discord-button>
                <discord-button type="destructive">Danger</discord-button>
                <discord-button url="https://discord.gg/2HWta4GXus">Link</discord-button>
            </discord-action-row>
            <discord-action-row>
                <discord-button type="primary" disabled>Primary</discord-button>
                <discord-button type="secondary" disabled>Secondary</discord-button>
                <discord-button type="success" disabled>Success</discord-button>
                <discord-button type="destructive" disabled>Danger</discord-button>
                <discord-button url="https://discord.gg/2HWta4GXus" disabled>Link</discord-button>
            </discord-action-row>
        </discord-attachments>
    </discord-message>
</discord-messages>

Well, that's all neat, but lets say you want to add an emoji. Being able to use any emoji is pretty neat, after all. That's also very simple!

```cs
var myButton = new DiscordButtonComponent
(
    ButtonStyle.Primary,
    "emoji_button",
    "OwO",
    false,
    new DiscordComponentEmoji(922569846569455646)
);
```

And you're done! Simply add that to a builder, and when you send, you'll get a message that has a button with our cute logo!

<discord-messages>
    <discord-message profile="dcs">
    What's this? Emojis in buttons??
        <discord-attachments slot="components">
            <discord-action-row>
                <discord-button type="primary" emoji="https://cdn.discordapp.com/emojis/922569846569455646.webp?size=96&quality=lossless" emoji-name="dcs">OwO</discord-button>
            </discord-action-row>
    </discord-message>
</discord-messages>

## Responding to button presses

When any button is pressed, it will fire the [ComponentInteractionCreated](xref:DisCatSharp.DiscordClient.ComponentInteractionCreated) event.

In the event args, `Id` will be the id of the button you specified. There's also an `Interaction` property, which contains the interaction the event created. It's important to respond to an interaction within 3 seconds, or it will time out. Responding after this period will throw a `NotFoundException`.

With buttons, there are two new response types: `DeferredMessageUpdate` and `UpdateMessage`.
using `DeferredMessageUpdate` lets you create followup messages via the [followup message builder](xref:DisCatSharp.Entities.DiscordFollowupMessageBuilder). The button will return to being in it's 'dormant' state, or it's 'unpushed' state, if you will.

You have 15 minutes from that point to make followup messages. Responding to that interaction looks like this:

```cs
client.ComponentInteractionCreated += async (s, e) =>
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    // Do things.. //
}
```

If you would like to update the message when a button is pressed, however, you'd use `UpdateMessage` instead, and pass a `DiscordInteractionResponseBuilder` with the new content you'd like.

```cs
client.ComponentInteractionCreated += async (s, e) =>
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("No more buttons for you >:)"));
}
```

This will update the message, and without the infamous <sub>(edited)</sub> next to it. Nice.

# Interactivity

Along with the typical `WaitForMessageAsync` and `WaitForReactionAsync` methods provided by interactivity, there are also button implementations as well.

More information about how interactivity works can be found in [the interactivity article](xref:modules_interactivity_introduction)

Since buttons create interactions, there are also two additional properties in the configuration:

-   @DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior
-   @DisCatSharp.Interactivity.InteractivityConfiguration.ResponseMessage

@DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior is what interactivity will do when handling something that isn't a valid valid button, in the context of waiting for a specific button. It defaults to @DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Ignore, which will cause the interaction fail.

Alternatively, setting it to @DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Ack will acknowledge the button, and continue waiting.

@DisCatSharp.Interactivity.Enums.InteractionResponseBehavior.Respond will reply with an ephemeral message with the aforementioned response message.

@DisCatSharp.Interactivity.InteractivityConfiguration.ResponseBehavior only applies to the overload accepting a string id of the button to wait for.
