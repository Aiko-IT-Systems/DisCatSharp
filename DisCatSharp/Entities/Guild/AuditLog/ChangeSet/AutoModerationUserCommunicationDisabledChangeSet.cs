using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a member being timed out by Auto Moderation.
/// </summary>
public sealed class AutoModerationUserCommunicationDisabledChangeSet : DiscordAuditLogEntry
{
	public AutoModerationUserCommunicationDisabledChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationUserCommunicationDisabled;
	}

	/// <summary>
	/// The rule's name that blocked the message and triggered the timeout.
	/// </summary>
	public string RuleName => this.Options.AutoModerationRuleName;

	/// <summary>
	/// What type of filter the rule uses.
	/// </summary>
	public AutomodTriggerType? TriggerType => this.Options.AutoModerationRuleTriggerType;

	/// <summary>
	/// The channel's id in which the rule triggered.
	/// </summary>
	public ulong? ChannelId => this.Options.ChannelId!.Value;

	/// <summary>
	/// The channel in which the rule triggered.
	/// </summary>
	public DiscordChannel Channel => this.Options.Channel;
}
