---
uid: misc_sentry
title: Sentry
author: DisCatSharp Team
---

# Sentry Integration in DisCatSharp

## Overview

Sentry is an error tracking tool that helps developers monitor and fix crashes in real time. In DisCatSharp, Sentry is used to report and analyze errors, missing fields, and operational patterns — helping keep the library more robust and reliable.

## Architecture

DisCatSharp uses a centralized **diagnostics sink** abstraction (`ILibraryDiagnosticsSink`) to isolate all library-side telemetry from consumer applications. This design ensures that:

-   Library telemetry never interferes with the consumer's own Sentry or logging setup.
-   Each `DiscordClient` (or `DiscordShardedClient`) gets its own per-client Sentry instance — no global state mutation.
-   When Sentry is disabled, a zero-cost no-op sink is used instead.
-   Extension packages (e.g., `DisCatSharp.Lavalink`, `DisCatSharp.Voice`) can opt-in to the same sink via a source identifier.

### Core Components

| Component                 | Purpose                                                         |
| ------------------------- | --------------------------------------------------------------- |
| `ILibraryDiagnosticsSink` | Internal interface — contract for all sink implementations      |
| `SentryDiagnosticsSink`   | Sentry-backed implementation with per-client `SentryClient`     |
| `NoOpDiagnosticsSink`     | Zero-cost stub when telemetry is disabled                       |
| `DiagnosticReport`        | Structured report for missing-field and deserialization events  |
| `DiagnosticTags`          | Well-known tag key/value constants for consistency              |
| `DiagnosticTimer`         | Disposable timer that emits duration metrics on `Dispose()`     |
| `TelemetryBootstrap`      | Factory that wires up the sink and `BeforeSend` configuration   |

### Environment Detection

The Sentry environment is automatically determined from the library version:

-   **Pre-release versions** (containing `-`, e.g., `10.7.0-nightly`) → `Environment = "dev"`
-   **Stable releases** (e.g., `10.7.0`) → `Environment = "production"`

## What Sentry Captures

The diagnostics sink captures several categories of telemetry:

### 1. Missing Field Reports

When Discord adds new fields to API responses, DisCatSharp detects and reports them with full type schemas (including nested object structures), without leaking any user data.

```json
{
	"tenant_metadata": {
		"guild_monetization": {
			"powerup": {
				"boost_price": "integer",
				"enabled": "boolean"
			}
		}
	},
	"clan_tag": "string",
	"max_stage_video_users": "integer",
	"soundboard_sounds": {
		"array_element": {
			"sound_id": "string",
			"volume": "float",
			"emoji_name": "string"
		}
	}
}
```

This tells us the exact shape of new fields without exposing any user data.

For payloads larger than 8 KB, the scrubbed JSON is attached as a **file attachment** (via `SentryHint`) instead of inline extra data, preventing event truncation while preserving the full structure.

### 2. Exceptions

Library-internal exceptions (REST errors, WebSocket failures, deserialization errors) are captured with structured context, tags, and the library context panel.

### 3. Breadcrumbs

Breadcrumbs provide an operation timeline leading up to any error. They accumulate on the per-client `Scope` and are automatically attached to every Sentry event. The following breadcrumbs are captured:

| Source                       | Category          | Data attached                                         |
| ---------------------------- | ----------------- | ----------------------------------------------------- |
| Gateway WebSocket receive    | `gateway`         | `opcode`, `event_name` (if dispatch), `sequence`      |
| Gateway disconnect           | `gateway`         | `close_code`, `fatal`, `reconnect`                    |
| Gateway dispatch             | `dispatch`        | `event_name`                                          |
| REST request (before send)   | `rest`            | `method`, `route`                                     |
| REST response (after parse)  | `rest`            | `method`, `route`, `status`                           |

Breadcrumb categories are prefixed with the source package, e.g. `dcs.DisCatSharp.gateway`, `dcs.DisCatSharp.rest`.

### 4. Session Lifecycle

The sink tracks gateway session start/end events, providing visibility into connection churn:

