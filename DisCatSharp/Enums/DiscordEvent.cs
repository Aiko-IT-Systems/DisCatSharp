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

#nullable enable

using System;

namespace DisCatSharp;

/// <summary>
/// Methods marked with this attribute will be registered as event handling methods
/// if the associated type / an associated instance is being registered.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventAttribute : Attribute
{
	internal readonly string? EventName;

	public EventAttribute() { }

	/// <param name="evtn"><para>The name of the event.</para>
	/// <para>The attributed method's name will be used if null.</para></param>
	public EventAttribute(DiscordEvent evtn)
	{
		this.EventName = evtn.ToString();
	}
}

/// <summary>
/// Classes marked with this attribute will be considered for event handler registration from an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EventHandlerAttribute : Attribute { }

/// <summary>
/// All events available in <see cref="DiscordClient"/> for use with <see cref="EventAttribute"/>.
/// </summary>
public enum DiscordEvent
{
	ApplicationCommandCreated,
	ApplicationCommandDeleted,
	ApplicationCommandPermissionsUpdated,
	ApplicationCommandUpdated,
	ChannelCreated,
	ChannelDeleted,
	ChannelPinsUpdated,
	ChannelUpdated,
	ClientErrored,
	ComponentInteractionCreated,
	ContextMenuInteractionCreated,
	DmChannelDeleted,
	EmbeddedActivityUpdated,
	GuildApplicationCommandCountUpdated,
	GuildAvailable,
	GuildBanAdded,
	GuildBanRemoved,
	GuildCreated,
	GuildDeleted,
	GuildDownloadCompleted,
	GuildEmojisUpdated,
	GuildIntegrationCreated,
	GuildIntegrationDeleted,
	GuildIntegrationsUpdated,
	GuildIntegrationUpdated,
	GuildMemberAdded,
	GuildMemberRemoved,
	GuildMembersChunked,
	GuildMemberTimeoutAdded,
	GuildMemberTimeoutChanged,
	GuildMemberTimeoutRemoved,
	GuildMemberUpdated,
	GuildRoleCreated,
	GuildRoleDeleted,
	GuildRoleUpdated,
	GuildScheduledEventCreated,
	GuildScheduledEventDeleted,
	GuildScheduledEventUpdated,
	GuildScheduledEventUserAdded,
	GuildScheduledEventUserRemoved,
	GuildStickersUpdated,
	GuildUnavailable,
	GuildUpdated,
	Heartbeated,
	InteractionCreated,
	InviteCreated,
	InviteDeleted,
	MessageAcknowledged,
	MessageCreated,
	MessageDeleted,
	MessageReactionAdded,
	MessageReactionRemoved,
	MessageReactionRemovedEmoji,
	MessageReactionsCleared,
	MessagesBulkDeleted,
	MessageUpdated,
	PayloadReceived,
	PresenceUpdated,
	RateLimitHit,
	Ready,
	Resumed,
	SocketClosed,
	SocketErrored,
	SocketOpened,
	StageInstanceCreated,
	StageInstanceDeleted,
	StageInstanceUpdated,
	ThreadCreated,
	ThreadDeleted,
	ThreadListSynced,
	ThreadMembersUpdated,
	ThreadMemberUpdated,
	ThreadUpdated,
	TypingStarted,
	UnknownEvent,
	UserSettingsUpdated,
	UserUpdated,
	VoiceServerUpdated,
	VoiceStateUpdated,
	WebhooksUpdated,
	Zombied,
	AutomodRuleCreated,
	AutomodRuleUpdated,
	AutomodRuleDeleted,
	AutomodActionExecuted
}
