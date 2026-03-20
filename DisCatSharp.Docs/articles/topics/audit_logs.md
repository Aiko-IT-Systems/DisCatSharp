---
uid: topics_audit_logs
title: Audit Logs
author: DisCatSharp Team
---

# Audit Logs

Audit logs are Discord's moderation and administration paper trail. They tell you who changed something, what changed, and, depending on the action type, which guild object was involved.

DisCatSharp's audit log API is built around Discord's actual payload shape instead of trying to pretend every action behaves the same way. That means:

- common action families are exposed as typed entry classes
- Discord's `changes` and `options` payloads are preserved
- undocumented or internal Discord action types still parse safely
- the REST API and the gateway event share the same parser behavior

This article walks through the practical side of working with that model.

## Requirements

To fetch audit logs with the REST API, your bot needs the `VIEW_AUDIT_LOG` permission in the target guild.

To receive live audit log gateway events through `GuildAuditLogEntryCreated`, your bot also needs:

- the `VIEW_AUDIT_LOG` permission
- the `GuildModeration` intent enabled in your `DiscordConfiguration`

```csharp
var discord = new DiscordClient(new DiscordConfiguration
{
    Token = "token",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.Guilds | DiscordIntents.GuildModeration
});
```

## Fetching Audit Log Pages

The main entry point is `DiscordGuild.GetAuditLogEntriesAsync`.

```csharp
DiscordAuditLogPage page = await guild.GetAuditLogEntriesAsync(new DiscordAuditLogQuery
{
    Limit = 25,
    ActionType = AuditLogActionType.MemberRoleUpdate
});
```

The returned [DiscordAuditLogPage](xref:DisCatSharp.Entities.DiscordAuditLogPage) contains:

- `Entries`
- `IsAscending`
- `FirstEntryId`
- `LastEntryId`

That explicit page metadata matters because Discord changes the order of results depending on which cursor you use.

### Ordering and Cursors

Discord's audit log endpoint behaves a little mischievously here:

- `before` returns entries in descending order
- `after` returns entries in ascending order
- omitting both behaves like Discord's usual descending order

DisCatSharp keeps that behavior visible through `DiscordAuditLogPage.IsAscending`.

```csharp
DiscordAuditLogPage page = await guild.GetAuditLogEntriesAsync(new DiscordAuditLogQuery
{
    After = 0,
    Limit = 100
});

if (page.IsAscending)
{
    // Older entries first.
}
```

> [!IMPORTANT]
> `Before` and `After` are mutually exclusive in DisCatSharp's audit log query API. Supplying both throws an `ArgumentException`.

### Filtering

[DiscordAuditLogQuery](xref:DisCatSharp.Entities.DiscordAuditLogQuery) currently supports:

- `Limit`
- `UserId`
- `ActionType`
- `Before`
- `After`

Example:

```csharp
DiscordAuditLogPage page = await guild.GetAuditLogEntriesAsync(new DiscordAuditLogQuery
{
    UserId = moderator.Id,
    ActionType = AuditLogActionType.MessageDelete,
    Before = someEntryId,
    Limit = 50
});
```

## Understanding Entry Families

Every returned entry derives from [DiscordAuditLogEntry](xref:DisCatSharp.Entities.DiscordAuditLogEntry). The base type exposes the fields that exist across Discord's audit log payloads:

- `Id`
- `Guild`
- `ActionType`
- `ActionCategory`
- `TargetId`
- `Actor`
- `Reason`
- `Changes`
- `RawChanges`
- `Options`
- `RawOptions`

For stable Discord action families, DisCatSharp returns typed subclasses such as:

- [DiscordChannelAuditLogEntry](xref:DisCatSharp.Entities.DiscordChannelAuditLogEntry)
- [DiscordMemberAuditLogEntry](xref:DisCatSharp.Entities.DiscordMemberAuditLogEntry)
- [DiscordRoleAuditLogEntry](xref:DisCatSharp.Entities.DiscordRoleAuditLogEntry)
- [DiscordMessageAuditLogEntry](xref:DisCatSharp.Entities.DiscordMessageAuditLogEntry)
- [DiscordThreadAuditLogEntry](xref:DisCatSharp.Entities.DiscordThreadAuditLogEntry)
- [DiscordAutoModerationRuleAuditLogEntry](xref:DisCatSharp.Entities.DiscordAutoModerationRuleAuditLogEntry)
- [DiscordGuildScheduledEventExceptionAuditLogEntry](xref:DisCatSharp.Entities.DiscordGuildScheduledEventExceptionAuditLogEntry)
- [DiscordPermissionMigrationAuditLogEntry](xref:DisCatSharp.Entities.DiscordPermissionMigrationAuditLogEntry)

