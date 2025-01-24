---
uid: experimental_search_guild_members
title: ElasticSearch-Based Member Search
author: DisCatSharp Team
hasDiscordComponents: false
---

# ElasticSearch-Based Member Search

## Overview

With the introduction of ElasticSearch in DisCatSharp, the member search functionality has become more powerful, allowing for complex queries to find members within a guild. This feature leverages ElasticSearch to return supplemental guild member objects that match specific criteria. It’s a significant upgrade from the previous search methods but requires a good understanding of ElasticSearch's capabilities and limitations.

### Prerequisites

- Requires the `MANAGE_GUILD` permission.
- The guild must be indexed in ElasticSearch. If the guild is not yet indexed, the request will return a `202 Accepted` response, indicating that the results are not immediately available. We will throw a `NotIndexedException` here.

### Validating Search Parameters

Given the complexity of the queries, a robust validation system has been implemented to ensure that user input adheres to the expected formats and limits. The validation checks:

- **Limit**: Ensures the `limit` parameter is between 1 and 1000.
- **OR Queries**: Validates that `OR` queries are only used where allowed and checks the number of items and types.
- **AND Queries**: Validates that `AND` queries are only used where allowed and checks the number of items and types.
- **Range Queries**: Ensures that `range` queries are only used with valid types like `snowflake` or `integer`.

If the search parameters do not meet these conditions, a `ValidationException` is thrown.

### Using the Member Search

To use the member search, you need to construct a `DiscordGuildMemberSearchParams` object and pass it to the `SearchMembersAsync` method on a `DiscordGuild` object.

```cs
using DisCatSharp.Experimental.Entities;

// ...

var searchParams = new DiscordGuildMemberSearchParams
{
    Limit = 10,
    OrQuery = new DiscordMemberFilter
    {
        Usernames = new DiscordQuery { OrQuery = new List<string> { "username1", "username2" } },
        RoleIds = new DiscordQuery { OrQuery = new List<string> { "123456789012345678" } }
    }
};

try
{
    var searchResponse = await guild.SearchMembersAsync(searchParams);
    foreach (var member in searchResponse.Members)
    {
        // Process each member
    }
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.ErrorMessage}");
}
catch (NotIndexedException ex)
{
    Console.WriteLine($"The guild is not yet indexed. Retry after: {ex.RetryAfter} seconds.");
}
```

### Handling Responses

The `SearchMembersAsync` method returns a `DiscordSearchGuildMembersResponse` object, which contains the following:

- `GuildId`: The ID of the guild searched.
- `Members`: A list of supplemental guild member objects matching the search criteria.
- `PageResultCount`: The number of results returned in the current page.
- `TotalResultCount`: The total number of results found.

### Example Scenarios

#### Search by Invite Code

```cs
using DisCatSharp.Experimental.Entities;

// ...

var searchParams = new DiscordGuildMemberSearchParams
{
    OrQuery = new DiscordMemberFilter
    {
        SourceInviteCode = new DiscordQuery { OrQuery = new List<string> { "inviteCode123" } }
    }
};
var searchResponse = await guild.SearchMembersAsync(searchParams);
```

#### Search by Role and Username

```cs
using DisCatSharp.Experimental.Entities;

// ...

var searchParams = new DiscordGuildMemberSearchParams
{
    AndQuery = new DiscordMemberFilter
    {
        RoleIds = new DiscordQuery { AndQuery = new List<string> { "roleId123" } }
    },
    OrQuery = new DiscordMemberFilter
    {
        Usernames = new DiscordQuery { OrQuery = new List<string> { "username1" } }
    }
};
var searchResponse = await guild.SearchMembersAsync(searchParams);
```
