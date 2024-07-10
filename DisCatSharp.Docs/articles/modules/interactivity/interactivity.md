---
uid: modules_interactivity_introduction
title: Interactivity Introduction
hasDiscordComponents: true
---

# Introduction to Interactivity

Interactivity will enable you to write commands which the user can interact with through reactions and messages.
The goal of this article is to introduce you to the general flow of this extension.

Make sure to install the `DisCatSharp.Interactivity` package from NuGet before continuing.

![Interactivity NuGet](/images/interactivity_01.png)

## Enabling Interactivity

Interactivity can be registered using the `DiscordClient#UseInteractivity()` extension method.<br/>
Optionally, you can also provide an instance of `InteractivityConfiguration` to modify default behaviors.

```cs
var discord = new DiscordClient();

discord.UseInteractivity(new InteractivityConfiguration()
{
    PollBehaviour = PollBehaviour.KeepEmojis,
    Timeout = TimeSpan.FromSeconds(30)
});
```

## Using Interactivity

There are two ways available to use interactivity:

-   Extension methods available for `DiscordChannel`, `DiscordMessage`, `DiscordClient` and `DiscordInteraction`.
-   [Instance methods](xref:DisCatSharp.Interactivity.InteractivityExtension#methods) available from `InteractivityExtension`.

We'll have a quick look at a few common interactivity methods along with an example of use for each.

The first (and arguably most useful) extension method is `SendPaginatedMessageAsync` for `DiscordChannel`.

This method displays a collection of _'pages'_ which are selected one-at-a-time by the user through reaction buttons.
Each button click will move the page view in one direction or the other until the timeout is reached.

You'll need to create a collection of pages before you can invoke this method.
This can be done easily using the `GeneratePagesInEmbed` and `GeneratePagesInContent` instance methods from `InteractivityExtension`.

Alternatively, for pre-generated content, you can create and add individual instances of `Page` to a collection.

This example will use the `GeneratePagesInEmbed` method to generate the pages.

```cs
public async Task PaginationCommand(CommandContext ctx)
{
    var reallyLongString = "Lorem ipsum dolor sit amet, consectetur adipiscing ..."

    var interactivity = ctx.Client.GetInteractivity();
    var pages = interactivity.GeneratePagesInEmbed(reallyLongString);

    await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
}
```

<discord-messages>
    <discord-message profile="dcs">
        <discord-embed slot="embeds">
            <discord-embed-description slot="description">Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero</discord-embed-description>
        </discord-embed>
        <discord-attachments slot="components">
            <discord-action-row>
                <discord-button type="secondary">⏮️</discord-button>
                <discord-button type="secondary">◀️</discord-button>
                <discord-button type="secondary">⏹️</discord-button>
                <discord-button type="secondary">▶️</discord-button>
                <discord-button type="secondary">⏭️</discord-button>
            </discord-action-row>
        </discord-attachments>
    </discord-message>
</discord-messages>

Next we'll look at the `WaitForReactionAsync` extension method for `DiscordMessage`.
This method waits for a reaction from a specific user and returns the emoji that was used.

An overload of this method also enables you to wait for a _specific_ reaction, as shown in the example below.

```cs
public async Task ReactionCommand(CommandContext ctx, DiscordMember member)
{
    var emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
    var message = await ctx.RespondAsync($"Hey {member.Mention}, react with {emoji}!");

    var result = await message.WaitForReactionAsync(member, emoji);

    if (!result.TimedOut) await ctx.RespondAsync("Thanks :3");
}
```

<discord-messages>
    <discord-message profile="dcs" highlight>
         Hey <discord-mention highlight profile="user">Discord User</discord-mention>, react with 👌!
         <discord-reactions slot="reactions">
            <discord-reaction name="👌" emoji="/images/ok_hand.svg" count="1" reacted></discord-reaction>
        </discord-reactions>
    </discord-message>
    <discord-message profile="dcs">
         Thanks :3
    </discord-message>
</discord-messages>

Another reaction extension method for `DiscordMessage` is `CollectReactionsAsync`.
As the name implies, this method collects all reactions on a message until the timeout is reached.

```cs
public async Task CollectionCommand(CommandContext ctx)
{
    var message = await ctx.RespondAsync("React here!");
    var reactions = await message.CollectReactionsAsync();

    var stringBuilder = new StringBuilder();
    foreach (var reaction in reactions)
        stringBuilder.AppendLine($"{reaction.Emoji}: {reaction.Total}");

    await ctx.RespondAsync(stringBuilder.ToString());
}
```

<discord-messages>
    <discord-message profile="dcs">
        React here!
        <discord-reactions slot="reactions">
            <discord-reaction name="uwu" emoji="https://cdn.discordapp.com/emojis/859022252372787241.png" count="3" reacted></discord-reaction>
            <discord-reaction name="👌" emoji="/images/ok_hand.svg" count="1"></discord-reaction>
        </discord-reactions>
    </discord-message>
    <discord-message profile="dcs">
        👌: 1<br/>
        <discord-custom-emoji name="uwu" url="https://cdn.discordapp.com/emojis/859022252372787241.png"></discord-custom-emoji>: 3
    </discord-message>
</discord-messages>

The final one we'll take a look at is the `GetNextMessageAsync` extension method for `DiscordMessage`.

This method will return the next message sent from the author of the original message.
Our example here will use its alternate overload which accepts an additional predicate.

```cs
public async Task ActionCommand(CommandContext ctx)
{
    await ctx.RespondAsync("Respond with `confirm` to continue.");
    var result = await ctx.Message.GetNextMessageAsync(m =>
    {
        return m.Content.ToLower() == "confirm";
    });

    if (!result.TimedOut)
        await ctx.RespondAsync("OwO, thanks for confirming <3");
}
```

<discord-messages>
    <discord-message profile="dcs">Respond with <discord-inline-code>confirm</discord-inline-code> to continue.</discord-message>
    <discord-message profile="user">confirm</discord-message>
    <discord-message profile="dcs">OwO, thanks for confirming <3</discord-message>
</discord-messages>
