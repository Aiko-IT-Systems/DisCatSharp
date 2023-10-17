using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating an Auto Moderation rule.
/// </summary>
public sealed class AutoModerationRuleUpdateChangeSet : DiscordAuditLogEntry
{
	internal AutoModerationRuleUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationRuleUpdate;
	}

	public bool EnabledChanged => this.EnabledBefore is not null || this.EnabledAfter is not null;
	public bool? EnabledBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.OldValue;
	public bool? EnabledAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.NewValue;

	public bool RuleNameChanged => this.RuleNameBefore is not null || this.RuleNameAfter is not null;
	public string? RuleNameBefore => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? RuleNameAfter => (string?)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool TriggerTypeChanged => this.TriggerTypeBefore is not null || this.TriggerTypeAfter is not null;
	public AutomodTriggerType? TriggerTypeBefore => (AutomodTriggerType?)this.Changes.FirstOrDefault(x => x.Key == "trigger_type")?.OldValue;
	public AutomodTriggerType? TriggerTypeAfter => (AutomodTriggerType?)this.Changes.FirstOrDefault(x => x.Key == "trigger_type")?.NewValue;

	public bool EventTypeChanged => this.EventTypeBefore is not null || this.EventTypeAfter is not null;
	public AutomodEventType? EventTypeBefore => (AutomodEventType?)this.Changes.FirstOrDefault(x => x.Key == "event_type")?.OldValue;
	public AutomodEventType? EventTypeAfter => (AutomodEventType?)this.Changes.FirstOrDefault(x => x.Key == "event_type")?.NewValue;

	public bool ActionsChanged => this.ActionsBefore is not null || this.ActionsAfter is not null;
	public AutomodAction[]? ActionsBefore => (AutomodAction[]?)this.Changes.FirstOrDefault(x => x.Key == "actions")?.OldValue;
	public AutomodAction[]? ActionsAfter => (AutomodAction[]?)this.Changes.FirstOrDefault(x => x.Key == "actions")?.NewValue;

	public bool TriggerMetadataChanged => this.TriggerMetadataBefore is not null || this.TriggerMetadataAfter is not null;
	public AutomodTriggerMetadata? TriggerMetadataBefore => (AutomodTriggerMetadata?)this.Changes.FirstOrDefault(x => x.Key == "trigger_metadata")?.OldValue;
	public AutomodTriggerMetadata? TriggerMetadataAfter => (AutomodTriggerMetadata?)this.Changes.FirstOrDefault(x => x.Key == "trigger_metadata")?.NewValue;

	public bool ExemptRoleIdsChanged => this.ExemptRoleIdsBefore is not null || this.ExemptRoleIdsAfter is not null;
	public IReadOnlyList<ulong>? ExemptRoleIdsBefore => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_roles")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IReadOnlyList<ulong>? ExemptRoleIdsAfter => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_roles")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordRole> ExemptRolesBefore => this.ExemptRoleIdsBefore.Select(x => this.Discord.Guilds[this.GuildId].Roles[x]);
	public IEnumerable<DiscordRole> ExemptRolesAfter => this.ExemptRoleIdsAfter.Select(x => this.Discord.Guilds[this.GuildId].Roles[x]);

	public bool ExemptChannelIdsChanged => this.ExemptChannelIdsBefore is not null || this.ExemptChannelIdsAfter is not null;
	public IReadOnlyList<ulong>? ExemptChannelIdsBefore => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_channels")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IReadOnlyList<ulong>? ExemptChannelIdsAfter => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_channels")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordChannel> ExemptChannelsBefore => this.ExemptChannelIdsBefore.Select(x => this.Discord.Guilds[this.GuildId].Channels[x]);
	public IEnumerable<DiscordChannel> ExemptChannelsAfter => this.ExemptChannelIdsAfter.Select(x => this.Discord.Guilds[this.GuildId].Channels[x]);
}
