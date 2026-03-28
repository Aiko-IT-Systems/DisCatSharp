DisCatSharp Release Notes

    - Overhauled presence caching and added follow-up regression coverage around gateway cache behavior.
    - Added Discord parity updates for store, entitlement, SKU, guild powerup / applied boost, application, audit-log, automod, message-type, and OAuth scope surfaces.
    - Fixed interaction response posting, soundboard cache refresh/list behavior, duplicate application-command execution logging, and several gateway/store dispatch follow-ups.
    - Removed .NET 8 support and aligned the core package with the current target framework matrix.
    - Reworked built-in Sentry telemetry around a per-client diagnostics sink with breadcrumbs, structured tags, better grouping, scrubbed file payload attachments, release/PDB upload support, and package-root stack frame rewriting for code mappings.
    - Added package-level diagnostics participation for built-in extensions and hosting paths, including origin tagging for upstream Lavalink failures.
    - Added gateway telemetry for unknown opcodes, disconnect/session lifecycle, and suppressed dispatch exceptions, plus follow-up fixes for wrapped exception filtering, preserved route-specific grouping, and shard-specific sink metadata.
    - Adjusted Sentry environment tagging so local stable source builds report as `dev` while CI stable builds continue reporting as `production`.
    - Removed the separate `ReportMissingFields` telemetry switch so schema-drift diagnostics now follow `EnableSentry` directly.
    - Prevented the temporary sharded gateway-info client from reusing the parent telemetry sink and emitting spurious session-ended events.

DisCatSharp.Attributes Release Notes

    - No major API additions.
    - Package metadata and target framework alignment were updated alongside the wider .NET 8 removal work.

DisCatSharp.ApplicationCommands Release Notes

    - Added dedicated `SlashCommandChecksFailed` and `ContextMenuChecksFailed` events with new event args for modern checks-failed handling.
    - Updated command error flow to use dedicated checks-failed events instead of the older errored-event-only pattern.
    - Included follow-up fixes and regression coverage around application-command checks-failed behavior and execution logging.
    - Added diagnostics-sink reporting for application-command execution, registration, and autocomplete failure paths.

DisCatSharp.CommandsNext Release Notes

    - No notable feature changes.
    - Package alignment was updated as part of the .NET 8 removal and current framework support refresh.
    - Added diagnostics-sink reporting for command execution failures.

DisCatSharp.Interactivity Release Notes

    - No notable feature changes.
    - Package alignment was updated as part of the current framework support refresh.
    - Added diagnostics-sink reporting for waiter, paginator, poller, and collector exception paths.

DisCatSharp.Common Release Notes

    - No dedicated end-user feature changes.
    - Shared framework/package alignment was updated with the rest of the solution.

DisCatSharp.Lavalink Release Notes

    - Fixed a regression affecting player updates when switching the bot's voice channel while using Lavalink.
    - Included follow-up Lavalink session/internal cleanup and removed obsolete archived Lavalink v1 sources from the maintained tree.
    - Added diagnostics-sink reporting for Lavalink REST, websocket, and connection failures with upstream-origin tagging to distinguish Lavalink server issues from library faults.

DisCatSharp.Voice Release Notes

    - Fixed voice/lavalink integration regressions around channel switching.
    - Voice native packaging/build settings were refreshed with the wider framework alignment changes.
    - Added diagnostics-sink reporting across voice sender, receiver, keepalive, disconnect, and native-loading failure paths.

DisCatSharp.Experimental Release Notes

    - Stabilized autocomplete interaction value handling.
    - Added documentation/supporting work around newer experimental search-related behavior.

DisCatSharp.Configuration Release Notes

    - No standalone package-specific API additions.
    - Configuration-related framework alignment was refreshed with the rest of the solution.

DisCatSharp.Hosting Release Notes

    - Added diagnostics-sink reporting for hosted-service startup and extension-initialization failures.
    - Hosting package alignment was updated as part of the current framework support refresh.

DisCatSharp.Hosting.DependencyInjection Release Notes

    - No notable feature changes.
    - Dependency injection/hosting package alignment was updated as part of the current framework support refresh.

DisCatSharp.Analyzer Release Notes

    - Reworked the analyzer/tooling stack into a NuGet-first workflow with real xUnit/Roslyn regression coverage.
    - Added `DCS2101` as a application-command migration analyzer/code fix with rewrite, split, and manual migration modes..
    - Added `DCS2101` to an error because leaving legacy checks-failed logic on errored events can break consumers.
    - Added `DCS1101`, a presence migration analyzer/code fix for moving supported manual `DiscordClient.Presences` filtering and direct lookup shapes to `DiscordClient.GetPresences(userId)`.
    - Improved `DCS0201` so the override fixer can update `DiscordConfiguration` across project documents.
    - Updated analyzer packaging so `DisCatSharp.Attributes.dll` is bundled with the analyzer package for Roslyn runtime loading.
    - Added analyzer authoring documentation, diagnostic family guidance, release tracking files, and release workflow support for publishing `DisCatSharp.Analyzer`.
