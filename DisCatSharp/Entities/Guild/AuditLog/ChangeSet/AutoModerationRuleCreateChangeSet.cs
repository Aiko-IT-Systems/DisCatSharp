using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for creating an Auto Moderation rule.
/// </summary>
public class AutoModerationRuleCreateChangeSet : DiscordAuditLogEntry
{
	internal AutoModerationRuleCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationRuleCreate;
	}

	public bool Enabled => (bool)this.Changes.FirstOrDefault(x => x.Key == "enabled")?.NewValue;
	public string RuleName => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public AutomodTriggerType TriggerType => (AutomodTriggerType)this.Changes.FirstOrDefault(x => x.Key == "trigger_type")?.NewValue;
	public AutomodEventType EventType => (AutomodEventType)this.Changes.FirstOrDefault(x => x.Key == "event_type")?.NewValue;
	public AutomodAction[] Actions => (AutomodAction[])this.Changes.FirstOrDefault(x => x.Key == "actions")?.NewValue;
	public AutomodTriggerMetadata TriggerMetadata => (AutomodTriggerMetadata)this.Changes.FirstOrDefault(x => x.Key == "trigger_metadata")?.NewValue;

	public IReadOnlyList<ulong>? ExemptRoleIds => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_roles")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordRole> ExemptRoles => this.ExemptRoleIds.Select(x => this.Discord.Guilds[this.GuildId].Roles[x]);

	public IReadOnlyList<ulong>? ExemptChannelIds => ((IReadOnlyList<string>?)this.Changes.FirstOrDefault(x => x.Key == "exempt_channels")?.NewValue)?.Select(x => ConvertToUlong(x)!.Value).ToList();
	public IEnumerable<DiscordChannel> ExemptChannels => this.ExemptChannelIds.Select(x => this.Discord.Guilds[this.GuildId].Channels[x]);
}
