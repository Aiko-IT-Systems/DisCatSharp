using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.AuditLogs;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.AuditLogs;

/// <summary>
///     Covers regression-prone audit log parser behavior introduced by the audit log V1 rewrite.
/// </summary>
public sealed class AuditLogParserTests
{
	/// <summary>
	///     Verifies that overwrite entries expose typed overwrite target semantics.
	/// </summary>
	[Fact]
	public void ParsePage_OverwriteEntry_ExposesTypedOverwriteTargetKind()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(302UL, "raid-room", ChannelType.Text);
		var role = harness.AddRole(404UL, "Raid Team");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90006UL,
				ActionType = AuditLogActionType.OverwriteCreate,
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					Id = role.Id,
					Type = "0",
					RoleName = role.Name
				}
			});

		var entry = Assert.IsType<DiscordOverwriteAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogOverwriteTargetType.Role, entry.OverwriteTargetType);
		Assert.True(entry.TargetsRole);
		Assert.False(entry.TargetsMember);
	}

	/// <summary>
	///     Verifies that hydration can upgrade synthetic actor, member, and channel references from cache without
	///     forcing REST calls.
	/// </summary>
	[Fact]
	public async Task HydrateAllAsync_MessageEntry_UpgradesCachedReferencesWithoutRestCalls()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90032UL,
				ActionType = AuditLogActionType.MessageDelete,
				TargetId = "606",
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = 505UL,
					Count = "1"
				}
			});

		var entry = Assert.IsType<DiscordMessageAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(42UL, entry.Actor?.Id);
		Assert.Equal("Audit Cat", entry.Actor?.Username);
		Assert.Equal(505UL, entry.Channel?.Id);
		Assert.Null(entry.Channel?.Name);
		Assert.Equal(606UL, entry.TargetMember?.Id);
		Assert.Equal("Unknown User", entry.TargetMember?.Username);

		var cachedActor = harness.AddUser(42UL, "Live Mod");
		var cachedChannel = harness.AddChannel(505UL, "live-chat", ChannelType.Text);
		var cachedTargetMember = harness.AddMember(606UL, "Live Target");

		await entry.HydrateAllAsync(force: false);

		Assert.Same(cachedActor, entry.Actor);
		Assert.Same(cachedChannel, entry.Channel);
		Assert.Same(cachedTargetMember, entry.TargetMember);
	}

	/// <summary>
	///     Verifies that overwrite target hydration can resolve the overwritten principal independently from related
	///     channel references.
	/// </summary>
	[Fact]
	public async Task HydrateAsync_OverwriteEntry_UpgradesOverwriteTargetWithoutHydratingRelatedChannel()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90033UL,
				ActionType = AuditLogActionType.OverwriteUpdate,
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = 303UL,
					Id = 404UL,
					Type = "0",
					RoleName = "Raid Team"
				}
			});

		var entry = Assert.IsType<DiscordOverwriteAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(303UL, entry.Channel?.Id);
		Assert.Null(entry.Channel?.Name);
		Assert.Null(entry.OverwrittenRole);

		var cachedRole = harness.AddRole(404UL, "Raid Team");

		await entry.HydrateAsync(AuditLogHydrationTargets.Target, force: false);

		Assert.Same(cachedRole, entry.OverwrittenRole);
		Assert.Null(entry.OverwrittenMember);
		Assert.Equal(303UL, entry.Channel?.Id);
		Assert.Null(entry.Channel?.Name);
	}

	/// <summary>
	///     Verifies that guild-scoped entries expose their typed change helper view.
	/// </summary>
	[Fact]
	public void ParsePage_GuildUpdateEntry_ExposesGuildChangeSet()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90000UL,
				ActionType = AuditLogActionType.GuildUpdate,
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "name",
						OldValue = JValue.CreateString("Old Guild"),
						NewValue = JValue.CreateString("New Guild")
					},
					new RawAuditLogChange
					{
						Key = "verification_level",
						OldValue = new JValue((int)VerificationLevel.None),
						NewValue = new JValue((int)VerificationLevel.High)
					}
				]
			});

		var entry = Assert.IsType<DiscordGuildAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal("Old Guild", entry.ChangeSet.Name?.Before);
		Assert.Equal("New Guild", entry.ChangeSet.Name?.After);
		Assert.Equal(VerificationLevel.None, entry.ChangeSet.VerificationLevel?.Before);
		Assert.Equal(VerificationLevel.High, entry.ChangeSet.VerificationLevel?.After);
	}

	/// <summary>
	///     Verifies that scalar value changes can be projected into the reusable typed helper model.
	/// </summary>
	[Fact]
	public void ParsePage_ThreadUpdateEntry_ProjectsTypedValueChanges()
	{
		using var harness = new AuditLogTestHarness();
		var thread = harness.AddChannel(706UL, "incident-thread", ChannelType.PublicThread);

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90001UL,
				ActionType = AuditLogActionType.ThreadUpdate,
				TargetId = thread.Id.ToString(),
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "name",
						OldValue = JValue.CreateString("incident-old"),
						NewValue = JValue.CreateString("incident-thread")
					},
					new RawAuditLogChange
					{
						Key = "archived",
						OldValue = new JValue(false),
						NewValue = new JValue(true)
					}
				]
			});

		var entry = Assert.IsType<DiscordThreadAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));
		var nameChange = entry.GetChange("name")?.ToValueChange<string>();
		var archivedChange = entry.GetChange("archived")?.ToValueChange<bool>();

		Assert.NotNull(nameChange);
		Assert.True(nameChange!.HasBefore);
		Assert.True(nameChange.HasAfter);
		Assert.Equal("incident-old", nameChange.Before);
		Assert.Equal("incident-thread", nameChange.After);

		Assert.NotNull(archivedChange);
		Assert.False(archivedChange!.Before);
		Assert.True(archivedChange.After);
		Assert.False(entry.GetChange("archived")?.GetOldBoolean());
		Assert.True(entry.GetChange("archived")?.GetNewBoolean());
		Assert.Equal("incident-thread", entry.ChangeSet.Name?.After);
		Assert.True(entry.ChangeSet.Archived?.After);
	}

	/// <summary>
	///     Verifies that timestamp-shaped change values can be converted without manual string parsing.
	/// </summary>
	[Fact]
	public void ParsePage_MemberVerificationUpdateEntry_ProjectsTypedDateTimeOffsetChanges()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90002UL,
				ActionType = AuditLogActionType.GuildMemberVerificationUpdate,
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "version",
						OldValue = JValue.CreateString("2026-03-19T08:30:00.000000+00:00"),
						NewValue = JValue.CreateString("2026-03-20T09:45:00.000000+00:00")
					}
				]
			});

		var entry = Assert.IsType<DiscordMemberVerificationAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));
		var versionChange = entry.GetChange("version")?.ToDateTimeOffsetChange();

		Assert.NotNull(versionChange);
		Assert.Equal(new DateTimeOffset(2026, 3, 19, 8, 30, 0, TimeSpan.Zero), versionChange!.Before);
		Assert.Equal(new DateTimeOffset(2026, 3, 20, 9, 45, 0, TimeSpan.Zero), versionChange.After);
	}

	/// <summary>
	///     Verifies that Discord's partial role delta arrays can be converted into typed partial role collections.
	/// </summary>
	[Fact]
	public void ParsePage_MemberRoleUpdateEntry_ProjectsPartialRoleCollections()
	{
		using var harness = new AuditLogTestHarness();
		var member = harness.AddMember(807UL, "Role Goblin");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90003UL,
				ActionType = AuditLogActionType.MemberRoleUpdate,
				TargetId = member.Id.ToString(),
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "$add",
						NewValue = JArray.Parse("""[{ "id": "404", "name": "Raid Team" }, { "id": "405", "name": "Mods" }]""")
					}
				]
			});

		var entry = Assert.IsType<DiscordMemberAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));
		var addChange = entry.AddedRolesChange?.ToPartialRoleCollectionChange();

		Assert.NotNull(addChange);
		Assert.False(addChange!.HasBefore);
		Assert.True(addChange.HasAfter);
		Assert.Collection(addChange.After!,
			role =>
			{
				Assert.Equal(404UL, role.Id);
				Assert.Equal("Raid Team", role.Name);
			},
			role =>
			{
				Assert.Equal(405UL, role.Id);
				Assert.Equal("Mods", role.Name);
			});
		Assert.Equal("Raid Team", entry.ChangeSet.AddedRoles?.After?[0].Name);
		Assert.Equal(404UL, entry.AddedRoles?[0].Id);
	}

	/// <summary>
	///     Verifies that role entries expose typed change helpers for common role fields.
	/// </summary>
	[Fact]
	public void ParsePage_RoleUpdateEntry_ExposesRoleChangeSet()
	{
		using var harness = new AuditLogTestHarness();
		var role = harness.AddRole(404UL, "Raid Team");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90004UL,
				ActionType = AuditLogActionType.RoleUpdate,
				TargetId = role.Id.ToString(),
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "permissions",
						OldValue = JValue.CreateString(((long)Permissions.ManageMessages).ToString()),
						NewValue = JValue.CreateString(((long)(Permissions.ManageMessages | Permissions.ManageThreads)).ToString())
					},
					new RawAuditLogChange
					{
						Key = "mentionable",
						OldValue = new JValue(false),
						NewValue = new JValue(true)
					}
				]
			});

		var entry = Assert.IsType<DiscordRoleAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(Permissions.ManageMessages, entry.ChangeSet.Permissions?.Before);
		Assert.Equal(Permissions.ManageMessages | Permissions.ManageThreads, entry.ChangeSet.Permissions?.After);
		Assert.True(entry.ChangeSet.Mentionable?.After);
	}

	/// <summary>
	///     Verifies that auto moderation rule mutations expose the typed helper view for common rule fields.
	/// </summary>
	[Fact]
	public void ParsePage_AutoModerationRuleUpdateEntry_ExposesAutoModerationChangeSet()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90005UL,
				ActionType = AuditLogActionType.AutoModerationRuleUpdate,
				TargetId = "555",
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "enabled",
						OldValue = new JValue(false),
						NewValue = new JValue(true)
					},
					new RawAuditLogChange
					{
						Key = "exempt_roles",
						OldValue = new JArray("404"),
						NewValue = new JArray("404", "405")
					}
				]
			});

		var entry = Assert.IsType<DiscordAutoModerationRuleAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.True(entry.ChangeSet.Enabled?.After);
		Assert.Equal([404UL, 405UL], entry.ChangeSet.ExemptRoles?.After ?? []);
		Assert.True(entry.IsRuleMutationAction);
		Assert.False(entry.IsExecutionAction);
	}

	/// <summary>
	///     Verifies that the high-level guild API rejects mutually exclusive before and after cursors.
	/// </summary>
	[Fact]
	public async Task GetAuditLogEntriesAsync_BeforeAndAfterCursors_ThrowsArgumentException()
	{
		using var harness = new AuditLogTestHarness();

		await Assert.ThrowsAsync<ArgumentException>(() => harness.Guild.GetAuditLogEntriesAsync(new()
		{
			Before = 1UL,
			After = 2UL
		}));
	}

	/// <summary>
	///     Verifies that the parser resolves voice channel status entries even when Discord only provides the channel id
	///     in the options payload.
	/// </summary>
	[Fact]
	public void ParsePage_VoiceChannelStatusEntry_UsesOptionsChannelFallback()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(101UL, "voice-lobby", ChannelType.Voice);

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9001UL,
				ActionType = AuditLogActionType.VoiceChannelStatusCreate,
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					Status = "active"
				}
			});

		var page = AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false);

		var entry = Assert.IsType<DiscordVoiceChannelStatusAuditLogEntry>(Assert.Single(page.Entries));
		Assert.False(page.IsAscending);
		Assert.Equal(9001UL, page.FirstEntryId);
		Assert.Equal(9001UL, page.LastEntryId);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal("voice-lobby", entry.Channel?.Name);
		Assert.Equal("active", entry.Status);
		Assert.Equal("active", entry.Options?.Status);
		Assert.Equal("active", entry.RawOptions?.Value<string>("status"));
		Assert.Equal(AuditLogActionCategory.Create, entry.ActionCategory);
		Assert.Equal(42UL, entry.Actor?.Id);
		Assert.Equal("Audit Cat", entry.Actor?.Username);
		Assert.False(entry.HasChanges);
	}

	/// <summary>
	///     Verifies that internal guild profile update entries are typed and retain generic change access.
	/// </summary>
	[Fact]
	public void ParsePage_GuildProfileUpdateEntry_PreservesChanges()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9002UL,
				ActionType = AuditLogActionType.GuildProfileUpdate,
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "game_application_ids",
						OldValue = new JArray(111UL),
						NewValue = new JArray(111UL, 222UL)
					}
				]
			});

		var entry = Assert.IsType<DiscordGuildProfileAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: true).Entries));
		var change = entry.GetChange("game_application_ids");

		Assert.Equal(AuditLogActionCategory.Update, entry.ActionCategory);
		Assert.True(entry.HasChanges);
		Assert.NotNull(change);
		Assert.True(entry.TryGetChange("game_application_ids", out var typedChange));
		Assert.False(entry.TryGetChange("Game_Application_Ids", out _));
		Assert.Equal([111UL, 222UL], typedChange.GetNewValue<ulong[]>() ?? []);
		Assert.Equal("game_application_ids", entry.RawChanges[0].Value<string>("key"));
	}

	/// <summary>
	///     Verifies that options-heavy auto moderation actions are typed without requiring a change list.
	/// </summary>
	[Fact]
	public void ParsePage_AutoModerationOptionsOnlyEntry_ParsesTypedOptions()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(202UL, "mod-log", ChannelType.Text);
		harness.AddMember(42UL, "Rule Breaker");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9003UL,
				ActionType = AuditLogActionType.AutoModerationBlockMessage,
				TargetId = "42",
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					AutoModerationRuleName = "No Meowing In General",
					AutoModerationRuleTriggerType = "keyword",
					ChannelId = channel.Id
				}
			});

		var entry = Assert.IsType<DiscordAutoModerationRuleAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Null(entry.AutoModerationRuleId);
		Assert.Equal("No Meowing In General", entry.RuleName);
		Assert.Equal("keyword", entry.TriggerType);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal(42UL, entry.TargetMember?.Id);
		Assert.Empty(entry.Changes);
		Assert.Equal("No Meowing In General", entry.Options?.AutoModerationRuleName);
		Assert.Equal("keyword", entry.Options?.AutoModerationRuleTriggerType);
		Assert.Equal(channel.Id, entry.Options?.ChannelId);
		Assert.Equal("keyword", entry.RawOptions?.Value<string>("auto_moderation_rule_trigger_type"));
		Assert.Equal(AuditLogActionCategory.Other, entry.ActionCategory);
	}

	/// <summary>
	///     Verifies that auto moderation quarantine actions stay in the auto moderation entry family rather than
	///     degrading to the generic member family.
	/// </summary>
	[Fact]
	public void ParsePage_AutoModerationQuarantineEntry_UsesAutoModerationFamily()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(203UL, "quarantine-log", ChannelType.Text);
		harness.AddMember(43UL, "Quarantined Cat");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 90031UL,
				ActionType = AuditLogActionType.AutoModerationQuarantineUser,
				TargetId = "43",
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					AutoModerationRuleName = "Quarantine Gremlins",
					AutoModerationRuleTriggerType = "mention_spam",
					ChannelId = channel.Id
				}
			});

		var entry = Assert.IsType<DiscordAutoModerationRuleAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal("Quarantine Gremlins", entry.RuleName);
		Assert.Equal("mention_spam", entry.TriggerType);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal(43UL, entry.TargetMember?.Id);
		Assert.Null(entry.AutoModerationRuleId);
	}

	/// <summary>
	///     Verifies that unmapped action types fall back to the raw entry model while preserving unknown option fields.
	/// </summary>
	[Fact]
	public void ParsePage_UnknownAction_FallsBackToRawEntryAndKeepsUnknownOptions()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9004UL,
				ActionType = (AuditLogActionType)9999,
				TargetId = "7777",
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "mystery_key",
						OldValue = JValue.CreateString("old"),
						NewValue = JValue.CreateString("new")
					}
				],
				Options = new RawAuditLogEntryOptions
				{
					Count = "5",
					AdditionalData = new Dictionary<string, JToken>
					{
						["mystery_flag"] = JValue.CreateString("nya")
					}
				}
			});

		var entry = Assert.IsType<DiscordRawAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Other, entry.ActionCategory);
		Assert.Equal("7777", entry.TargetId);
		Assert.Equal("new", entry.GetChange("mystery_key")?.GetNewValue<string>());
		Assert.Equal(5, entry.Options?.Count);
		Assert.Equal("nya", entry.Options?.RawObject.Value<string>("mystery_flag"));
		Assert.Equal("nya", entry.RawOptions?.Value<string>("mystery_flag"));
	}

	/// <summary>
	///     Verifies that overwrite entries resolve their target data from the options object.
	/// </summary>
	[Fact]
	public void ParsePage_OverwriteEntry_UsesOptionFields()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(303UL, "raid-room", ChannelType.Text);
		var role = harness.AddRole(404UL, "Raid Team");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9005UL,
				ActionType = AuditLogActionType.OverwriteUpdate,
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					Id = role.Id,
					Type = "0",
					RoleName = role.Name
				}
			});

		var entry = Assert.IsType<DiscordOverwriteAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Update, entry.ActionCategory);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal(role.Id, entry.OverwrittenEntityId);
		Assert.Equal("0", entry.OverwrittenEntityType);
		Assert.Equal("Raid Team", entry.RoleName);
		Assert.Equal(role.Id, entry.Options?.Id);
		Assert.Equal("0", entry.Options?.Type);
	}

	/// <summary>
	///     Verifies that message deletion entries remain useful even when Discord only provides options data.
	/// </summary>
	[Fact]
	public void ParsePage_MessageDeleteEntry_UsesOptionsWithoutChanges()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(505UL, "kitty-chat", ChannelType.Text);
		var targetMember = harness.AddMember(606UL, "Pinned Poster");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9006UL,
				ActionType = AuditLogActionType.MessageDelete,
				TargetId = targetMember.Id.ToString(),
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					Count = "3",
					MessageId = 707UL
				}
			});

		var entry = Assert.IsType<DiscordMessageAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Delete, entry.ActionCategory);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal(targetMember.Id, entry.TargetMember?.Id);
		Assert.Equal("Pinned Poster", entry.TargetMember?.DisplayName);
		Assert.Equal(707UL, entry.MessageId);
		Assert.Equal(3, entry.Count);
		Assert.Equal(3, entry.AffectedMessageCount);
		Assert.Equal(707UL, entry.TargetMessageId);
		Assert.False(entry.HasChanges);
		Assert.Equal(3, entry.Options?.Count);
		Assert.Equal(707UL, entry.Options?.MessageId);
		Assert.False(entry.IsBulkDeleteAction);
		Assert.False(entry.IsPinAction);
	}

	/// <summary>
	///     Verifies that thread update entries preserve the diverse raw change payload Discord emits in practice.
	/// </summary>
	[Fact]
	public void ParsePage_ThreadUpdateEntry_PreservesChangeKeys()
	{
		using var harness = new AuditLogTestHarness();
		var thread = harness.AddChannel(707UL, "incident-thread", ChannelType.PublicThread);

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9007UL,
				ActionType = AuditLogActionType.ThreadUpdate,
				TargetId = thread.Id.ToString(),
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "name",
						OldValue = JValue.CreateString("incident-old"),
						NewValue = JValue.CreateString("incident-thread")
					},
					new RawAuditLogChange
					{
						Key = "archived",
						OldValue = new JValue(false),
						NewValue = new JValue(true)
					},
					new RawAuditLogChange
					{
						Key = "applied_tags",
						OldValue = new JArray("tag-a"),
						NewValue = new JArray("tag-a", "tag-b")
					}
				]
			});

		var entry = Assert.IsType<DiscordThreadAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Update, entry.ActionCategory);
		Assert.Equal(thread.Id, entry.TargetThread?.Id);
		Assert.Equal("incident-thread", entry.NameChange?.GetNewValue<string>());
		Assert.True(entry.ArchivedChange?.GetNewValue<bool>());
		Assert.Equal(["tag-a", "tag-b"], entry.GetChange("applied_tags")?.GetNewValue<string[]>() ?? []);
	}

	/// <summary>
	///     Verifies that cached thread channel instances win over side-loaded thread payloads so the richer runtime type is preserved.
	/// </summary>
	[Fact]
	public void ParsePage_ThreadUpdateEntry_PrefersCachedThreadChannelOverSideLoadedThread()
	{
		using var harness = new AuditLogTestHarness();
		var cachedThread = harness.AddThreadChannel(708UL, "cached-incident-thread");

		var rawAuditLog = new RawAuditLog
		{
			Users =
			[
				new RawAuditLogUser
				{
					Id = 42UL,
					Username = "Audit Cat",
					Discriminator = "0",
					GlobalName = "Lala Fixture"
				}
			],
			Threads =
			[
				new RawAuditLogThread
				{
					Id = cachedThread.Id,
					GuildId = harness.Guild.Id,
					ParentId = 123UL,
					Name = "payload-thread-name",
					Type = ChannelType.PublicThread
				}
			],
			Entries =
			[
				new RawAuditLogEntry
				{
					Id = 90016UL,
					ActionType = AuditLogActionType.ThreadUpdate,
					TargetId = cachedThread.Id.ToString(),
					UserId = 42UL
				}
			]
		};

		var entry = Assert.IsType<DiscordThreadAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.IsType<DiscordThreadChannel>(entry.TargetThread);
		Assert.Same(cachedThread, entry.TargetThread);
	}

	/// <summary>
	///     Verifies that member role update entries preserve Discord's raw <c>$add</c> and <c>$remove</c> role deltas.
	/// </summary>
	[Fact]
	public void ParsePage_MemberRoleUpdateEntry_PreservesRoleDeltaArrays()
	{
		using var harness = new AuditLogTestHarness();
		var member = harness.AddMember(808UL, "Role Kitty");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9008UL,
				ActionType = AuditLogActionType.MemberRoleUpdate,
				TargetId = member.Id.ToString(),
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "$add",
						NewValue = JArray.Parse("""[{ "id": "404", "name": "Raid Team" }]""")
					},
					new RawAuditLogChange
					{
						Key = "$remove",
						NewValue = JArray.Parse("""[{ "id": "505", "name": "Muted" }]""")
					}
				]
			});

		var entry = Assert.IsType<DiscordMemberAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(member.Id, entry.TargetMember?.Id);
		Assert.Equal("Role Kitty", entry.TargetMember?.DisplayName);
		Assert.Equal("Raid Team", entry.AddedRolesChange?.GetNewValue<JArray>()?[0]?["name"]?.Value<string>());
		Assert.Equal("Muted", entry.RemovedRolesChange?.GetNewValue<JArray>()?[0]?["name"]?.Value<string>());
	}

	/// <summary>
	///     Verifies that member move style entries expose their stable options in addition to the typed target member.
	/// </summary>
	[Fact]
	public void ParsePage_MemberMoveEntry_UsesTypedOptionConveniences()
	{
		using var harness = new AuditLogTestHarness();
		var member = harness.AddMember(818UL, "Moved Cat");
		var channel = harness.AddChannel(819UL, "voice-jail", ChannelType.Voice);

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9010UL,
				ActionType = AuditLogActionType.MemberMove,
				TargetId = member.Id.ToString(),
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					Count = "2"
				}
			});

		var entry = Assert.IsType<DiscordMemberAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Other, entry.ActionCategory);
		Assert.Equal(member.Id, entry.TargetMember?.Id);
		Assert.Equal(channel.Id, entry.Channel?.Id);
		Assert.Equal(2, entry.Count);
		Assert.Equal(2, entry.Options?.Count);
	}

	/// <summary>
	///     Verifies that guild scheduled event exception actions use the parent event as the typed target while exposing
	///     the scoped exception id from the options payload.
	/// </summary>
	[Fact]
	public void ParsePage_ScheduledEventExceptionEntry_UsesParentEventAndExceptionId()
	{
		using var harness = new AuditLogTestHarness();
		var scheduledEvent = harness.AddScheduledEvent(9091UL, "Weekly Meowdown");

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9011UL,
				ActionType = AuditLogActionType.GuildScheduledEventExceptionCreate,
				TargetId = scheduledEvent.Id.ToString(),
				UserId = 42UL,
				Options = new RawAuditLogEntryOptions
				{
					EventExceptionId = 202603200000000000UL
				}
			});

		var entry = Assert.IsType<DiscordGuildScheduledEventExceptionAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Create, entry.ActionCategory);
		Assert.Equal(scheduledEvent.Id, entry.TargetGuildScheduledEvent?.Id);
		Assert.Equal("Weekly Meowdown", entry.TargetGuildScheduledEvent?.Name);
		Assert.Equal(202603200000000000UL, entry.GuildScheduledEventExceptionId);
		Assert.Equal(202603200000000000UL, entry.Options?.EventExceptionId);
	}

	/// <summary>
	///     Verifies that the pin permission migration system action is elevated into a tiny typed entry instead of
	///     staying in the raw fallback bucket.
	/// </summary>
	[Fact]
	public void ParsePage_PermissionMigrationEntry_UsesTypedPermissionTransition()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9012UL,
				ActionType = AuditLogActionType.GuildMigratePinPermission,
				UserId = 42UL
			});

		var entry = Assert.IsType<DiscordPermissionMigrationAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Other, entry.ActionCategory);
		Assert.Equal([Permissions.ManageMessages], entry.LegacyPermissions);
		Assert.Equal(Permissions.ManageMessages, entry.LegacyPermission);
		Assert.Equal(Permissions.PinMessages, entry.ReplacementPermission);
	}

	/// <summary>
	///     Verifies that member verification updates are parsed into their own typed family while keeping the generic
	///     change contract intact.
	/// </summary>
	[Fact]
	public void ParsePage_MemberVerificationUpdateEntry_UsesTypedFamilyAndPreservesChanges()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9013UL,
				ActionType = AuditLogActionType.GuildMemberVerificationUpdate,
				UserId = 42UL,
				Changes =
				[
					new RawAuditLogChange
					{
						Key = "description",
						OldValue = JValue.CreateString("Old screening text"),
						NewValue = JValue.CreateString("Read the rules and be nice")
					},
					new RawAuditLogChange
					{
						Key = "version",
						OldValue = JValue.CreateString("2026-03-19T08:30:00.000000+00:00"),
						NewValue = JValue.CreateString("2026-03-20T09:45:00.000000+00:00")
					}
				]
			});

		var entry = Assert.IsType<DiscordMemberVerificationAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Update, entry.ActionCategory);
		Assert.Equal(harness.Guild.Id, entry.TargetGuild.Id);
		Assert.Equal("Read the rules and be nice", entry.GetChange("description")?.GetNewValue<string>());
		Assert.Equal("2026-03-20T09:45:00.000000+00:00", entry.GetChange("version")?.GetNewValue<string>());
	}

	/// <summary>
	///     Verifies that the bypass slowmode migration action can describe a many-to-one permission migration.
	/// </summary>
	[Fact]
	public void ParsePage_BypassSlowmodePermissionMigrationEntry_UsesGeneralizedPermissionContract()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9014UL,
				ActionType = AuditLogActionType.GuildMigrateBypassSlowmodePermission,
				UserId = 42UL
			});

		var entry = Assert.IsType<DiscordPermissionMigrationAuditLogEntry>(Assert.Single(AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries));

		Assert.Equal(AuditLogActionCategory.Other, entry.ActionCategory);
		Assert.Equal([Permissions.ManageMessages, Permissions.ManageChannels, Permissions.ManageThreads], entry.LegacyPermissions);
		Assert.Null(entry.LegacyPermission);
		Assert.Equal(Permissions.BypassSlowmode, entry.ReplacementPermission);
	}

	/// <summary>
	///     Verifies that creator monetization system events are parsed into an intentional guild-scoped entry family
	///     rather than falling through to the raw fallback path.
	/// </summary>
	[Fact]
	public void ParsePage_CreatorMonetizationEntries_UseTypedGuildScopedFamily()
	{
		using var harness = new AuditLogTestHarness();

		var rawAuditLog = harness.CreateAuditLog(
			new RawAuditLogEntry
			{
				Id = 9015UL,
				ActionType = AuditLogActionType.CreatorMonetizationRequestCreated,
				UserId = 42UL
			},
			new RawAuditLogEntry
			{
				Id = 9016UL,
				ActionType = AuditLogActionType.CreatorMonetizationTermsAccepted,
				UserId = 42UL
			});

		var entries = AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false).Entries;

		var requestEntry = Assert.IsType<DiscordCreatorMonetizationAuditLogEntry>(entries[0]);
		var termsEntry = Assert.IsType<DiscordCreatorMonetizationAuditLogEntry>(entries[1]);

		Assert.Equal(AuditLogActionCategory.Other, requestEntry.ActionCategory);
		Assert.Equal(AuditLogActionCategory.Other, termsEntry.ActionCategory);
		Assert.Equal(harness.Guild.Id, requestEntry.TargetGuild.Id);
		Assert.Equal(harness.Guild.Id, termsEntry.TargetGuild.Id);
		Assert.False(requestEntry.HasChanges);
		Assert.False(termsEntry.HasChanges);
	}

	/// <summary>
	///     Verifies that gateway-style single entry parsing works without a side-loaded audit log page payload.
	/// </summary>
	[Fact]
	public void ParseEntry_GatewayStyleEntry_UsesClientCacheForActor()
	{
		using var harness = new AuditLogTestHarness();
		var channel = harness.AddChannel(909UL, "pins", ChannelType.Text);
		harness.AddUser(1000UL, "Cached Mod");

		var entry = AuditLogEntryParserRegistry.Instance.ParseEntry(
			harness.Guild,
			new AuditLogReferenceStore(harness.Guild, null),
			new RawAuditLogEntry
			{
				Id = 9009UL,
				ActionType = AuditLogActionType.MessagePin,
				UserId = 1000UL,
				Options = new RawAuditLogEntryOptions
				{
					ChannelId = channel.Id,
					MessageId = 1111UL
				}
			});

		var messageEntry = Assert.IsType<DiscordMessageAuditLogEntry>(entry);
		Assert.Equal(1000UL, messageEntry.Actor?.Id);
		Assert.Equal("Cached Mod", messageEntry.Actor?.Username);
		Assert.Equal(channel.Id, messageEntry.Channel?.Id);
		Assert.Equal(1111UL, messageEntry.MessageId);
		Assert.Equal(AuditLogActionCategory.Other, messageEntry.ActionCategory);
	}

	/// <summary>
	///     Provides a minimal guild/client fixture for audit log parser tests.
	/// </summary>
	private sealed class AuditLogTestHarness : IDisposable
	{
		/// <summary>
		///     Gets the client backing the fixture.
		/// </summary>
		public DiscordClient Client { get; }

		/// <summary>
		///     Gets the guild under test.
		/// </summary>
		public DiscordGuild Guild { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="AuditLogTestHarness"/> class.
		/// </summary>
		public AuditLogTestHarness()
		{
			this.Client = new(new DiscordConfiguration
			{
				Token = "copilot-tests-token",
				MessageCacheSize = 0,
				AutoReconnect = false
			});

			this.Guild = new DiscordGuild
			{
				Discord = this.Client,
				Id = 123456789012345678UL,
				Name = "Audit Log Test Guild"
			};
		}

		/// <summary>
		///     Adds a cached channel to the test guild.
		/// </summary>
		/// <param name="id">The channel id.</param>
		/// <param name="name">The channel name.</param>
		/// <param name="type">The channel type.</param>
		/// <returns>The created channel.</returns>
		public DiscordChannel AddChannel(ulong id, string name, ChannelType type)
		{
			var channel = new DiscordChannel
			{
				Discord = this.Client,
				Id = id,
				GuildId = this.Guild.Id,
				Name = name,
				Type = type
			};

			this.Guild.ChannelsInternal[id] = channel;
			return channel;
		}

		/// <summary>
		///     Adds a cached thread channel to the test guild.
		/// </summary>
		/// <param name="id">The thread id.</param>
		/// <param name="name">The thread name.</param>
		/// <returns>The created thread channel.</returns>
		public DiscordThreadChannel AddThreadChannel(ulong id, string name)
		{
			var thread = (DiscordThreadChannel)Activator.CreateInstance(typeof(DiscordThreadChannel), nonPublic: true)!;
			thread.Discord = this.Client;
			thread.Id = id;
			thread.GuildId = this.Guild.Id;
			thread.Name = name;
			thread.Type = ChannelType.PublicThread;

			this.Guild.ChannelsInternal[id] = thread;
			this.Guild.ThreadsInternal[id] = thread;
			return thread;
		}

		/// <summary>
		///     Adds a cached role to the test guild.
		/// </summary>
		/// <param name="id">The role id.</param>
		/// <param name="name">The role name.</param>
		/// <returns>The created role.</returns>
		public DiscordRole AddRole(ulong id, string name)
		{
			var role = new DiscordRole
			{
				Discord = this.Client,
				Id = id,
				GuildId = this.Guild.Id,
				Name = name
			};

			this.Guild.RolesInternal[id] = role;
			return role;
		}

		/// <summary>
		///     Adds a cached user to the client.
		/// </summary>
		/// <param name="id">The user id.</param>
		/// <param name="username">The username.</param>
		/// <returns>The created user.</returns>
		public DiscordUser AddUser(ulong id, string username)
		{
			var user = new DiscordUser
			{
				Discord = this.Client,
				Id = id,
				Username = username,
				Discriminator = "0"
			};

			this.Client.UserCache[id] = user;
			return user;
		}

		/// <summary>
		///     Adds a cached member to the test guild.
		/// </summary>
		/// <param name="id">The member user id.</param>
		/// <param name="displayName">The display name to expose through the partial member.</param>
		/// <returns>The created member.</returns>
		public DiscordMember AddMember(ulong id, string displayName)
		{
			var user = this.AddUser(id, displayName.Replace(" ", string.Empty));
			var member = new DiscordMember(user)
			{
				Discord = this.Client,
				GuildId = this.Guild.Id,
				Nickname = displayName
			};

			this.Guild.MembersInternal[id] = member;
			return member;
		}

		/// <summary>
		///     Adds a cached scheduled event to the test guild.
		/// </summary>
		/// <param name="id">The scheduled event id.</param>
		/// <param name="name">The event name.</param>
		/// <returns>The created scheduled event.</returns>
		public DiscordScheduledEvent AddScheduledEvent(ulong id, string name)
		{
			var scheduledEvent = new DiscordScheduledEvent
			{
				Discord = this.Client,
				Id = id,
				GuildId = this.Guild.Id,
				Name = name
			};

			this.Guild.ScheduledEventsInternal[id] = scheduledEvent;
			return scheduledEvent;
		}

		/// <summary>
		///     Creates a raw audit log payload with a synthetic actor entry.
		/// </summary>
		/// <param name="entries">The entries to include in the payload.</param>
		/// <returns>A raw audit log payload suitable for parser tests.</returns>
		public RawAuditLog CreateAuditLog(params RawAuditLogEntry[] entries)
			=> new()
			{
				Users =
				[
					new RawAuditLogUser
					{
						Id = 42UL,
						Username = "Audit Cat",
						Discriminator = "0",
						GlobalName = "Lala Fixture"
					}
				],
				Entries = entries
			};

		/// <summary>
		///     Disposes the underlying client resources.
		/// </summary>
		public void Dispose()
			=> this.Client.Dispose();
	}
}
