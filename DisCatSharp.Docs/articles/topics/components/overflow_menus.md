---
uid: topics_components_overflow_menus
title: Overflow Menus
---

# Overflow menus

Overflow menus are an alpha Components V2 message component. They let a message group up to eight sets of actions behind one compact menu. A menu can appear in an action row or as the accessory to a section. Each group contains up to eight menu action items.

Default and danger items send a component interaction with `ComponentType.MenuActionItem`; link items open their URL and do not send an interaction. Interactive items need a custom ID, while link items need a URL.

```cs
var menu = new DiscordOverflowMenuComponent(
[
    new DiscordOverflowMenuGroup(
    [
        new DiscordMenuActionItemComponent(
            MenuActionItemStyle.Default,
            "Edit",
            customId: "edit"),
        new DiscordMenuActionItemComponent(
            MenuActionItemStyle.Danger,
            "Delete",
            customId: "delete",
            description: "Permanently remove this entry")
    ],
    label: "Manage"),
    new DiscordOverflowMenuGroup(
    [
        new DiscordMenuActionItemComponent(
            MenuActionItemStyle.Link,
            "Documentation",
            url: "https://discord.com/developers/docs")
    ])
]);

var builder = new DiscordMessageBuilder()
    .WithV2Components()
    .AddComponents(new DiscordActionRowComponent([menu]));

await channel.SendMessageAsync(builder);
```

You can also place it beside section text:

```cs
var section = new DiscordSectionComponent(
[
    new DiscordTextDisplayComponent("Manage this entry from the menu.")
]).WithOverflowMenuComponent(menu);

var builder = new DiscordMessageBuilder()
    .WithV2Components()
    .AddComponents(new DiscordContainerComponent([section]));
```

Handle interactive item selections through `DiscordClient.ComponentInteractionCreated`. The interaction data includes `ComponentType.MenuActionItem` and the selected item's `CustomId`:

```cs
client.ComponentInteractionCreated += async (_, e) =>
{
    if (e.Interaction.Data.ComponentType is not ComponentType.MenuActionItem)
        return;

    switch (e.Id)
    {
        case "edit":
            await e.Interaction.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("Editing…")
                    .AsEphemeral());
            break;
    }
};
```

Menu groups allow up to eight items, group labels are limited to 80 characters, item labels to 80 characters, descriptions to 100 characters, and custom IDs to 100 characters. Link items cannot have a custom ID. Overflow menus require Components V2 and count toward Discord's 40-component message limit.