The recommended pattern is to branch on the entry family first, then inspect changes or options as needed.

```csharp
foreach (DiscordAuditLogEntry entry in page.Entries)
{
    switch (entry)
    {
        case DiscordMemberAuditLogEntry memberEntry:
            Console.WriteLine($"Member action for {memberEntry.TargetMember?.DisplayName ?? entry.TargetId}");
            break;

        case DiscordThreadAuditLogEntry threadEntry:
            Console.WriteLine($"Thread action for {threadEntry.TargetThread?.Name ?? entry.TargetId}");
            break;

        case DiscordRawAuditLogEntry:
            Console.WriteLine($"Raw audit log action: {(int)entry.ActionType}");
            break;
    }
}
```

## Working with Changes

Discord's `changes` array is intentionally preserved instead of being flattened into hundreds of fragile per-property members.

Each element is exposed as a [DiscordAuditLogChange](xref:DisCatSharp.Entities.DiscordAuditLogChange) with:

- `Key`
- `OldValue`
- `NewValue`
- `GetOldValue<T>()`
- `GetNewValue<T>()`

You can query changes either by iterating manually or by using the helper methods on the entry itself.

```csharp
if (entry.TryGetChange("name", out DiscordAuditLogChange nameChange))
{
    string? oldName = nameChange.GetOldValue<string>();
    string? newName = nameChange.GetNewValue<string>();
}

DiscordAuditLogChange? archivedChange = entry.GetChange("archived");
bool? isArchived = archivedChange?.GetNewValue<bool>();
```

### Typed Change Helpers

DisCatSharp provides helper wrappers over the raw change array for the most common Discord entry families.

These helpers do not replace `Changes`. They sit on top of it and give you a more comfortable typed view for the keys that tend to show up often.

Common examples include:

- [DiscordGuildAuditLogChanges](xref:DisCatSharp.Entities.DiscordGuildAuditLogChanges)
- [DiscordChannelAuditLogChanges](xref:DisCatSharp.Entities.DiscordChannelAuditLogChanges)
- [DiscordThreadAuditLogChanges](xref:DisCatSharp.Entities.DiscordThreadAuditLogChanges)
- [DiscordMemberAuditLogChanges](xref:DisCatSharp.Entities.DiscordMemberAuditLogChanges)
- [DiscordRoleAuditLogChanges](xref:DisCatSharp.Entities.DiscordRoleAuditLogChanges)
- [DiscordMessageAuditLogChanges](xref:DisCatSharp.Entities.DiscordMessageAuditLogChanges)
- [DiscordAutoModerationRuleAuditLogChanges](xref:DisCatSharp.Entities.DiscordAutoModerationRuleAuditLogChanges)

```csharp
if (entry is DiscordThreadAuditLogEntry threadEntry)
{
    bool? archived = threadEntry.ChangeSet.Archived?.After;
    bool? locked = threadEntry.ChangeSet.Locked?.After;
    int? autoArchiveDuration = threadEntry.ChangeSet.AutoArchiveDuration?.After;
    IReadOnlyList<ulong>? appliedTags = threadEntry.ChangeSet.AppliedTags?.After;
}
```

Each helper property typically returns either:

- `DiscordAuditLogValueChange<T>`
- `DiscordAuditLogCollectionChange<T>`

That makes common field deltas easier to inspect without sacrificing Discord's original payload shape.

### Typed Raw Conversions

[DiscordAuditLogChange](xref:DisCatSharp.Entities.DiscordAuditLogChange) also exposes convenience conversion helpers for common Discord data shapes.

Examples include:

- `GetOldSnowflake()` and `GetNewSnowflake()`
- `GetOldBoolean()` and `GetNewBoolean()`
- `GetOldDateTimeOffset()` and `GetNewDateTimeOffset()`
- `GetOldEnum<TEnum>()` and `GetNewEnum<TEnum>()`
- `GetOldPermissions()` and `GetNewPermissions()`

```csharp
if (entry.TryGetChange("verification_level", out DiscordAuditLogChange verificationChange))
{
    VerificationLevel? before = verificationChange.GetOldEnum<VerificationLevel>();
    VerificationLevel? after = verificationChange.GetNewEnum<VerificationLevel>();
}
```