-   **`StartSession()`** — Emitted on `READY` event, tagged with `dcs.session_event = "start"`.
-   **`EndSession()`** — Emitted on disconnect/dispose, tagged with `dcs.session_event = "end"`.

These events include the full library context panel (API version, shard ID, shard count, intents).

### 5. Rate Limit Tracking

REST rate limit hits are captured as Warning-level reports with structured tags:

-   `dcs.rest_route` — The rate-limited route.
-   `dcs.rest_status` — The HTTP status code (429).

This applies to both form-data and regular REST request paths.

### 6. Metrics

Metric-style events can be emitted for counters, gauges, and timing measurements. These are captured as `Debug`-level Sentry events with structured tags and extra data, queryable in **Sentry Discover**:

```json
{
	"level": "debug",
	"logger": "DisCatSharp.Metrics",
	"message": "dcs.metric.rest_latency",
	"tags": {
		"dcs.metric_name": "rest_latency",
		"dcs.metric_unit": "ms"
	},
	"extra": {
		"metric.name": "rest_latency",
		"metric.value": 142.5,
		"metric.unit": "ms"
	}
}
```

Use `StartTiming()` for automatic duration measurement:

```csharp
using (sink.StartTiming("DisCatSharp", "rest_call", tags))
{
    await httpClient.SendAsync(request);
}
// Duration metric emitted automatically in milliseconds
```

## Structured Tags

Every Sentry event is tagged with well-known keys defined in `DiagnosticTags` for consistent filtering and grouping:

| Tag Key               | Example Value           | Description                                   |
| --------------------- | ----------------------- | --------------------------------------------- |
| `dcs.source`          | `DisCatSharp`           | Package that produced the event               |
| `dcs.api_version`     | `10`                    | Discord API version                           |
| `dcs.shard_id`        | `0`                     | Shard ID the event originated from            |
| `dcs.shard_count`     | `4`                     | Total shard count                             |
| `dcs.error_origin`    | `library` / `upstream`  | Distinguishes library vs upstream errors      |
| `dcs.upstream_service` | `lavalink`             | Upstream service name (when origin=upstream)   |
| `dcs.rest_route`      | `GET /guilds/{id}`      | REST route that triggered the event           |
| `dcs.rest_status`     | `429`                   | HTTP status code                              |
| `dcs.entity_type`     | `DiscordGuild`          | Entity type being deserialized                |
| `dcs.session_event`   | `start` / `end`         | Gateway session lifecycle event               |
| `dcs.metric_name`     | `rest_latency`          | Metric name for metric-style events           |
| `dcs.metric_unit`     | `ms`                    | Metric unit                                   |

## Custom Fingerprinting

Events are deduplicated using intelligent fingerprinting that goes beyond Sentry defaults:

-   **Missing-field reports** include the `entity:{entityType}` tag in the fingerprint, so missing fields on `DiscordGuild` are grouped separately from `DiscordChannel`.
-   **REST errors** include the `route:{route}` tag, grouping errors by API endpoint rather than collapsing all 429s together.
-   **Exception events** include the exception type, message, target site, and inner exception for precise grouping.

## Extension Package Opt-In

Built-in extension packages can access the diagnostics sink via `BaseExtension.DiagnosticsSink` (internal, visible through `InternalsVisibleTo`).

### Origin Tagging Pattern

When an extension wraps an upstream service (e.g., Lavalink), it should tag errors with their origin to distinguish library bugs from upstream issues:

```csharp
// In an extension's error handler
sink.CaptureException("DisCatSharp.Lavalink", exception, tags: new Dictionary<string, string>
{
    [DiagnosticTags.ErrorOrigin] = DiagnosticTags.OriginUpstream,
    [DiagnosticTags.UpstreamService] = "lavalink"
});
```

This allows filtering in Sentry to separate "our bugs" from "Lavalink server errors".

### Instrumented Packages

The diagnostics sink is wired through the core library and the built-in packages that have meaningful client access:

