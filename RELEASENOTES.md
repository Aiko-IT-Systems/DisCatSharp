DisCatSharp Release Notes

    - Fixed `RingBuffer<T>.CopyTo` to respect the `index` parameter and validate that enough free slots exist before copying, preventing silent data corruption.
    - Hardened gateway dispatch: all `GuildsInternal` indexer accesses now use `TryGetValue` guards to prevent `KeyNotFoundException` on late-arriving or missing guild payloads.
    - Synchronized session state during reconnect under `_sessionStateLock` to close a TOCTOU race where `OnHelloAsync` could read a partially-cleared session ID and produce a malformed RESUME payload.
    - Fixed heartbeat skipped-beat counter reset from a no-op `Interlocked.CompareExchange` to `Volatile.Write`, and cancels the old heartbeat loop before starting a new one to prevent task leaks across reconnects.
    - Null-guarded `GetSocketLock` to return `null` instead of throwing before the WebSocket is fully initialized.
    - Made `ReadyGuildIds` thread-safe via a dedicated lock and an atomic `SetReadyGuildIds()` mutator; dispatch now calls the mutator instead of writing directly to the backing set.
    - Narrowed broad `catch (Exception)` blocks to specific exception types (`JsonReaderException`, `ArgumentOutOfRangeException`) in all seven Discord exception wrapper types and `Utilities`.
    - Replaced `Stream.Read` with `Stream.ReadExactly` in `MediaTool` to prevent silent partial reads.
    - Eliminated a redundant `DateTimeOffset` → `DateTime` → `DateTimeOffset` round-trip in `Formatter`, preserving UTC offset fidelity.
    - Added a defensive copy on `AttachmentsInternal` in `DiscordMessage.Attachments` to prevent external callers from mutating cached message state.
    - Capped REST `retry_after` to 3 600 s to guard against adversarially large values from misbehaving proxies or rogue buckets.
    - Linearized concurrent bucket enumeration in `RestClient` with a `.ToList()` snapshot before iteration to prevent `InvalidOperationException` under concurrent request pressure.
    - Fixed `SocketLock.ContinueWith` calls to use `CancellationToken.None` and `TaskScheduler.Default`, preventing ambient sync-context capture and potential deadlocks in UI/ASP.NET contexts.
    - Added `DiscordConfiguration.Validate()`, called from `ConnectAsync` before any network I/O, which throws `InvalidOperationException` when `ShardId ≥ ShardCount`.
    - Clamped `LargeThreshold` to the Discord-specified bot range (50–250) in `DiscordConfiguration`; values outside the range are silently clamped on assignment.
    - Added non-negative validation for `ShardId` and positive validation for `ShardCount` in `DiscordConfiguration` property setters.
    - Replaced fire-and-forget `Task.Run` gateway dispatch with an ordered `Channel<T>` queue and single-consumer loop per shard, guaranteeing FIFO cache and state mutations.
    - Added `GatewayDispatchMode` enum (`ConcurrentHandlers` / `SequentialHandlers`) configurable via `GatewayAdvancedConfiguration.DispatchMode`; defaults to `ConcurrentHandlers` (fire-and-forget user handlers after sequential cache mutations).
    - Routed `PRESENCE_UPDATE` to a dedicated unbounded channel and consumer task, decoupled from the main dispatch queue. Prevents high-volume presence spam from starving other gateway events.
    - Added `AsyncEvent<TSender, TArgs>.HasHandlers` property; `OnPresenceUpdateEventAsync` now skips old-presence cloning, event args allocation, and `RaiseEventAsync` when no `PresenceUpdated` handlers are registered.
    - Added presence coalescing: the presence consumer drains all immediately available payloads, deduplicates by user ID (latest wins), and processes only the coalesced batch — eliminating redundant cache writes during burst updates.
    - Added 11 internal extension events on `DiscordClient` for reliable, ordered delivery to library extensions before public events fire. Extensions (AppCommands, CommandsNext, Interactivity, Lavalink, Voice) now subscribe to these instead of public events.
    - Moved `Proxy` from `RestConfiguration` to root `DiscordConfiguration` since it applies to both REST and Gateway connections.
    - Moved `EnableLibraryDeveloperMode` from `TelemetryConfiguration` to root `DiscordConfiguration` since it gates behavior beyond telemetry.
    - Audited `NullValueHandling` across 51 serialized properties in 19 entity files: corrected 25 from `Include` to `Ignore` on fields Discord never reads as explicit `null`; preserved `Include` on 26 fields where `null` carries a distinct semantic (ungrouping channels, disconnecting from voice, clearing timeout/gradient/incident restrictions, null-as-boolean role tags, and unicode-vs-custom emoji disambiguation).
    - Fixed `DiscordMember.BanAsync` and `UnbanAsync` to route through `ApiClient` directly, consistent with all other member REST operations.
    - Renamed `deleteMessageDays` to `deleteMessageSeconds` on `DiscordMember.BanAsync` and all `DiscordGuild.BanMemberAsync` overloads; removed the legacy days-to-seconds compatibility shim — the API has always accepted seconds. **Note:** positional callers passing day values must be updated manually; named-argument callers are handled by `DCS1102`.
    - Padded log-level labels to eight characters in `DefaultLogger` for consistent column alignment across log lines.
    - Fixed `DiscordEventArgs` scoped service scope to be properly disposed after event dispatch, preventing scoped service leaks on high-volume bots.

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
    - Made `DiscordApplicationCommandLocalization.ValidLocales` a static `FrozenSet<string>` for O(1) lookup and zero per-instance allocation.

