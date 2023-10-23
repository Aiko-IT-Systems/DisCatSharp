using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for a message being blocked by Auto Moderation.
/// </summary>
public sealed class AutoModerationBlockMessageChangeSet : DiscordAuditLogEntry
{
	public AutoModerationBlockMessageChangeSet()
	{
		this.ValidFor = AuditLogActionType.AutoModerationBlockMessage;
	}

	/// <summary>
	/// The rule's name that blocked the message.
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

	/// <summary>
	/// The member that was timed out.
	/// </summary>
	public DiscordMember TargetMember => this.Discord.Guilds[this.GuildId].Members[this.TargetId.Value!];

	/// <inheritdoc />
	internal override string? ChangeDescription
		=> $"Automod blocked a message from being sended in {this.Channel} from {this.TargetMember} for reason {this.Reason ?? "No reason given".Italic()}";
}