### Known Discord Change Quirks

Discord's audit log changes are not uniform. A few common quirks are worth keeping in mind:

- member role updates use `$add` and `$remove`
- application command permission updates use snowflakes as `key`
- some actions reset values by omitting `new_value`
- some actions clear values by omitting `old_value`

Because of that, prefer defensive parsing for less common keys, especially if you are handling undocumented or newly-added Discord actions.

## Working with Options

Not every useful audit log entry has a populated `changes` array.

Several Discord actions place their important data in the `options` object instead, including common cases such as:

- message delete counts
- pin and unpin message ids
- overwrite target ids and types
- auto moderation rule execution metadata
- scheduled event exception ids
- internal or system action payload details

DisCatSharp exposes the documented fields through [DiscordAuditLogEntryOptions](xref:DisCatSharp.Entities.DiscordAuditLogEntryOptions) and also preserves the raw JSON object.

```csharp
if (entry is DiscordMessageAuditLogEntry messageEntry)
{
    Console.WriteLine($"Affected channel: {messageEntry.Channel?.Name}");
    Console.WriteLine($"Affected message id: {messageEntry.MessageId}");
    Console.WriteLine($"Affected count: {messageEntry.Count}");
}

if (entry is DiscordOverwriteAuditLogEntry overwriteEntry)
{
    Console.WriteLine($"Overwrite target id: {overwriteEntry.OverwrittenEntityId}");
    Console.WriteLine($"Overwrite target type: {overwriteEntry.OverwriteTargetType}");
}
```

> [!TIP]
> `HasChanges == false` does not mean an entry is empty. Always check `Options` for message, overwrite, auto moderation, and system-style actions.

## Working with Convenience Members

DisCatSharp also exposes a few lightweight helpers for the Discord action families that tend to be mildly annoying in raw form.

### Overwrite Entries

[DiscordOverwriteAuditLogEntry](xref:DisCatSharp.Entities.DiscordOverwriteAuditLogEntry) now exposes a typed [AuditLogOverwriteTargetType](xref:DisCatSharp.Enums.AuditLogOverwriteTargetType) instead of forcing you to compare Discord's raw `"0"` and `"1"` strings yourself.

```csharp
if (entry is DiscordOverwriteAuditLogEntry overwriteEntry)
{
    if (overwriteEntry.TargetsRole)
        Console.WriteLine($"Role overwrite: {overwriteEntry.RoleName}");
}
```

### Member Role Updates

[DiscordMemberAuditLogEntry](xref:DisCatSharp.Entities.DiscordMemberAuditLogEntry) exposes parsed partial role deltas for `$add` and `$remove`.

```csharp
if (entry is DiscordMemberAuditLogEntry memberEntry)
{
    foreach (DiscordAuditLogPartialRole role in memberEntry.AddedRoles ?? [])
        Console.WriteLine($"Added role: {role.Name} ({role.Id})");
}
```

### Message Actions

[DiscordMessageAuditLogEntry](xref:DisCatSharp.Entities.DiscordMessageAuditLogEntry) keeps Discord's message metadata visible through convenience members such as:

- `AffectedMessageCount`
- `TargetMessageId`
- `IsBulkDeleteAction`
- `IsPinAction`

Those helpers are especially handy because message-style entries often carry most of their useful information in `Options` rather than `Changes`.

### Auto Moderation Actions

[DiscordAutoModerationRuleAuditLogEntry](xref:DisCatSharp.Entities.DiscordAutoModerationRuleAuditLogEntry) now exposes `IsExecutionAction` and `IsRuleMutationAction` so execution entries and rule edits are easier to branch on.

```csharp
if (entry is DiscordAutoModerationRuleAuditLogEntry autoModEntry)
{
    if (autoModEntry.IsExecutionAction)
        Console.WriteLine("This entry represents a rule execution.");
}
```

## Undocumented and Internal Discord Actions

Discord ships audit log actions that are not always fully documented in the public resource docs. DisCatSharp deliberately keeps those entries usable.

If an action has a stable enough shape, DisCatSharp may expose a dedicated typed family for it. Examples include:

- voice channel status actions
- guild profile updates
- member verification updates
- permission migration entries
- creator monetization system entries

