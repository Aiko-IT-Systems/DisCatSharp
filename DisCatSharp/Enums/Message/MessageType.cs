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

namespace DisCatSharp.Enums;

/// <summary>
/// Represents the type of a message.
/// </summary>
public enum MessageType : int
{
	/// <summary>
	/// Indicates a regular message.
	/// </summary>
	Default = 0,

	/// <summary>
	/// Message indicating a recipient was added to a group direct message or a thread channel.
	/// </summary>
	RecipientAdd = 1,

	/// <summary>
	/// Message indicating a recipient was removed from a group direct message or a thread channel.
	/// </summary>
	RecipientRemove = 2,

	/// <summary>
	/// Message indicating a call.
	/// </summary>
	Call = 3,

	/// <summary>
	/// Message indicating a group direct message or thread channel rename.
	/// </summary>
	ChannelNameChange = 4,

	/// <summary>
	/// Message indicating a group direct message channel icon change.
	/// </summary>
	ChannelIconChange = 5,

	/// <summary>
	/// Message indicating a user pinned a message to a channel.
	/// </summary>
	ChannelPinnedMessage = 6,

	/// <summary>
	/// Message indicating a guild member joined. Most frequently seen in newer, smaller guilds.
	/// </summary>
	GuildMemberJoin = 7,

	/// <summary>
	/// Message indicating a member nitro boosted a guild.
	/// </summary>
	UserPremiumGuildSubscription = 8,

	/// <summary>
	/// Message indicating a guild reached tier one of nitro boosts.
	/// </summary>
	TierOneUserPremiumGuildSubscription = 9,

	/// <summary>
	/// Message indicating a guild reached tier two of nitro boosts.
	/// </summary>
	TierTwoUserPremiumGuildSubscription = 10,

	/// <summary>
	/// Message indicating a guild reached tier three of nitro boosts.
	/// </summary>
	TierThreeUserPremiumGuildSubscription = 11,

	/// <summary>
	/// Message indicating a user followed a news channel.
	/// </summary>
	ChannelFollowAdd = 12,

	/// <summary>
	/// Message indicating a user is streaming in a guild.
	/// </summary>
	GuildStream = 13,

	/// <summary>
	/// Message indicating a guild was removed from guild discovery.
	/// </summary>
	GuildDiscoveryDisqualified = 14,

	/// <summary>
	/// Message indicating a guild was re-added to guild discovery.
	/// </summary>
	GuildDiscoveryRequalified = 15,

	/// <summary>
	/// Message indicating that a guild has failed to meet guild discovery requirements for a week.
	/// </summary>
	GuildDiscoveryGracePeriodInitialWarning = 16,

	/// <summary>
	/// Message indicating that a guild has failed to meet guild discovery requirements for 3 weeks.
	/// </summary>
	GuildDiscoveryGracePeriodFinalWarning = 17,

	/// <summary>
	/// Message indicating a thread was created.
	/// </summary>
	ThreadCreated = 18,

	/// <summary>
	/// Message indicating a user replied to another user.
	/// </summary>
	Reply = 19,

	/// <summary>
	/// Message indicating an slash command was invoked.
	/// </summary>
	ChatInputCommand = 20,

	/// <summary>
	/// Message indicating a new was message sent as the first message in threads that are started from an existing message in the parent channel.
	/// </summary>
	ThreadStarterMessage = 21,

	/// <summary>
	/// Message reminding you to invite people to help you build the server.
	/// </summary>
	GuildInviteReminder = 22,

	/// <summary>
	/// Message indicating an context menu command was invoked.
	/// </summary>
	ContextMenuCommand = 23,

	/// <summary>
	/// Message indicating the guilds automod acted.
	/// </summary>
	AutoModerationAction = 24,

	/// <summary>
	/// Message indicating that a member purchased a role subscription.
	/// </summary>
	RoleSubscriptionPurchase = 25,

	/// <summary>
	/// Unknown.
	/// </summary>
	InteractionPremiumUpsell = 26,

	/// <summary>
	/// Message indicating that a stage started.
	/// </summary>
	StageStart = 27,

	/// <summary>
	/// Message indicating that a stage ended.
	/// </summary>
	StageEnd = 28,

	/// <summary>
	/// Message indicating that a user is now a stage speaker.
	/// </summary>
	StageSpeaker = 29,

	/// <summary>
	/// Message indicating that a user in a stage raised there hand (Request to speak).
	/// </summary>
	StageRaiseHand = 30,

	/// <summary>
	/// Message indicating that a stage topic was changed.
	/// </summary>
	StageTopic = 31,

	/// <summary>
	/// Message indicating that a member purchased a application premium subscription.
	/// </summary>
	GuildApplicationPremiumSubscription = 32
}
