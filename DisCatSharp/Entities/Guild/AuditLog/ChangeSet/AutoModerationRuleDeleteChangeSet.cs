using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting an Auto Moderation rule.
/// </summary>
public sealed class AutoModerationRuleDeleteChangeSet : DiscordAuditLogEntry
{
	public AutoModerationRuleDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationRuleDelete;
	}

	public bool Enabled => (bool)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.OldValue;
	public string RuleName => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public AutomodTriggerType TriggerType => (AutomodTriggerType)this.Changes.FirstOrDefault(x => x.Key == "trigger_type")?.OldValue;
	public AutomodEventType EventType => (AutomodEventType)this.Changes.FirstOrDefault(x => x.Key == "event_type")?.OldValue;
	public AutomodAction[] Actions => (AutomodAction[])this.Changes.FirstOrDefault(x => x.Key == "actions")?.OldValue;
	public AutomodTriggerMetadata TriggerMetadata => (AutomodTriggerMetadata)this.Changes.FirstOrDefault(x => x.Key == "trigger_metadata")?.OldValue;

	public IReadOnlyList<ulong>? ExemptRoleIds => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_roles")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordRole> ExemptRoles => this.ExemptRoleIds.Select(x => this.Discord.Guilds[this.GuildId].Roles[x]);

	public IReadOnlyList<ulong>? ExemptChannelIds => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_channels")?.OldValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordChannel> ExemptChannels => this.ExemptChannelIds.Select(x => this.Discord.Guilds[this.GuildId].Channels[x]);
}