If DisCatSharp does not have a dedicated typed family yet, the entry is returned as [DiscordRawAuditLogEntry](xref:DisCatSharp.Entities.DiscordRawAuditLogEntry) instead of being discarded.

That means you can still inspect:

- `ActionType`
- `TargetId`
- `Actor`
- `Reason`
- `Changes`
- `Options`
- `RawChanges`
- `RawOptions`

```csharp
if (entry is DiscordRawAuditLogEntry rawEntry)
{
    Console.WriteLine($"Unknown audit log action: {(int)rawEntry.ActionType}");

    foreach (DiscordAuditLogChange change in rawEntry.Changes)
        Console.WriteLine($"{change.Key}: {change.OldValue} -> {change.NewValue}");
}
```

## Partial References and Cache Behavior

DisCatSharp does not make surprise REST calls while parsing audit log entries.

Instead, it resolves references from:

- the objects included in Discord's audit log payload
- the current guild cache
- the current client cache

If Discord does not include enough information and the cache does not have the target either, the entry remains usable but the typed target may be partial or `null`.

That behavior is intentional:

- parsing stays deterministic
- gateway event handling stays lightweight
- undocumented payloads do not force hidden network requests

## Upgrading Partial References to Live Data

If you want the parsed audit log entry to keep its comfy typed shape but upgrade placeholder or synthetic references into
real live entities, use the hydration helpers on [DiscordAuditLogEntry](xref:DisCatSharp.Entities.DiscordAuditLogEntry).

These helpers are opt-in on purpose:

- parsing still stays side-effect free by default
- you decide whether cache-only resolution is enough
- you decide whether live REST fetches are worth the extra round-trips

The two main entry points are:

- `HydrateAllAsync(bool force = true)`
- `HydrateAsync(AuditLogHydrationTargets targets, bool force = true)`

`force` controls whether DisCatSharp is allowed to hit REST:

- `force: false` only upgrades references from the current client or guild cache
- `force: true` fetches live entities where DisCatSharp exposes a retrieval API

```csharp
DiscordAuditLogPage page = await guild.GetAuditLogEntriesAsync(new DiscordAuditLogQuery
{
    Limit = 25
});

foreach (DiscordAuditLogEntry entry in page.Entries)
{
    await entry.HydrateAllAsync(force: false);
}
```

If you only care about specific parts of the entry, use [AuditLogHydrationTargets](xref:DisCatSharp.Enums.AuditLogHydrationTargets).

```csharp
if (entry is DiscordOverwriteAuditLogEntry overwriteEntry)
{
    await overwriteEntry.HydrateAsync(AuditLogHydrationTargets.Target, force: true);

    if (overwriteEntry.TargetsRole)
        Console.WriteLine(overwriteEntry.OverwrittenRole?.Name ?? overwriteEntry.RoleName);
}
```

This is especially useful for entry families where Discord often gives you only ids or partial side-loaded objects, such as:

- overwrite entries
- member and message entries
- thread entries
- scheduled event entries
- emoji, sticker, webhook, and soundboard entries

> [!NOTE]
> Hydration is best-effort for deleted or no-longer-accessible entities. If Discord returns `404`, DisCatSharp keeps the
> existing partial reference instead of clearing it.

## Receiving Live Audit Log Entries

DisCatSharp also exposes the gateway event through `GuildAuditLogEntryCreated`.

```csharp
discord.GuildAuditLogEntryCreated += async (client, eventArgs) =>
{
    DiscordAuditLogEntry entry = eventArgs.AuditLogEntry;

    if (entry is DiscordAutoModerationRuleAuditLogEntry autoModEntry)
    {
        Console.WriteLine($"AutoMod action: {autoModEntry.ActionType}");
        Console.WriteLine($"Rule: {autoModEntry.RuleName}");
        Console.WriteLine($"Target member: {autoModEntry.TargetMember?.DisplayName}");
    }

    await Task.CompletedTask;
};
```

The gateway event uses the same parser contract as the REST API, so you can reuse the same pattern matching and `Changes` or `Options` handling logic in both places.

## Real-World Example: Slash Command Browser

If you want something a bit more practical than `Console.WriteLine`, a nice moderation utility is an ephemeral slash command that fetches recent audit log entries and lets the caller browse them with paginated buttons.

This example uses:

- `DisCatSharp.ApplicationCommands`
- `DisCatSharp.Interactivity`
- components v2 / UI kit messages

It fetches the most recent entries, renders them into a components-v2 browser, and lets the command invoker flip through pages without spamming the channel.