| Package                          | Coverage                                                                                   | Origin Tagging |
| -------------------------------- | ------------------------------------------------------------------------------------------ | -------------- |
| `DisCatSharp`                    | REST errors, WebSocket errors, deserialization, rate limits, gateway lifecycle              | `library`      |
| `DisCatSharp.ApplicationCommands`| Registration failures, command execution, autocomplete, modal/context-menu handlers (10 sites) | `library`      |
| `DisCatSharp.CommandsNext`       | Command execution errors (1 site)                                                          | `library`      |
| `DisCatSharp.Hosting`            | Startup orchestration, extension initialization, client build errors (3 sites)              | `library`      |
| `DisCatSharp.Interactivity`      | Event waiters, paginators, pollers, reaction/component/modal collectors (11 sites)          | `library`      |
| `DisCatSharp.Lavalink`           | REST errors, WebSocket message failures, connection retry errors (5 sites)                  | `upstream`     |
| `DisCatSharp.Voice`              | Disconnect, sender, decoder, keepalive, UDP receive, DAVE native loading (7 sites)          | `library`      |

The following packages do **not** require instrumentation:

| Package                                  | Reason                                                              |
| ---------------------------------------- | ------------------------------------------------------------------- |
| `DisCatSharp.Common`                     | Utility library with no `DiscordClient` access                      |
| `DisCatSharp.Configuration`              | Runs at startup before client construction                          |
| `DisCatSharp.Hosting.DependencyInjection`| Thin DI extension with no error handling code                       |
| `DisCatSharp.Experimental`               | Stateless utilities (e.g., FFmpeg wrapper) with no client reference |

> [!NOTE]
> Lavalink errors are tagged with `dcs.error_origin = "upstream"` and `dcs.upstream_service = "lavalink"` because these often originate from the Lavalink server itself, not from library bugs.

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

1. **Event Metadata**: Includes SDK information, event ID, timestamps, environment, structured tags, and library context.

    ```json
    {
    	"sdk": {
    		"name": "sentry.dotnet",
    		"version": "6.2.0"
    	},
    	"event_id": "d9d303e3d75d400e992e1b1d7aef6641",
    	"timestamp": "2024-05-16T19:58:31.6006568+00:00",
    	"environment": "production",
    	"tags": {
    		"dcs.source": "DisCatSharp",
    		"dcs.api_version": "10",
    		"dcs.shard_id": "0",
    		"dcs.shard_count": "1"
    	},
    	"contexts": {
    		"library": {
    			"api_version": "10",
    			"shard_id": 0,
    			"shard_count": 1,
    			"intents": "AllUnprivileged"
    		}
    	}
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
    				"module": "DisCatSharp, Version=10.7.0.0, Culture=neutral, PublicKeyToken=null",
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

3. **Missing Field Type Schemas**: Structured type information about newly discovered fields.

    ```json
    {
    	"extras": {
    		"Found Fields": "{\"new_field\": \"string\", \"nested_obj\": {\"child\": \"integer\"}}"
    	}
    }
    ```

4. **Breadcrumbs**: Operation timeline leading up to the error, with structured data per category.

    ```json
    {
    	"breadcrumbs": [
    		{
    			"timestamp": "2024-05-16T19:58:09.000Z",
    			"message": "Received gateway message",
    			"category": "dcs.DisCatSharp.gateway",
    			"level": "info",
    			"data": {
    				"opcode": "Dispatch",
    				"event_name": "MESSAGE_CREATE",
    				"sequence": "42"
    			}
    		},
    		{
    			"timestamp": "2024-05-16T19:58:09.100Z",
    			"message": "REST request",
    			"category": "dcs.DisCatSharp.rest",
    			"level": "info",
    			"data": {
    				"method": "GET",
    				"route": "/guilds/123456789"
    			}
    		}
    	]
    }
    ```

5. **File Attachments**: For payloads larger than 8 KB, the full scrubbed JSON is attached as a file via `SentryHint`.

6. **User Information**: If `AttachUserInfo` is enabled, the bot's username, ID, and developer contact details.
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

Missing field reports contain **only type descriptors** (e.g., `"string"`, `"integer"`, `"object"`) — never actual field values. File payload attachments contain scrubbed JSON (tokens and IDs stripped) with type-only schemas.

## Client-Side Filters

To enhance safety and ensure no sensitive information is leaked, various filters have been implemented on the client side:

1. **StripTokens Utility**: This utility function removes any Discord-based tokens from strings before they might be sent to Sentry.

    ```csharp
    public static string? StripTokens(string? str)
    {
       if (string.IsNullOrWhiteSpace(str))
          return str;

       str = Regex.Replace(str, @"([a-zA-Z0-9]{68,})", "{WEBHOOK_OR_INTERACTION_TOKEN}");
       str = Regex.Replace(str, @"(mfa\\.[a-z0-9_-]{20,})|((?<botid>[a-z0-9_-]{23,28})\\.(?<creation>[a-z0-9_-]{6,7})\\.(?<enc>[a-z0-9_-]{27,}))", "{BOT_OR_USER_TOKEN}");

       return str;
    }
    ```

2. **StripIds Utility**: This utility function removes any Discord-based IDs from strings before they might be sent to Sentry.

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
    options.SetBeforeBreadcrumb((b, _)
    => new(
    	Utilities.StripTokensAndOptIds(b.Message, config.EnableDiscordIdScrubber)!,
    	b.Type!,
    	b.Data?.Select(x => new KeyValuePair<string, string>(
    		x.Key,
    		Utilities.StripTokensAndOptIds(x.Value, config.EnableDiscordIdScrubber)!))
    		.ToDictionary(x => x.Key, x => x.Value),
    	b.Category,
    	b.Level));
    ```