DisCatSharp.Attributes Release Notes

    - No major API additions.
    - Package metadata and target framework alignment were updated alongside the wider .NET 8 removal work.

DisCatSharp.ApplicationCommands Release Notes

    - Added dedicated `SlashCommandChecksFailed` and `ContextMenuChecksFailed` events with new event args for modern checks-failed handling.
    - Updated command error flow to use dedicated checks-failed events instead of the older errored-event-only pattern.
    - Included follow-up fixes and regression coverage around application-command checks-failed behavior and execution logging.
    - Added diagnostics-sink reporting for application-command execution, registration, and autocomplete failure paths.
    - Fixed command equality change detection: defensive copies for both source and target, type-based matching for entry points and context menus, and explicit `Optional` clearing for unchanged option fields.
    - Fixed entry point (launch) command registration so it flows through normal create/overwrite/unchanged detection instead of being unconditionally re-created on every startup.
    - Strip built-in localizations from the entry point command when `EnableLocalization` is disabled.
    - Added `RegisterEntryPointCommand()` on `ApplicationCommandsExtension` so consumers can configure entry point properties (description, handler type, contexts, integration types) without hardcoding them.
    - Replaced three wasteful `new ServiceCollection().BuildServiceProvider()` allocations with a shared `EmptyServiceProvider.Instance` singleton.
    - Hardened `ChoiceProvider.Services` with a backing field and `InvalidOperationException` guard when accessed before initialization.
    - Fixed bare `catch` blocks to use filtered `catch (Exception ex)` for proper debugger break behavior.
    - Fixed unsafe `int`/`ulong` casts to use `Convert.ToInt32`/`Convert.ToUInt64` for safe numeric conversion.
    - Normalized `ToLower()` calls to `ToLowerInvariant()` for culture-independent command name handling (including `SlashCommandGroupAttribute`).
    - Improved choice value comparison with numeric normalization (`AreChoiceValuesEqual`) to avoid false change detection between `int`/`long`/`double` representations from JSON.
    - Rewrote `DeepEqualOptions` to collect all option mismatches with per-field diagnostic messages instead of returning on the first difference.
    - Added thread-safe snapshots for public collection properties (`GlobalCommands`, `GuildCommands`).
    - Isolated `resultCommands` from the input `updateList` to prevent mutation of caller-provided data.
    - Migrated to internal extension events for reliable, ordered event delivery from the dispatch pipeline.

DisCatSharp.CommandsNext Release Notes

    - No notable feature changes.
    - Package alignment was updated as part of the .NET 8 removal and current framework support refresh.
    - Added diagnostics-sink reporting for command execution failures.
    - Migrated to internal extension events for reliable, ordered event delivery from the dispatch pipeline.

