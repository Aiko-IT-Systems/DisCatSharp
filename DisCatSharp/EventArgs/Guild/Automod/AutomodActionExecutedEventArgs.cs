// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public DiscordChannel? Channel
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		=> this.ChannelId.HasValue ? this.Guild.GetChannel(this.ChannelId.Value) : null;

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
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public string? MessageContent { get; internal set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	/// <summary>
	/// The word or phrase configured in the rule that triggered this.
	/// </summary>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public string? MatchedKeyword { get; internal set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	/// <summary>
	/// The substring in the content which triggered the rule.
	/// </summary>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public string? MatchedContent { get; internal set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	public AutomodActionExecutedEventArgs(IServiceProvider provider) : base(provider) { }
}