Before using this example, make sure interactivity is enabled on your client:

```csharp
discord.UseInteractivity();
```

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Enums.Core;

namespace MyBot;

public sealed class ModerationAuditLogModule : ApplicationCommandsModule
{
	[SlashCommand("auditlog_browser", "Browse the most recent audit log entries for this guild.", allowedContexts: [InteractionContextType.Guild], integrationTypes: [ApplicationCommandIntegrationTypes.GuildInstall])]
	public async Task AuditLogBrowserAsync(InteractionContext ctx)
	{
		if (ctx.Guild is null)
		{
			await ctx.CreateResponseAsync(
				InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder()
					.WithContent("This command can only be used inside a guild.")
					.AsEphemeral());
			return;
		}

		var auditPage = await ctx.Guild.GetAuditLogEntriesAsync(new DiscordAuditLogQuery
		{
			Limit = 50
		});

		foreach (DiscordAuditLogEntry entry in auditPage.Entries)
			await entry.HydrateAllAsync(force: false);

		if (auditPage.Entries.Count is 0)
		{
			await ctx.CreateResponseAsync(
				InteractionResponseType.ChannelMessageWithSource,
				new DiscordInteractionResponseBuilder()
					.WithContent("No audit log entries matched the current filter.")
					.AsEphemeral());
			return;
		}

		var currentPage = 0;
		var pageCount = (int)Math.Ceiling(auditPage.Entries.Count / 5d);

		await ctx.CreateResponseAsync(
			InteractionResponseType.ChannelMessageWithSource,
			new DiscordInteractionResponseBuilder()
				.AsEphemeral()
				.WithV2Components()
				.AddComponents(BuildBrowser(auditPage, currentPage, pageCount)));

		var message = await ctx.GetOriginalResponseAsync();

		while (true)
		{
			var buttonResult =
				await message.WaitForButtonAsync(ctx.User, TimeSpan.FromMinutes(2));

			if (buttonResult.TimedOut)
			{
				await ctx.EditResponseAsync(
					new DiscordWebhookBuilder()
						.WithV2Components()
						.AddComponents(BuildBrowser(auditPage, currentPage, pageCount, disableButtons: true)),
					ModifyMode.Replace);
				return;
			}

			switch (buttonResult.Result.Id)
			{
				case "auditlog_prev":
					currentPage = Math.Max(0, currentPage - 1);
					break;

				case "auditlog_next":
					currentPage = Math.Min(pageCount - 1, currentPage + 1);
					break;

				case "auditlog_close":
					await buttonResult.Result.Interaction.CreateResponseAsync(
						InteractionResponseType.UpdateMessage,
						new DiscordInteractionResponseBuilder()
							.WithV2Components()
							.AddComponents(BuildBrowser(auditPage, currentPage, pageCount, disableButtons: true)));
					return;
			}

			await buttonResult.Result.Interaction.CreateResponseAsync(
				InteractionResponseType.UpdateMessage,
				new DiscordInteractionResponseBuilder()
					.WithV2Components()
					.AddComponents(BuildBrowser(auditPage, currentPage, pageCount)));
		}
	}

	private static IEnumerable<DiscordComponent> BuildBrowser(
		DiscordAuditLogPage auditPage,
		int currentPage,
		int pageCount,
		bool disableButtons = false)
	{
		var pageEntries = auditPage.Entries
			.Skip(currentPage * 5)
			.Take(5)
			.ToArray();

		DiscordContainerComponent container = new(accentColor: DiscordColor.Blurple);
		container.AddComponent(new DiscordTextDisplayComponent(
			$"## Audit Log Browser\nShowing page **{currentPage + 1}/{pageCount}** with **{auditPage.Entries.Count}** loaded entries."));

		for (var index = 0; index < pageEntries.Length; index++)
		{
			var entry = pageEntries[index];
			container.AddComponents([
				new DiscordTextDisplayComponent($"### {entry.ActionType}"),
				new DiscordTextDisplayComponent($"Actor: {FormatActor(entry)}\nTarget: {FormatTarget(entry)}"),
				new DiscordTextDisplayComponent($"Details: {FormatDetails(entry)}\nReason: {entry.Reason ?? "No reason provided."}")
			]);

			if (index < pageEntries.Length - 1)
				container.AddComponent(new DiscordSeparatorComponent());
		}

		container.AddComponent(new DiscordActionRowComponent([
			new DiscordButtonComponent(ButtonStyle.Secondary, "auditlog_prev", "Previous", disableButtons || currentPage is 0),
			new DiscordButtonComponent(ButtonStyle.Primary, "auditlog_next", "Next", disableButtons || currentPage >= pageCount - 1),
			new DiscordButtonComponent(ButtonStyle.Danger, "auditlog_close", "Close", disableButtons)
		]));

		return [container];
	}