DisCatSharp.Interactivity Release Notes

    - Fixed `ComponentCollectRequest.Collected` never being initialized, which caused `NullReferenceException` on any component collect operation.
    - Replaced non-thread-safe `Dictionary` in `ComponentPaginator` with `ConcurrentDictionary` to prevent race condition corruption during concurrent pagination.
    - Added try-catch blocks around all fire-and-forget `Task.Run` calls in `Paginator` and `Poller` to prevent silent exception swallowing.
    - Fixed `CancellationTokenSource` leak in `GetCancellationToken` by registering a self-disposal callback on the token.
    - Fixed `InteractionPaginationRequest.GetPageAsync` so navigation buttons are properly disabled for single-page pagination and boundary conditions.
    - Removed broken followup-without-ACK path in `ComponentEventWaiter.Handle` that caused Discord API 400 errors on non-matching interactions.
    - Eliminated shared mutable `_builder` field in `ComponentPaginator`; each pagination response now uses a local builder to prevent concurrent corruption.
    - Aligned `PaginationButtons` default custom IDs with their declared constants so custom ID matching works correctly.
    - Removed spam followup messages in `ModalEventWaiter.Handle` for every non-matching interaction event.
    - Fixed dispose-race in `Paginator` and `Poller` where `_client` was set to null before pending `Task.Run` could complete, causing `NullReferenceException`.
    - Replaced reflection-based event lookup in `EventWaiter<T>` with explicit subscribe/unsubscribe delegates for the three known event types; reflection kept only as fallback for the dynamic `WaitForEventArgsAsync<T>` / `CollectEventArgsAsync<T>` public API.
    - Replaced reflection-based event lookup in `ReactionCollector` with direct `+=` / `-=` event subscription matching the pattern `Paginator` and `Poller` already use.
    - Added `lock()` guards around non-atomic read-modify-write operations on `ConcurrentHashSet` in `ReactionCollector` and `PollRequest` to prevent race conditions losing reactions or votes.
    - Fixed multiple enumeration of `IEnumerable<Page>` in `SendPaginatedMessageAsync` / `SendPaginatedResponseAsync` by coercing to `IList<Page>` upfront.
    - Added empty-pages guard (`ArgumentException`) in pagination methods instead of letting `pages.First()` throw an unhelpful exception.
    - Removed dead code `HandleInvalidInteraction` method and unused `_componentInteractionWaiter` / `_modalInteractionWaiter` fields along with their suppressions.
    - `IPaginator` now extends `IDisposable` so paginators held via the interface can be properly disposed.
    - `InteractivityExtension` now implements `IDisposable` with a `_disposed` guard that disposes all nine internal components (waiters, paginators, poller, collector).
    - Added double-dispose guards with `_disposed` flags to `ComponentPaginator`, `PaginationRequest`, `PollRequest`, `ReactionCollectRequest`, `Paginator`, and `Poller`.
    - Replaced unnecessary `await Task.Yield()` calls in `PaginationRequest` with `Task.FromResult` / `Task.CompletedTask`.
    - Renamed confusing `_behaviorBehavior` field to `_buttonBehavior`.
    - `InteractivityHelpers.Recalculate` now throws if all pages lack embeds and preserves content-only pages in the output.
    - Stored `CancellationTokenRegistration` in `ComponentMatchRequest` and `ModalMatchRequest` for proper lifecycle tracking.
    - Added diagnostics-sink reporting for waiter, paginator, poller, and collector exception paths.
    - Migrated to internal extension events for reliable, ordered event delivery from the dispatch pipeline.

DisCatSharp.Common Release Notes

    - No dedicated end-user feature changes.
    - Shared framework/package alignment was updated with the rest of the solution.

DisCatSharp.Lavalink Release Notes

    - Fixed a regression affecting player updates when switching the bot's voice channel while using Lavalink.
    - Included follow-up Lavalink session/internal cleanup and removed obsolete archived Lavalink v1 sources from the maintained tree.
    - Added diagnostics-sink reporting for Lavalink REST, websocket, and connection failures with upstream-origin tagging to distinguish Lavalink server issues from library faults.
    - Migrated to internal extension events for reliable, ordered event delivery from the dispatch pipeline.

DisCatSharp.Voice Release Notes

    - Fixed voice/lavalink integration regressions around channel switching.
    - Voice native packaging/build settings were refreshed with the wider framework alignment changes.
    - Added diagnostics-sink reporting across voice sender, receiver, keepalive, disconnect, and native-loading failure paths.
    - Migrated to internal extension events for reliable, ordered event delivery from the dispatch pipeline.

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
    - Improved `DCS0201` so the override fixer can update `DiscordConfiguration` across project documents.
    - Updated analyzer packaging so `DisCatSharp.Attributes.dll` is bundled with the analyzer package for Roslyn runtime loading.
    - Added analyzer authoring documentation, diagnostic family guidance, release tracking files, and release workflow support for publishing `DisCatSharp.Analyzer`.
    - Added `DCS1102`, a ban-parameter migration analyzer/code fix: detects `deleteMessageDays:` named arguments on `BanAsync`/`BanMemberAsync` and renames them to `deleteMessageSeconds:`, multiplying integer literals by 86400 automatically.
    - Updated `DCS1201` config property migration to support root-level property targets (empty `NestedPath`) for `Proxy` and `EnableLibraryDeveloperMode`.
