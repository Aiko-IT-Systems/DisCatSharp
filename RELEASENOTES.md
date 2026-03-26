DisCatSharp Release Notes

    - Overhauled presence caching and added follow-up regression coverage around gateway cache behavior.
    - Added Discord parity updates for store, entitlement, SKU, guild powerup / applied boost, application, audit-log, automod, message-type, and OAuth scope surfaces.
    - Fixed interaction response posting, soundboard cache refresh/list behavior, duplicate application-command execution logging, and several gateway/store dispatch follow-ups.
    - Removed .NET 8 support and aligned the core package with the current target framework matrix.

DisCatSharp.Attributes Release Notes

    - No major API additions.
    - Package metadata and target framework alignment were updated alongside the wider .NET 8 removal work.

DisCatSharp.ApplicationCommands Release Notes

    - Added dedicated `SlashCommandChecksFailed` and `ContextMenuChecksFailed` events with new event args for modern checks-failed handling.
    - Updated command error flow to use dedicated checks-failed events instead of the older errored-event-only pattern.
    - Included follow-up fixes and regression coverage around application-command checks-failed behavior and execution logging.

DisCatSharp.CommandsNext Release Notes

    - No notable feature changes.
    - Package alignment was updated as part of the .NET 8 removal and current framework support refresh.

DisCatSharp.Interactivity Release Notes

    - No notable feature changes.
    - Package alignment was updated as part of the current framework support refresh.

DisCatSharp.Common Release Notes

    - No dedicated end-user feature changes.
    - Shared framework/package alignment was updated with the rest of the solution.

DisCatSharp.Lavalink Release Notes

    - Fixed a regression affecting player updates when switching the bot's voice channel while using Lavalink.
    - Included follow-up Lavalink session/internal cleanup and removed obsolete archived Lavalink v1 sources from the maintained tree.

DisCatSharp.Voice Release Notes

    - Fixed voice/lavalink integration regressions around channel switching.
    - Voice native packaging/build settings were refreshed with the wider framework alignment changes.

DisCatSharp.Experimental Release Notes

    - Stabilized autocomplete interaction value handling.
    - Added documentation/supporting work around newer experimental search-related behavior.

DisCatSharp.Configuration Release Notes

    - No standalone package-specific API additions.
    - Configuration-related framework alignment was refreshed with the rest of the solution.

DisCatSharp.Hosting Release Notes

    - No notable feature changes.
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
