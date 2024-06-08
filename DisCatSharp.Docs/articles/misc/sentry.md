---
uid: misc_sentry
title: Sentry
author: DisCatSharp Team
---

# Sentry Integration in DisCatSharp

## Overview

Sentry is an error tracking tool that helps developers monitor and fix crashes in real-time. In DisCatSharp, Sentry is utilized to report and analyze errors and missing fields, ensuring a more robust and reliable library.

## How Sentry is Used in DisCatSharp

The Sentry integration in DisCatSharp is configured to capture and report exceptions, missing fields, and other critical information. This data helps developers identify and fix issues promptly. The integration is controlled by several configuration options within the `DiscordConfiguration` class.

## Configuration Options

The following configuration options are available for Sentry, and they are all **disabled** or set to **null** by default:

-   **ReportMissingFields**: Determines whether to report missing fields for Discord objects. This is useful for library development.

    ```csharp
    public bool ReportMissingFields { internal get; set; } = false;
    ```

-   **EnableSentry**: Enables the Sentry integration.

    ```csharp
    public bool EnableSentry { internal get; set; } = false;
    ```

-   **AttachRecentLogEntries**: If enabled, attaches the recent log entries.

    > [!IMPORTANT]
    > Please be mindful about how much information you log, if you enabled this. We might be able to see unwanted things.

    ```csharp
    public bool AttachRecentLogEntries { internal get; set; } = false;
    ```

-   **AttachUserInfo**: If enabled, attaches the bot's username and ID to Sentry reports to help pinpoint problems.

    ```csharp
    public bool AttachUserInfo { internal get; set; } = false;
    ```

-   **FeedbackEmail**: An email address that can be used to reach out when the bot encounters library bugs. It is only transmitted if `AttachUserInfo` is enabled.

    ```csharp
    public string? FeedbackEmail { internal get; set; } = null;
    ```

-   **DeveloperUserId**: The Discord user ID for contacting the developer when the bot encounters library bugs. It is only transmitted if `AttachUserInfo` is enabled.

    ```csharp
    public ulong? DeveloperUserId { internal get; set; } = null;
    ```

-   **EnableDiscordIdScrubber**: Optional, whether to additionally scrub discord-based ids from logs. Defaults to `false` since that isn't needed in most cases. Set it to `true` if you want additional filtering.
    ```csharp
    public bool EnableDiscordIdScrubber { internal get; set; } = false;
    ```

## Data Transmitted to Sentry

When an error or missing field is detected, the following data is transmitted to Sentry:

1. **Event Metadata**: Includes SDK information, event ID, and timestamps.

    ```json
    {
    	"sdk": {
    		"name": "sentry.dotnet",
    		"version": "4.5.0"
    	},
    	"event_id": "d9d303e3d75d400e992e1b1d7aef6641",
    	"timestamp": "2024-05-16T19:58:31.6006568+00:00"
    }
    ```

2. **Exception Details**: Information about the exception, including the type, value, module, and stack trace.

    ```json
    {
    	"exception": {
    		"values": [
    			{
    				"type": "DisCatSharp.Exceptions.BadRequestException",
    				"value": "Bad request: BadRequest",
    				"module": "DisCatSharp, Version=10.6.3.0, Culture=neutral, PublicKeyToken=null",
    				"stacktrace": {
    					"frames": [
    						{
    							"filename": "Entities\\Command.cs",
    							"function": "async Task<CommandResult> Command.ExecuteAsync(CommandContext ctx)",
    							"lineno": 100
    						}
    					]
    				}
    			}
    		]
    	}
    }
    ```

3. **Breadcrumbs**: Log entries leading up to the error, providing context for the issue.

    ```json
    {
    	"breadcrumbs": [
    		{
    			"timestamp": "2024-05-16T19:58:09.814Z",
    			"message": "Release notes disabled by config",
    			"category": "DisCatSharp.BaseDiscordClient"
    		}
    	]
    }
    ```

4. **User Information**: If `AttachUserInfo` is enabled, the bot's username, ID, and developer contact details.
    ```json
    {
    	"user": {
    		"id": "822242444070092860",
    		"username": "nyuw#7780",
    		"ip_address": "{{auto}}",
    		"other": {
    			"developer": "856780995629154305",
    			"email": "aiko@aitsys.dev"
    		}
    	}
    }
    ```

## Ensuring User Privacy

No sensitive data, such as tokens or user messages, is logged or transmitted to Sentry. The only potentially sensitive data included are the bot's username and ID, which are only sent if explicitly enabled in the configuration. Additionally, steps have been taken to ensure that log lines sent with bad request reports do not contain sensitive information.

Furthermore, while the JSON payload includes the field `"ip_address": "{{auto}}"`, no actual IP addresses are transmitted.

## Client-Side Filters

To enhance safety and ensure no sensitive information is leaked, various filters have been implemented on the client side:

1. **StripTokens Utility**: This utility function removes any Discord-based tokens from strings before they might be sent to Sentry.

    ```csharp
    public static string? StripTokens(string? str)
    {
       if (string.IsNullOrWhiteSpace(str))
          return str;

       str = Regex.Replace(str, @"([a-zA-Z0-9]{68,})", "{WEBHOOK_OR_INTERACTION_TOKEN}"); // Any alphanumeric string this long is likely to be sensitive information anyways
       str = Regex.Replace(str, @"(mfa\\.[a-z0-9_-]{20,})|((?<botid>[a-z0-9_-]{23,28})\\.(?<creation>[a-z0-9_-]{6,7})\\.(?<enc>[a-z0-9_-]{27,}))", "{BOT_OR_USER_TOKEN}");

       return str;
    }
    ```

2. **StripIds Utility**: This utility function removes any Discord-based tokens from strings before they might be sent to Sentry.

    ```csharp
    public static string? StripIds(string? str, bool strip)
    {
    	if (string.IsNullOrWhiteSpace(str) || !strip)
    		return str;

    	str = DiscordRegEx.IdRegex().Replace(str, "{DISCORD_ID}");

    	return str;
    }
    ```

3. **Breadcrumb Filter**: Filters out sensitive information from breadcrumb logs before sending them to Sentry.

    ```csharp
    options.SetBeforeBreadcrumb(b
    => new(Utilities.StripIds(Utilities.StripTokens(b.Message), this.Configuration.EnableDiscordIdScrubber)!,
    	b.Type!,
    	b.Data?.Select(x => new KeyValuePair<string, string>(x.Key, Utilities.StripIds(Utilities.StripTokens(x.Value), this.Configuration.EnableDiscordIdScrubber)!))
          b.Category,
          b.Level));
    ```

4. **Transaction Filter**: Ensures that sensitive information is not included in transaction data sent to Sentry.

    ```csharp
    options.SetBeforeSendTransaction(tr =>
    {
       if (tr.Request.Data is string str)
          tr.Request.Data = Utilities.StripIds(Utilities.StripTokens(str), this.Configuration.EnableDiscordIdScrubber);

       return tr;
    });
    ```

By maintaining these practices, DisCatSharp ensures user privacy while leveraging Sentry to improve the library's reliability and performance.

For more information on configuring and using Sentry in DisCatSharp, refer to the official [announcement](https://docs.dcs.aitsys.dev/changelogs/v10/10_6_0#sentry-integration).
