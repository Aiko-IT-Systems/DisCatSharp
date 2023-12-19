using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutomodActionExecuted"/> event.
/// </summary>
public class AutomodActionExecutedEventArgs : DiscordEventArgs
{
	/// <summary>
	/// The guild associated with this event.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// The action that was executed.
	/// </summary>
	public AutomodAction Action { get; internal set; }

	/// <summary>
	/// The id of the rule the action belongs to.
	/// </summary>
	public ulong RuleId { get; internal set; }

	/// <summary>
	/// The type of trigger of the rule which was executed.
	/// </summary>
	public AutomodTriggerType TriggerType { get; internal set; }

	/// <summary>
	/// The member which caused this event.
	/// </summary>
	public DiscordMember Member
		=> this.Guild.Members.TryGetValue(this.UserId, out var member) ? member : this.Guild.GetMemberAsync(this.UserId, true).Result;

	/// <summary>
	/// The user id which caused this event.
	/// </summary>
	public ulong UserId { get; internal set; }

	public DiscordChannel? Channel => this.ChannelId.HasValue ? this.Guild.GetChannel(this.ChannelId.Value) : null;

	/// <summary>
	/// Fall-back channel id this event happened in.
	/// </summary>
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	/// The id of any user message the content belongs to.
	/// This will not exist if the message was blocked or content was not part of message.
	/// </summary>
	public ulong? MessageId { get; internal set; }

	/// <summary>
	/// The id of any system auto moderation messages posted as a result of this action.
	/// This will not exist if the event doesn't correspond to an action with type SendAlertMessage.
	/// </summary>
	public ulong? AlertMessageId { get; internal set; }

	/// <summary>
	/// The user-generated text content.
	/// </summary>
	public string? MessageContent { get; internal set; }

	/// <summary>
	/// The word or phrase configured in the rule that triggered this.
	/// </summary>
	public string? MatchedKeyword { get; internal set; }

	/// <summary>
	/// The substring in the content which triggered the rule.
	/// </summary>
	public string? MatchedContent { get; internal set; }

	public AutomodActionExecutedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
