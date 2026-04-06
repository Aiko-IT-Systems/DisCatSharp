---
uid: experimental_search_guild_messages
title: ElasticSearch-Based Message Search
author: DisCatSharp Team
hasDiscordComponents: false
---

# ElasticSearch-Based Message Search

## Overview

DisCatSharp.Experimental includes a guild message search API backed by Discord's ElasticSearch-powered search endpoint. This feature is experimental, unsupported by Discord, and may change or break without notice.

The entry point is [SearchMessagesAsync](xref:DisCatSharp.Experimental.Entities.DiscordGuildMethodsHook.SearchMessagesAsync*).

## Prerequisites

- Reference the `DisCatSharp.Experimental` package and namespace.
- The bot must have permission to use the underlying Discord search endpoint.
- The guild must be indexed. If Discord returns `202 Accepted`, DisCatSharp throws a [NotIndexedException](xref:DisCatSharp.Exceptions.NotIndexedException).

## Searching Messages

To search messages, create a [DiscordGuildMessageSearchParams](xref:DisCatSharp.Experimental.Entities.DiscordGuildMessageSearchParams) instance and pass it to `SearchMessagesAsync`.

```cs
using DisCatSharp.Experimental.Entities;
using DisCatSharp.Experimental.Enums;

// ...

var searchParams = new DiscordGuildMessageSearchParams
{
    Content = "release notes",
    AuthorIds = [123456789012345678],
    ChannelIds = [987654321098765432],
    Has = [HasOption.Link, HasOption.Image],
    SortBy = SortingMode.Timestamp,
    SortOrder = SortingOrder.Descending,
    Limit = 25
};

var response = await guild.SearchMessagesAsync(searchParams);
```

## Common Filters

The message search parameters support a broad set of filters, including:

- message content
- author ids and author types
- mentions and `@everyone`
- message id ranges
- result limit and offset
- attachment, embed, and link filters
- pinned state
- channel ids
- NSFW inclusion

Some fields are marked experimental on Discord's side as well, so invalid combinations may still be rejected by Discord with a bad request response.

## Handling the Response

The response is a [DiscordSearchGuildMessagesResponse](xref:DisCatSharp.Experimental.Entities.DiscordSearchGuildMessagesResponse).

It includes:

- `Messages`, returned as nested message groups
- `Threads`, when matching messages belong to threads
- `Members`, containing thread member data when Discord includes it
- `TotalResults`
- `AnalyticsId`
- `DoingDeepHistoricalIndex`
- `DocumentsIndexed`

If you want a flat list of messages, flatten the nested groups:

```cs
var response = await guild.SearchMessagesAsync(searchParams);

if (response is not null)
{
    var messages = response.Messages.SelectMany(x => x);

    foreach (var message in messages)
        Console.WriteLine($"{message.Author.Username}: {message.Content}");
}
```

## Error Handling

Because the endpoint is experimental, make sure you handle failures explicitly:

```cs
try
{
    var response = await guild.SearchMessagesAsync(searchParams);
}
catch (NotIndexedException ex)
{
    Console.WriteLine($"Guild search index is not ready yet. Retry after {ex.RetryAfter} seconds.");
}
catch (BadRequestException ex)
{
    Console.WriteLine($"Discord rejected the search: {ex.JsonMessage}");
}
```

## Recommendations

- Treat this feature as best-effort and experimental.
- Expect Discord to reject unsupported parameter combinations.
- Prefer small, focused queries first, then paginate or broaden the search if needed.