4. **Transaction Filter**: Ensures that sensitive information is not included in transaction data sent to Sentry.

    ```csharp
    options.SetBeforeSendTransaction((tr, _) =>
    {
       if (tr.Request.Data is string str)
          tr.Request.Data = Utilities.StripTokensAndOptIds(str, config.EnableDiscordIdScrubber);

       return tr;
    });
    ```

5. **Exception Filter**: Only whitelisted exception types are forwarded; all others are silently dropped.

6. **BeforeSend Filter**: Events without tracked exceptions or missing-field data are discarded before transmission.

By maintaining these practices, DisCatSharp ensures user privacy while leveraging Sentry to improve the library's reliability and performance.

## CI/CD Integration

### Sentry Releases

GitHub releases automatically create a corresponding Sentry release via the `sentry.yml` workflow:

-   **Version format**: `DisCatSharp@{version}+{commit_sha}` (e.g., `DisCatSharp@10.7.0+abc1234`).
-   **Environment**: Automatically set to `dev` for pre-releases, `production` for stable releases.
-   **Commit association**: Full git history is fetched (`fetch-depth: 0`) so Sentry can track commits between releases.

### PDB Uploads

Portable PDB files are uploaded to Sentry for **source-mapped stack traces**. This happens in two places:

-   **`build.yml`** — On every push to `main` and manual workflow dispatch (development builds).
-   **`release.yml`** — On NuGet package releases (production builds).

PDBs are extracted from `.snupkg` symbol packages and uploaded with `--include-sources` so that Sentry stack traces display actual source code lines.

## Sentry Dashboard Tips

With a Sentry Business plan, the following features are available for DisCatSharp telemetry:

-   **Discover**: Query metric-style events by `dcs.metric_name` and `dcs.metric_unit` tags. Build charts for REST latency, gateway dispatch frequency, and rate limit hit rates.
-   **Custom Dashboards**: Create widgets grouping events by `dcs.source`, `dcs.shard_id`, `dcs.entity_type`, or `dcs.rest_route`.
-   **Alerts**: Set up metric alerts on rate limit frequency (`dcs.rest_status:429`) or gateway disconnect patterns (`dcs.session_event:end`).
-   **Seer AI**: Automated issue triage and root cause analysis on incoming error events.

For more information on configuring and using Sentry in DisCatSharp, refer to the official [announcement](https://docs.dcs.aitsys.dev/changelogs/v10/10_6_0#sentry-integration).
