---
uid: experimental_preamble
title: Experimental Features in DisCatSharp
author: DisCatSharp Team
hasDiscordComponents: false
---

# Experimental Features in DisCatSharp

## Overview

DisCatSharp introduces several experimental features to provide advanced capabilities and customization options. These features are not officially supported or endorsed by Discord, and they may break or change without notice. Use these features at your own risk, and be prepared for potential instability.

### Important Notes

- **Experimental Nature**: Features in the experimental package are subject to change or removal in future updates.
- **No Official Support**: Discord does not officially support these features, meaning they may not work as expected, and updates from Discord might break functionality.
- **Potential for Breaking Changes**: Given the experimental status, updates to the library could introduce breaking changes without warning.

### Enabling Experimental Features

To use experimental features, you need to reference the `DisCatSharp.Experimental` namespace. Some experimental methods require specific features or treatments to be enabled on your Discord account or guild.

### Examples of Experimental Features

#### Elasticsearch-Based Member Search

This feature allows you to perform advanced member searches within a guild using Elasticsearch. This functionality is especially useful for large servers where filtering members by various criteria is necessary.

```cs
using DisCatSharp.Experimental.Entities;

// ...

var searchParams = new DiscordGuildMemberSearchParams
{
    Limit = 100,
    Sort = MemberSortType.JoinedAtDesc
};

var searchResponse = await guild.SearchMembersAsync(searchParams);
```

#### GCP Attachments

The GCP Attachments feature allows you to directly upload files to Discord via Google Cloud Storage. This is particularly useful for managing large files. However, this feature is experimental and should be used with caution.

```cs
using DisCatSharp.Experimental.Entities;

// Uploading a file to Discord using GCP
public async Task UploadFileExample(DiscordChannel channel, string filePath)
{
    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    var uploadInfo = await channel.UploadFileAsync("my_file.txt", fileStream, "My file description");

    // Sending the file as an attachment in a message
    var msg = new DiscordMessageBuilder()
        .WithContent("Here's the file you uploaded!")
        .AddGcpAttachment(uploadInfo)
        .SendAsync(channel);
}
```

### Handling Errors in Experimental Features

Since experimental features can be unstable, it's essential to handle exceptions appropriately. Some common exceptions include:

- **ValidationException**: Thrown when input parameters do not meet the expected format or constraints.
- **NotIndexedException**: Thrown when the Elasticsearch index is not yet available for the guild.
- **UnauthorizedException**: Thrown when the bot lacks the necessary permissions to perform the requested action.

### Conclusion

Experimental features in DisCatSharp offer powerful new capabilities but come with risks. Ensure you test thoroughly in a development environment before deploying these features in production. Always keep an eye on library updates, as breaking changes can occur.
