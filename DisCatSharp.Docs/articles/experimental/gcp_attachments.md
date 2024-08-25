---
uid: experimental_gcp_attachments
title: GCP Attachments
author: DisCatSharp Team
hasDiscordComponents: false
---

# GCP Attachments

## Overview

The GCP (Google Cloud Platform) Attachments feature in DisCatSharp allows for directzly uploading files to Discord via Google Cloud Storage. This feature is experimental, and while it offers advanced file management options, it is not officially supported or endorsed by Discord. Use this feature with caution, as it may break or become unsupported in the future.

## Uploading Files

To upload a file to Discord using GCP Attachments, you can use the `UploadFileAsync` method provided in the `DiscordChannelMethodHooks` class. This method handles the file upload and returns the necessary information for later use in a `DiscordMessageBuilder`.

### Example: Uploading a File

```cs
using DisCatSharp.Experimental.Entities;

// ...

public async Task UploadFileExample(DiscordChannel channel, string filePath)
{
    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    var uploadInfo = await channel.UploadFileAsync("my_file.txt", fileStream, "My file description");

    // Now you can use this upload information to send a message
    var msg = new DiscordMessageBuilder()
        .WithContent("Here's the file you uploaded!")
        .AddGcpAttachment(uploadInfo)
        .SendAsync(channel);
}
```

## Adding GCP Attachments to a Message

Once a file is uploaded, it can be attached to a message using the `AddGcpAttachment` method in the `DiscordMessageBuilderMethodHooks` class. This method takes a `GcpAttachmentUploadInformation` object, which contains the details of the uploaded file.

### Example: Sending a Message with an Uploaded File

```cs
using DisCatSharp.Experimental.Entities;

// ...

public async Task SendFileMessage(DiscordChannel channel, GcpAttachmentUploadInformation uploadInfo)
{
    var msg = await new DiscordMessageBuilder()
        .WithContent("Here's the file!")
        .AddGcpAttachment(uploadInfo)
        .SendAsync(channel);
}
```

## Conclusion

The GCP Attachments feature provides powerful file management capabilities for advanced use cases. However, due to its experimental nature, developers should use it carefully and be aware of the risks associated with using unsupported features.
