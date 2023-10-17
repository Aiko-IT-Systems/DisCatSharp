using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a message being blocked by Auto Moderation.
/// </summary>
public class AutoModerationBlockMessageChangeSet : DiscordAuditLogEntry
{
	public AutoModerationBlockMessageChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationBlockMessage;
	}

	public string RuleName => this.Options.AutoModerationRuleName;
	public AutomodTriggerType? TriggerType => this.Options.AutoModerationRuleTriggerType;

	public ulong ChannelId => this.Options.ChannelId!.Value;
	public DiscordChannel Channel => this.Options.Channel;
}
