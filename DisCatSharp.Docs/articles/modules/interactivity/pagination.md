---
uid: modules_interactivity_pagination
title: Pagination
---

# Pagination

Interactivity supports both classic reaction pagination and newer button-based pagination.
The important part is that pagination works on a collection of `Page` objects.

## Building pages

The modern way is to create empty `Page` instances and fill them with the layout you want.

```cs
var pages = new List<Page>
[
    new Page()
        .WithContent("First page")
        .WithEmbed(new DiscordEmbedBuilder()
            .WithTitle("Overview")
            .WithDescription("This page uses the classic content + embed layout.")),
    new Page()
        .WithEmbed(new DiscordEmbedBuilder()
            .WithTitle("Second page")
            .WithDescription("You can also use only an embed if that is enough."))
];
```

Classic pages may use:

- content only
- an embed only
- content and an embed together

## CV2 component pages

`Page` also supports Components V2 content through `AddComponents(...)`.
This is the right option when your page content should be built out of V2 components instead of plain content or embeds.

```cs
var pages = new List<Page>
[
    new Page().AddComponents(
        new DiscordContainerComponent(
        [
            new DiscordTextDisplayComponent("## Interactivity"),
            new DiscordSeparatorComponent(true, SeparatorSpacingSize.Small),
            new DiscordTextDisplayComponent("This page uses Components V2 layout.")
        ],
        accentColor: new DiscordColor("#EC4CA5"))),
    new Page().AddComponents(
        new DiscordContainerComponent(
        [
            new DiscordTextDisplayComponent("## Another page"),
            new DiscordTextDisplayComponent("The paginator will still add its navigation controls for you.")
        ]))
];
```

> [!IMPORTANT]
> A `Page` cannot use classic content or embeds at the same time as CV2 components.
> Calling `AddComponents(...)` switches the page into CV2 mode, so do **not** mix it with `WithContent(...)` or `WithEmbed(...)`.

## Generating pages from text

The old helpers still exist and are fine for quick text pagination:

```cs
var interactivity = ctx.Client.GetInteractivity();

var pages = interactivity.GeneratePagesInEmbed(longText);
// or:
var contentPages = interactivity.GeneratePagesInContent(longText);
```

Both helpers support character-based splitting and line-based splitting through `SplitType`, but they are convenience helpers, not the only way to build pages.

## Button pagination in a channel

The simplest `SendPaginatedMessageAsync()` overload sends button controls using the configuration defaults.

```cs
var pages = new List<Page>
[
    new Page().WithEmbed(new DiscordEmbedBuilder().WithTitle("Page 1").WithDescription("Classic embed page")),
    new Page().WithEmbed(new DiscordEmbedBuilder().WithTitle("Page 2").WithDescription("Another embed page"))
];

await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
```

You can also paginate CV2 pages the same way:

```cs
var cv2Pages = new List<Page>
[
    new Page().AddComponents(
        new DiscordContainerComponent(
        [
            new DiscordTextDisplayComponent("## Page 1"),
            new DiscordTextDisplayComponent("This is a CV2 page.")
        ])),
    new Page().AddComponents(
        new DiscordContainerComponent(
        [
            new DiscordTextDisplayComponent("## Page 2"),
            new DiscordTextDisplayComponent("Still paginated with the same API.")
        ]))
];

await ctx.Channel.SendPaginatedMessageAsync(ctx.User, cv2Pages);
```

You can still provide custom `PaginationButtons`, timeout, behavior, and cleanup rules.

## Reaction pagination in a channel

If you want reaction controls instead, pass `PaginationEmojis`.
This path is still based on regular page content rather than CV2 page layouts.

```cs
var pages = new List<Page>
[
    new Page().WithContent("Page 1"),
    new Page().WithContent("Page 2"),
    new Page().WithContent("Page 3")
];

await ctx.Channel.SendPaginatedMessageAsync(
    ctx.User,
    pages,
    new PaginationEmojis(),
    timeoutOverride: TimeSpan.FromMinutes(2));
```

Reaction pagination requires the appropriate reaction intents.

## Pagination in interaction responses

For slash commands or other interaction-driven flows, use `SendPaginatedResponseAsync()`:

```cs
var pages = new List<Page>
[
    new Page().WithEmbed(new DiscordEmbedBuilder().WithTitle("Page 1").WithDescription("Interaction pagination")),
    new Page().WithEmbed(new DiscordEmbedBuilder().WithTitle("Page 2").WithDescription("Still uses Page objects"))
];

await ctx.Interaction.SendPaginatedResponseAsync(
    deferred: false,
    ephemeral: true,
    user: ctx.User,
    pages: pages);
```

This is especially useful when you want a paginated response without sending a separate channel message first.

## Useful pagination settings

| Setting | Purpose |
| --- | --- |
| `PaginationBehaviour.WrapAround` | Loop from the end back to the beginning |
| `PaginationBehaviour.Ignore` | Stop moving when the user reaches the first or last page |
| `PaginationDeletion.DeleteEmojis` | Remove reaction controls after timeout |
| `PaginationDeletion.KeepEmojis` | Leave reaction controls in place |
| `PaginationDeletion.DeleteMessage` | Delete the whole reaction pagination message |
| `ButtonPaginationBehavior.Disable` | Disable buttons on timeout |
| `ButtonPaginationBehavior.DeleteButtons` | Remove buttons from the message |
| `ButtonPaginationBehavior.DeleteMessage` | Delete the whole button pagination message |

The defaults come from `InteractivityConfiguration`, so you can tune them once and reuse them across your bot.