	private static string FormatActor(DiscordAuditLogEntry entry)
		=> entry.Actor?.Mention ?? (entry.Actor is not null ? $"`{entry.Actor.Id}`" : "Unknown actor");

	private static string FormatTarget(DiscordAuditLogEntry entry)
		=> entry switch
		{
			DiscordChannelAuditLogEntry channelEntry => channelEntry.TargetChannel?.Mention ?? FormatTarget(entry.TargetId),
			DiscordMemberAuditLogEntry memberEntry => memberEntry.TargetMember?.Mention ?? FormatTarget(entry.TargetId),
			DiscordRoleAuditLogEntry roleEntry => roleEntry.TargetRole?.Mention ?? FormatTarget(entry.TargetId),
			DiscordThreadAuditLogEntry threadEntry => threadEntry.TargetThread?.Name ?? FormatTarget(entry.TargetId),
			DiscordWebhookAuditLogEntry webhookEntry => webhookEntry.TargetWebhook?.Name ?? FormatTarget(entry.TargetId),
			_ => FormatTarget(entry.TargetId)
		};

	private static string FormatDetails(DiscordAuditLogEntry entry)
		=> entry switch
		{
			DiscordMessageAuditLogEntry messageEntry when messageEntry.IsBulkDeleteAction
				=> $"Bulk delete in {messageEntry.Channel?.Mention ?? FormatTarget(entry.TargetId)} ({messageEntry.AffectedMessageCount ?? 0} messages)",
			DiscordMessageAuditLogEntry messageEntry when messageEntry.IsPinAction
				=> $"Pin state changed for message `{messageEntry.TargetMessageId}` in {messageEntry.Channel?.Mention ?? "unknown channel"}",
			DiscordMemberAuditLogEntry memberEntry when memberEntry.AddedRoles is { Count: > 0 }
				=> $"Added roles: {string.Join(", ", memberEntry.AddedRoles.Select(static x => x.Name))}",
			DiscordThreadAuditLogEntry threadEntry when threadEntry.ChangeSet.Archived is not null
				=> $"Archived: {threadEntry.ChangeSet.Archived.After}",
			DiscordAutoModerationRuleAuditLogEntry autoModEntry when autoModEntry.IsExecutionAction
				=> $"Executed rule `{autoModEntry.RuleName ?? "unknown rule"}`",
			_ => $"{entry.ActionType} ({entry.ActionCategory})"
		};

	private static string FormatTarget(string? targetId)
		=> string.IsNullOrWhiteSpace(targetId) ? "Unknown target" : $"`{targetId}`";
}
```

This pattern is comfy because:

- the slash command stays responsive and ephemeral
- the browser is rendered as a components-v2 UI kit message
- button handling is kept local to the command with interactivity
- typed audit log families still let you sprinkle in richer rendering for specific cases
- unknown or internal Discord actions still show up instead of disappearing

If you want to make the browser more advanced, good next upgrades are:

- a `user` filter option
- separate browsing for `before` and `after`
- an expanded detail view for the selected entry's `Changes` and `Options`
- a raw fallback page for unknown actions

## Compatibility Notes

DisCatSharp still exposes the older `GetAuditLogsAsync(...)` compatibility overload, but new code should prefer `GetAuditLogEntriesAsync(DiscordAuditLogQuery?)`.

The newer API is better suited for Discord's actual behavior because it:

- makes page ordering explicit
- keeps cursor handling readable
- returns a page object instead of hiding paging details
- fits the typed-family plus raw-fallback model more naturally

## Recommended Pattern

If you are building moderation tooling, dashboards, or forensic utilities, the comfiest approach is usually:

1. query pages with `GetAuditLogEntriesAsync`
2. branch on the typed entry family
3. inspect `Changes` for field-level deltas
4. inspect `Options` for action-specific metadata
5. fall back to `DiscordRawAuditLogEntry` for unknown Discord behavior

That gives you good type safety for common actions without losing forward compatibility when Discord adds new audit log weirdness.
