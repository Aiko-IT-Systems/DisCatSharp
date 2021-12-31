// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.EventArgs;
using Microsoft.Extensions.Logging;

namespace DisCatSharp
{
    /// <summary>
    /// Represents a discord sharded client.
    /// </summary>
    public sealed partial class DiscordShardedClient
    {
        #region WebSocket

        /// <summary>
        /// Fired whenever a WebSocket error occurs within the client.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketErrorEventArgs> SocketErrored
        {
            add => this._socketErrored.Register(value);
            remove => this._socketErrored.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketErrorEventArgs> _socketErrored;

        /// <summary>
        /// Fired whenever WebSocket connection is established.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketEventArgs> SocketOpened
        {
            add => this._socketOpened.Register(value);
            remove => this._socketOpened.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketEventArgs> _socketOpened;

        /// <summary>
        /// Fired whenever WebSocket connection is terminated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, SocketCloseEventArgs> SocketClosed
        {
            add => this._socketClosed.Register(value);
            remove => this._socketClosed.Unregister(value);
        }
        private AsyncEvent<DiscordClient, SocketCloseEventArgs> _socketClosed;

        /// <summary>
        /// Fired when the client enters ready state.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Ready
        {
            add => this._ready.Register(value);
            remove => this._ready.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ReadyEventArgs> _ready;

        /// <summary>
        /// Fired whenever a session is resumed.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ReadyEventArgs> Resumed
        {
            add => this._resumed.Register(value);
            remove => this._resumed.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ReadyEventArgs> _resumed;

        /// <summary>
        /// Fired on received heartbeat ACK.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, HeartbeatEventArgs> Heartbeated
        {
            add => this._heartbeated.Register(value);
            remove => this._heartbeated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, HeartbeatEventArgs> _heartbeated;

        #endregion

        #region Channel

        /// <summary>
        /// Fired when a new channel is created.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelCreateEventArgs> ChannelCreated
        {
            add => this._channelCreated.Register(value);
            remove => this._channelCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelCreateEventArgs> _channelCreated;

        /// <summary>
        /// Fired when a channel is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelUpdateEventArgs> ChannelUpdated
        {
            add => this._channelUpdated.Register(value);
            remove => this._channelUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelUpdateEventArgs> _channelUpdated;

        /// <summary>
        /// Fired when a channel is deleted
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelDeleteEventArgs> ChannelDeleted
        {
            add => this._channelDeleted.Register(value);
            remove => this._channelDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelDeleteEventArgs> _channelDeleted;

        /// <summary>
        /// Fired when a dm channel is deleted
        /// For this Event you need the <see cref="DiscordIntents.DirectMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, DmChannelDeleteEventArgs> DmChannelDeleted
        {
            add => this._dmChannelDeleted.Register(value);
            remove => this._dmChannelDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, DmChannelDeleteEventArgs> _dmChannelDeleted;

        /// <summary>
        /// Fired whenever a channel's pinned message list is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ChannelPinsUpdateEventArgs> ChannelPinsUpdated
        {
            add => this._channelPinsUpdated.Register(value);
            remove => this._channelPinsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs> _channelPinsUpdated;

        #endregion

        #region Guild

        /// <summary>
        /// Fired when the user joins a new guild.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <remarks>[alias="GuildJoined"][alias="JoinedGuild"]</remarks>
        public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildCreated
        {
            add => this._guildCreated.Register(value);
            remove => this._guildCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildCreated;

        /// <summary>
        /// Fired when a guild is becoming available.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildCreateEventArgs> GuildAvailable
        {
            add => this._guildAvailable.Register(value);
            remove => this._guildAvailable.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildCreateEventArgs> _guildAvailable;

        /// <summary>
        /// Fired when a guild is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildUpdateEventArgs> GuildUpdated
        {
            add => this._guildUpdated.Register(value);
            remove => this._guildUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildUpdateEventArgs> _guildUpdated;

        /// <summary>
        /// Fired when the user leaves or is removed from a guild.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildDeleted
        {
            add => this._guildDeleted.Register(value);
            remove => this._guildDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildDeleted;

        /// <summary>
        /// Fired when a guild becomes unavailable.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDeleteEventArgs> GuildUnavailable
        {
            add => this._guildUnavailable.Register(value);
            remove => this._guildUnavailable.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDeleteEventArgs> _guildUnavailable;

        /// <summary>
        /// Fired when all guilds finish streaming from Discord.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildDownloadCompletedEventArgs> GuildDownloadCompleted
        {
            add => this._guildDownloadCompleted.Register(value);
            remove => this._guildDownloadCompleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs> _guildDownloadCompleted;

        /// <summary>
        /// Fired when a guilds emojis get updated
        /// For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildEmojisUpdateEventArgs> GuildEmojisUpdated
        {
            add => this._guildEmojisUpdated.Register(value);
            remove => this._guildEmojisUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs> _guildEmojisUpdated;

        /// <summary>
        /// Fired when a guilds stickers get updated
        /// For this Event you need the <see cref="DiscordIntents.GuildEmojisAndStickers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildStickersUpdateEventArgs> GuildStickersUpdated
        {
            add => this._guildStickersUpdated.Register(value);
            remove => this._guildStickersUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs> _guildStickersUpdated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildIntegrationsUpdateEventArgs> GuildIntegrationsUpdated
        {
            add => this._guildIntegrationsUpdated.Register(value);
            remove => this._guildIntegrationsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs> _guildIntegrationsUpdated;

        #endregion

        #region Guild Ban

        /// <summary>
        /// Fired when a guild ban gets added
        /// For this Event you need the <see cref="DiscordIntents.GuildBans"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildBanAddEventArgs> GuildBanAdded
        {
            add => this._guildBanAdded.Register(value);
            remove => this._guildBanAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildBanAddEventArgs> _guildBanAdded;

        /// <summary>
        /// Fired when a guild ban gets removed
        /// For this Event you need the <see cref="DiscordIntents.GuildBans"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildBanRemoveEventArgs> GuildBanRemoved
        {
            add => this._guildBanRemoved.Register(value);
            remove => this._guildBanRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildBanRemoveEventArgs> _guildBanRemoved;

        #endregion

        #region Guild Event

        /// <summary>
        /// Fired when a scheduled event is created.
        /// For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildScheduledEventCreateEventArgs> GuildScheduledEventCreated
        {
            add => this._guildScheduledEventCreated.Register(value);
            remove => this._guildScheduledEventCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildScheduledEventCreateEventArgs> _guildScheduledEventCreated;

        /// <summary>
        /// Fired when a scheduled event is updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildScheduledEventUpdateEventArgs> GuildScheduledEventUpdated
        {
            add => this._guildScheduledEventUpdated.Register(value);
            remove => this._guildScheduledEventUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildScheduledEventUpdateEventArgs> _guildScheduledEventUpdated;

        /// <summary>
        /// Fired when a scheduled event is deleted.
        /// For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildScheduledEventDeleteEventArgs> GuildScheduledEventDeleted
        {
            add => this._guildScheduledEventDeleted.Register(value);
            remove => this._guildScheduledEventDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildScheduledEventDeleteEventArgs> _guildScheduledEventDeleted;

        /// <summary>
        /// Fired when a user subscribes to a scheduled event.
        /// For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildScheduledEventUserAddEventArgs> GuildScheduledEventUserAdded
        {
            add => this._guildScheduledEventUserAdded.Register(value);
            remove => this._guildScheduledEventUserAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildScheduledEventUserAddEventArgs> _guildScheduledEventUserAdded;

        /// <summary>
        /// Fired when a user unsubscribes from a scheduled event.
        /// For this Event you need the <see cref="DiscordIntents.GuildScheduledEvents"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildScheduledEventUserRemoveEventArgs> GuildScheduledEventUserRemoved
        {
            add => this._guildScheduledEventUserRemoved.Register(value);
            remove => this._guildScheduledEventUserRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildScheduledEventUserRemoveEventArgs> _guildScheduledEventUserRemoved;

        #endregion

        #region Guild Integration

        /// <summary>
        /// Fired when a guild integration is created.
        /// For this Event you need the <see cref="DiscordIntents.GuildIntegrations"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildIntegrationCreateEventArgs> GuildIntegrationCreated
        {
            add => this._guildIntegrationCreated.Register(value);
            remove => this._guildIntegrationCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildIntegrationCreateEventArgs> _guildIntegrationCreated;

        /// <summary>
        /// Fired when a guild integration is updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildIntegrations"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildIntegrationUpdateEventArgs> GuildIntegrationUpdated
        {
            add => this._guildIntegrationUpdated.Register(value);
            remove => this._guildIntegrationUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildIntegrationUpdateEventArgs> _guildIntegrationUpdated;

        /// <summary>
        /// Fired when a guild integration is deleted.
        /// For this Event you need the <see cref="DiscordIntents.GuildIntegrations"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildIntegrationDeleteEventArgs> GuildIntegrationDeleted
        {
            add => this._guildIntegrationDeleted.Register(value);
            remove => this._guildIntegrationDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildIntegrationDeleteEventArgs> _guildIntegrationDeleted;

        #endregion

        #region Guild Member

        /// <summary>
        /// Fired when a new user joins a guild.
        /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberAddEventArgs> GuildMemberAdded
        {
            add => this._guildMemberAdded.Register(value);
            remove => this._guildMemberAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberAddEventArgs> _guildMemberAdded;

        /// <summary>
        /// Fired when a user is removed from a guild (leave/kick/ban).
        /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberRemoveEventArgs> GuildMemberRemoved
        {
            add => this._guildMemberRemoved.Register(value);
            remove => this._guildMemberRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs> _guildMemberRemoved;

        /// <summary>
        /// Fired when a guild member is updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMemberUpdateEventArgs> GuildMemberUpdated
        {
            add => this._guildMemberUpdated.Register(value);
            remove => this._guildMemberUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs> _guildMemberUpdated;

        /// <summary>
        /// Fired in response to Gateway Request Guild Members.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildMembersChunkEventArgs> GuildMembersChunked
        {
            add => this._guildMembersChunk.Register(value);
            remove => this._guildMembersChunk.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildMembersChunkEventArgs> _guildMembersChunk;

        #endregion

        #region Guild Role

        /// <summary>
        /// Fired when a guild role is created.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleCreateEventArgs> GuildRoleCreated
        {
            add => this._guildRoleCreated.Register(value);
            remove => this._guildRoleCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleCreateEventArgs> _guildRoleCreated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleUpdateEventArgs> GuildRoleUpdated
        {
            add => this._guildRoleUpdated.Register(value);
            remove => this._guildRoleUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs> _guildRoleUpdated;

        /// <summary>
        /// Fired when a guild role is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildRoleDeleteEventArgs> GuildRoleDeleted
        {
            add => this._guildRoleDeleted.Register(value);
            remove => this._guildRoleDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs> _guildRoleDeleted;

        #endregion

        #region Invite

        /// <summary>
        /// Fired when an invite is created.
        /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, InviteCreateEventArgs> InviteCreated
        {
            add => this._inviteCreated.Register(value);
            remove => this._inviteCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, InviteCreateEventArgs> _inviteCreated;

        /// <summary>
        /// Fired when an invite is deleted.
        /// For this Event you need the <see cref="DiscordIntents.GuildInvites"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, InviteDeleteEventArgs> InviteDeleted
        {
            add => this._inviteDeleted.Register(value);
            remove => this._inviteDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, InviteDeleteEventArgs> _inviteDeleted;

        #endregion

        #region Message

        /// <summary>
        /// Fired when a message is created.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageCreateEventArgs> MessageCreated
        {
            add => this._messageCreated.Register(value);
            remove => this._messageCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageCreateEventArgs> _messageCreated;

        /// <summary>
        /// Fired when a message is updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageUpdateEventArgs> MessageUpdated
        {
            add => this._messageUpdated.Register(value);
            remove => this._messageUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageUpdateEventArgs> _messageUpdated;

        /// <summary>
        /// Fired when a message is deleted.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageDeleteEventArgs> MessageDeleted
        {
            add => this._messageDeleted.Register(value);
            remove => this._messageDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageDeleteEventArgs> _messageDeleted;

        /// <summary>
        /// Fired when multiple messages are deleted at once.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessages"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageBulkDeleteEventArgs> MessagesBulkDeleted
        {
            add => this._messageBulkDeleted.Register(value);
            remove => this._messageBulkDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs> _messageBulkDeleted;

        #endregion

        #region Message Reaction

        /// <summary>
        /// Fired when a reaction gets added to a message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> MessageReactionAdded
        {
            add => this._messageReactionAdded.Register(value);
            remove => this._messageReactionAdded.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _messageReactionAdded;

        /// <summary>
        /// Fired when a reaction gets removed from a message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> MessageReactionRemoved
        {
            add => this._messageReactionRemoved.Register(value);
            remove => this._messageReactionRemoved.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _messageReactionRemoved;

        /// <summary>
        /// Fired when all reactions get removed from a message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> MessageReactionsCleared
        {
            add => this._messageReactionsCleared.Register(value);
            remove => this._messageReactionsCleared.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _messageReactionsCleared;

        /// <summary>
        /// Fired when all reactions of a specific reaction are removed from a message.
        /// For this Event you need the <see cref="DiscordIntents.GuildMessageReactions"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, MessageReactionRemoveEmojiEventArgs> MessageReactionRemovedEmoji
        {
            add => this._messageReactionRemovedEmoji.Register(value);
            remove => this._messageReactionRemovedEmoji.Unregister(value);
        }
        private AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs> _messageReactionRemovedEmoji;

        #endregion

        #region Stage Instance

        /// <summary>
        /// Fired when a Stage Instance is created.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, StageInstanceCreateEventArgs> StageInstanceCreated
        {
            add => this._stageInstanceCreated.Register(value);
            remove => this._stageInstanceCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, StageInstanceCreateEventArgs> _stageInstanceCreated;

        /// <summary>
        /// Fired when a Stage Instance is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, StageInstanceUpdateEventArgs> StageInstanceUpdated
        {
            add => this._stageInstanceUpdated.Register(value);
            remove => this._stageInstanceUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs> _stageInstanceUpdated;

        /// <summary>
        /// Fired when a Stage Instance is deleted.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, StageInstanceDeleteEventArgs> StageInstanceDeleted
        {
            add => this._stageInstanceDeleted.Register(value);
            remove => this._stageInstanceDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs> _stageInstanceDeleted;

        #endregion

        #region Thread

        /// <summary>
        /// Fired when a thread is created.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadCreateEventArgs> ThreadCreated
        {
            add => this._threadCreated.Register(value);
            remove => this._threadCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadCreateEventArgs> _threadCreated;

        /// <summary>
        /// Fired when a thread is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadUpdateEventArgs> ThreadUpdated
        {
            add => this._threadUpdated.Register(value);
            remove => this._threadUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadUpdateEventArgs> _threadUpdated;

        /// <summary>
        /// Fired when a thread is deleted.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadDeleteEventArgs> ThreadDeleted
        {
            add => this._threadDeleted.Register(value);
            remove => this._threadDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadDeleteEventArgs> _threadDeleted;

        /// <summary>
        /// Fired when a thread member is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadListSyncEventArgs> ThreadListSynced
        {
            add => this._threadListSynced.Register(value);
            remove => this._threadListSynced.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadListSyncEventArgs> _threadListSynced;

        /// <summary>
        /// Fired when a thread member is updated.
        /// For this Event you need the <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadMemberUpdateEventArgs> ThreadMemberUpdated
        {
            add => this._threadMemberUpdated.Register(value);
            remove => this._threadMemberUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs> _threadMemberUpdated;

        /// <summary>
        /// Fired when the thread members are updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.Guilds"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ThreadMembersUpdateEventArgs> ThreadMembersUpdated
        {
            add => this._threadMembersUpdated.Register(value);
            remove => this._threadMembersUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs> _threadMembersUpdated;

        #endregion

        #region Activities

        /// <summary>
        /// Fired when a embedded activity has been updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, EmbeddedActivityUpdateEventArgs> EmbeddedActivityUpdated
        {
            add => this._embeddedActivityUpdated.Register(value);
            remove => this._embeddedActivityUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, EmbeddedActivityUpdateEventArgs> _embeddedActivityUpdated;

        #endregion

        #region User/Presence Update

        /// <summary>
        /// Fired when a presence has been updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, PresenceUpdateEventArgs> PresenceUpdated
        {
            add => this._presenceUpdated.Register(value);
            remove => this._presenceUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, PresenceUpdateEventArgs> _presenceUpdated;


        /// <summary>
        /// Fired when the current user updates their settings.
        /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, UserSettingsUpdateEventArgs> UserSettingsUpdated
        {
            add => this._userSettingsUpdated.Register(value);
            remove => this._userSettingsUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs> _userSettingsUpdated;

        /// <summary>
        /// Fired when properties about the current user change.
        /// For this Event you need the <see cref="DiscordIntents.GuildPresences"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        /// <remarks>
        /// NB: This event only applies for changes to the <b>current user</b>, the client that is connected to Discord.
        /// </remarks>
        public event AsyncEventHandler<DiscordClient, UserUpdateEventArgs> UserUpdated
        {
            add => this._userUpdated.Register(value);
            remove => this._userUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UserUpdateEventArgs> _userUpdated;

        #endregion

        #region Voice

        /// <summary>
        /// Fired when someone joins/leaves/moves voice channels.
        /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, VoiceStateUpdateEventArgs> VoiceStateUpdated
        {
            add => this._voiceStateUpdated.Register(value);
            remove => this._voiceStateUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs> _voiceStateUpdated;

        /// <summary>
        /// Fired when a guild's voice server is updated.
        /// For this Event you need the <see cref="DiscordIntents.GuildVoiceStates"/> intent specified in <seealso cref="DiscordConfiguration.Intents"/>
        /// </summary>
        public event AsyncEventHandler<DiscordClient, VoiceServerUpdateEventArgs> VoiceServerUpdated
        {
            add => this._voiceServerUpdated.Register(value);
            remove => this._voiceServerUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs> _voiceServerUpdated;

        #endregion

        #region Application

        /// <summary>
        /// Fired when a new application command is registered.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandCreated
        {
            add => this._applicationCommandCreated.Register(value);
            remove => this._applicationCommandCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandCreated;

        /// <summary>
        /// Fired when an application command is updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandUpdated
        {
            add => this._applicationCommandUpdated.Register(value);
            remove => this._applicationCommandUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandUpdated;

        /// <summary>
        /// Fired when an application command is deleted.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ApplicationCommandEventArgs> ApplicationCommandDeleted
        {
            add => this._applicationCommandDeleted.Register(value);
            remove => this._applicationCommandDeleted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ApplicationCommandEventArgs> _applicationCommandDeleted;

        /// <summary>
        /// Fired when a new application command is registered.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, GuildApplicationCommandCountEventArgs> GuildApplicationCommandCountUpdated
        {
            add => this._guildApplicationCommandCountUpdated.Register(value);
            remove => this._guildApplicationCommandCountUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, GuildApplicationCommandCountEventArgs> _guildApplicationCommandCountUpdated;

        /// <summary>
        /// Fired when a user uses a context menu.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ContextMenuInteractionCreateEventArgs> ContextMenuInteractionCreated
        {
            add => this._contextMenuInteractionCreated.Register(value);
            remove => this._contextMenuInteractionCreated.Unregister(value);
        }

        private AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs> _contextMenuInteractionCreated;

        /// <summary>
        /// Fired when application command permissions gets updated.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs> ApplicationCommandPermissionsUpdated
        {
            add => this._applicationCommandPermissionsUpdated.Register(value);
            remove => this._applicationCommandPermissionsUpdated.Unregister(value);
        }

        private AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs> _applicationCommandPermissionsUpdated;


        #endregion

        #region Misc

        /// <summary>
        /// Fired when an interaction is invoked.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, InteractionCreateEventArgs> InteractionCreated
        {
            add => this._interactionCreated.Register(value);
            remove => this._interactionCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, InteractionCreateEventArgs> _interactionCreated;

        /// <summary>
        /// Fired when a component is invoked.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> ComponentInteractionCreated
        {
            add => this._componentInteractionCreated.Register(value);
            remove => this._componentInteractionCreated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs> _componentInteractionCreated;

        /// <summary>
        /// Fired when a user starts typing in a channel.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, TypingStartEventArgs> TypingStarted
        {
            add => this._typingStarted.Register(value);
            remove => this._typingStarted.Unregister(value);
        }
        private AsyncEvent<DiscordClient, TypingStartEventArgs> _typingStarted;

        /// <summary>
        /// Fired when an unknown event gets received.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, UnknownEventArgs> UnknownEvent
        {
            add => this._unknownEvent.Register(value);
            remove => this._unknownEvent.Unregister(value);
        }
        private AsyncEvent<DiscordClient, UnknownEventArgs> _unknownEvent;

        /// <summary>
        /// Fired whenever webhooks update.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, WebhooksUpdateEventArgs> WebhooksUpdated
        {
            add => this._webhooksUpdated.Register(value);
            remove => this._webhooksUpdated.Unregister(value);
        }
        private AsyncEvent<DiscordClient, WebhooksUpdateEventArgs> _webhooksUpdated;

        /// <summary>
        /// Fired whenever an error occurs within an event handler.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ClientErrorEventArgs> ClientErrored
        {
            add => this._clientErrored.Register(value);
            remove => this._clientErrored.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ClientErrorEventArgs> _clientErrored;

        #endregion

        #region Error Handling

        /// <summary>
        /// Events the error handler.
        /// </summary>
        /// <param name="AsyncEvent">The async event.</param>
        /// <param name="Ex">The ex.</param>
        /// <param name="Handler">The handler.</param>
        /// <param name="Sender">The sender.</param>
        /// <param name="EventArgs">The event args.</param>
        internal void EventErrorHandler<TArgs>(AsyncEvent<DiscordClient, TArgs> AsyncEvent, Exception Ex, AsyncEventHandler<DiscordClient, TArgs> Handler, DiscordClient Sender, TArgs EventArgs)
            where TArgs : AsyncEventArgs
        {
            if (Ex is AsyncEventTimeoutException)
            {
                this.Logger.LogWarning(LoggerEvents.EventHandlerException, $"An event handler for {AsyncEvent.Name} took too long to execute. Defined as \"{Handler.Method.ToString().Replace(Handler.Method.ReturnType.ToString(), "").TrimStart()}\" located in \"{Handler.Method.DeclaringType}\".");
                return;
            }

            this.Logger.LogError(LoggerEvents.EventHandlerException, Ex, "Event handler exception for event {0} thrown from {1} (defined in {2})", AsyncEvent.Name, Handler.Method, Handler.Method.DeclaringType);
            this._clientErrored.InvokeAsync(Sender, new ClientErrorEventArgs(this.ShardClients[0].ServiceProvider) { EventName = AsyncEvent.Name, Exception = Ex }).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Fired on heartbeat attempt cancellation due to too many failed heartbeats.
        /// </summary>
        public event AsyncEventHandler<DiscordClient, ZombiedEventArgs> Zombied
        {
            add => this._zombied.Register(value);
            remove => this._zombied.Unregister(value);
        }
        private AsyncEvent<DiscordClient, ZombiedEventArgs> _zombied;

        /// <summary>
        /// Fired when a gateway
        /// </summary>
        public event AsyncEventHandler<DiscordClient, PayloadReceivedEventArgs> PayloadReceived
        {
            add => this._payloadReceived.Register(value);
            remove => this._payloadReceived.Unregister(value);
        }
        private AsyncEvent<DiscordClient, PayloadReceivedEventArgs> _payloadReceived;

        /// <summary>
        /// Goofs the.
        /// </summary>
        /// <param name="AsyncEvent">The async event.</param>
        /// <param name="Ex">The ex.</param>
        /// <param name="Handler">The handler.</param>
        /// <param name="Sender">The sender.</param>
        /// <param name="EventArgs">The event args.</param>
        private void Goof<TArgs>(AsyncEvent<DiscordClient, TArgs> AsyncEvent, Exception Ex, AsyncEventHandler<DiscordClient, TArgs> Handler, DiscordClient Sender, TArgs EventArgs)
            where TArgs : AsyncEventArgs => this.Logger.LogCritical(LoggerEvents.EventHandlerException, Ex, "Exception event handler {0} (defined in {1}) threw an exception", Handler.Method, Handler.Method.DeclaringType);

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Client_S the zombied.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_Zombied(DiscordClient Client, ZombiedEventArgs E)
            => this._zombied.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the embedded activity updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_EmbeddedActivityUpdated(DiscordClient Client, EmbeddedActivityUpdateEventArgs E)
            => this._embeddedActivityUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Payload_S the received.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_PayloadReceived(DiscordClient Client, PayloadReceivedEventArgs E)
            => this._payloadReceived.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the client error.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ClientError(DiscordClient Client, ClientErrorEventArgs E)
            => this._clientErrored.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the socket error.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_SocketError(DiscordClient Client, SocketErrorEventArgs E)
            => this._socketErrored.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the socket opened.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_SocketOpened(DiscordClient Client, SocketEventArgs E)
            => this._socketOpened.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the socket closed.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_SocketClosed(DiscordClient Client, SocketCloseEventArgs E)
            => this._socketClosed.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the ready.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_Ready(DiscordClient Client, ReadyEventArgs E)
            => this._ready.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the resumed.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_Resumed(DiscordClient Client, ReadyEventArgs E)
            => this._resumed.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the channel created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ChannelCreated(DiscordClient Client, ChannelCreateEventArgs E)
            => this._channelCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the channel updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ChannelUpdated(DiscordClient Client, ChannelUpdateEventArgs E)
            => this._channelUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the channel deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ChannelDeleted(DiscordClient Client, ChannelDeleteEventArgs E)
            => this._channelDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the d m channel deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_DMChannelDeleted(DiscordClient Client, DmChannelDeleteEventArgs E)
            => this._dmChannelDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the channel pins updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ChannelPinsUpdated(DiscordClient Client, ChannelPinsUpdateEventArgs E)
            => this._channelPinsUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildCreated(DiscordClient Client, GuildCreateEventArgs E)
            => this._guildCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild available.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildAvailable(DiscordClient Client, GuildCreateEventArgs E)
            => this._guildAvailable.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildUpdated(DiscordClient Client, GuildUpdateEventArgs E)
            => this._guildUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildDeleted(DiscordClient Client, GuildDeleteEventArgs E)
            => this._guildDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild unavailable.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildUnavailable(DiscordClient Client, GuildDeleteEventArgs E)
            => this._guildUnavailable.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild download completed.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildDownloadCompleted(DiscordClient Client, GuildDownloadCompletedEventArgs E)
            => this._guildDownloadCompleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageCreated(DiscordClient Client, MessageCreateEventArgs E)
            => this._messageCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the invite created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_InviteCreated(DiscordClient Client, InviteCreateEventArgs E)
            => this._inviteCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the invite deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_InviteDeleted(DiscordClient Client, InviteDeleteEventArgs E)
            => this._inviteDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the presence update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_PresenceUpdate(DiscordClient Client, PresenceUpdateEventArgs E)
            => this._presenceUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild ban add.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildBanAdd(DiscordClient Client, GuildBanAddEventArgs E)
            => this._guildBanAdded.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild ban remove.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildBanRemove(DiscordClient Client, GuildBanRemoveEventArgs E)
            => this._guildBanRemoved.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild emojis update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildEmojisUpdate(DiscordClient Client, GuildEmojisUpdateEventArgs E)
            => this._guildEmojisUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild stickers update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildStickersUpdate(DiscordClient Client, GuildStickersUpdateEventArgs E)
            => this._guildStickersUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild integrations update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildIntegrationsUpdate(DiscordClient Client, GuildIntegrationsUpdateEventArgs E)
            => this._guildIntegrationsUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild member add.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildMemberAdd(DiscordClient Client, GuildMemberAddEventArgs E)
            => this._guildMemberAdded.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild member remove.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildMemberRemove(DiscordClient Client, GuildMemberRemoveEventArgs E)
            => this._guildMemberRemoved.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild member update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildMemberUpdate(DiscordClient Client, GuildMemberUpdateEventArgs E)
            => this._guildMemberUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild role create.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildRoleCreate(DiscordClient Client, GuildRoleCreateEventArgs E)
            => this._guildRoleCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild role update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildRoleUpdate(DiscordClient Client, GuildRoleUpdateEventArgs E)
            => this._guildRoleUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild role delete.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildRoleDelete(DiscordClient Client, GuildRoleDeleteEventArgs E)
            => this._guildRoleDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageUpdate(DiscordClient Client, MessageUpdateEventArgs E)
            => this._messageUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message delete.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageDelete(DiscordClient Client, MessageDeleteEventArgs E)
            => this._messageDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message bulk delete.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageBulkDelete(DiscordClient Client, MessageBulkDeleteEventArgs E)
            => this._messageBulkDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the typing start.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_TypingStart(DiscordClient Client, TypingStartEventArgs E)
            => this._typingStarted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the user settings update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_UserSettingsUpdate(DiscordClient Client, UserSettingsUpdateEventArgs E)
            => this._userSettingsUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the user update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_UserUpdate(DiscordClient Client, UserUpdateEventArgs E)
            => this._userUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the voice state update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_VoiceStateUpdate(DiscordClient Client, VoiceStateUpdateEventArgs E)
            => this._voiceStateUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the voice server update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_VoiceServerUpdate(DiscordClient Client, VoiceServerUpdateEventArgs E)
            => this._voiceServerUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild members chunk.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildMembersChunk(DiscordClient Client, GuildMembersChunkEventArgs E)
            => this._guildMembersChunk.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the unknown event.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_UnknownEvent(DiscordClient Client, UnknownEventArgs E)
            => this._unknownEvent.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message reaction add.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageReactionAdd(DiscordClient Client, MessageReactionAddEventArgs E)
            => this._messageReactionAdded.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message reaction remove.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageReactionRemove(DiscordClient Client, MessageReactionRemoveEventArgs E)
            => this._messageReactionRemoved.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message reaction remove all.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageReactionRemoveAll(DiscordClient Client, MessageReactionsClearEventArgs E)
            => this._messageReactionsCleared.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the message reaction removed emoji.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_MessageReactionRemovedEmoji(DiscordClient Client, MessageReactionRemoveEmojiEventArgs E)
            => this._messageReactionRemovedEmoji.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the interaction create.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_InteractionCreate(DiscordClient Client, InteractionCreateEventArgs E)
            => this._interactionCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the component interaction create.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ComponentInteractionCreate(DiscordClient Client, ComponentInteractionCreateEventArgs E)
            => this._componentInteractionCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the context menu interaction create.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ContextMenuInteractionCreate(DiscordClient Client, ContextMenuInteractionCreateEventArgs E)
            => this._contextMenuInteractionCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the webhooks update.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_WebhooksUpdate(DiscordClient Client, WebhooksUpdateEventArgs E)
            => this._webhooksUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the heart beated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_HeartBeated(DiscordClient Client, HeartbeatEventArgs E)
            => this._heartbeated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the application command created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ApplicationCommandCreated(DiscordClient Client, ApplicationCommandEventArgs E)
            => this._applicationCommandCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the application command updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        private Task Client_ApplicationCommandUpdated(DiscordClient Client, ApplicationCommandEventArgs E)
            => this._applicationCommandUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the application command deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        private Task Client_ApplicationCommandDeleted(DiscordClient Client, ApplicationCommandEventArgs E)
            => this._applicationCommandDeleted.InvokeAsync(Client, E);


        /// <summary>
        /// Client_S the guild application command count updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        private Task Client_GuildApplicationCommandCountUpdated(DiscordClient Client, GuildApplicationCommandCountEventArgs E)
            => this._guildApplicationCommandCountUpdated.InvokeAsync(Client, E);


        /// <summary>
        /// Client_S the application command permissions updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        private Task Client_ApplicationCommandPermissionsUpdated(DiscordClient Client, ApplicationCommandPermissionsUpdateEventArgs E)
            => this._applicationCommandPermissionsUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild integration created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildIntegrationCreated(DiscordClient Client, GuildIntegrationCreateEventArgs E)
            => this._guildIntegrationCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild integration updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildIntegrationUpdated(DiscordClient Client, GuildIntegrationUpdateEventArgs E)
            => this._guildIntegrationUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the guild integration deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildIntegrationDeleted(DiscordClient Client, GuildIntegrationDeleteEventArgs E)
            => this._guildIntegrationDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the stage instance created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_StageInstanceCreated(DiscordClient Client, StageInstanceCreateEventArgs E)
            => this._stageInstanceCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the stage instance updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_StageInstanceUpdated(DiscordClient Client, StageInstanceUpdateEventArgs E)
            => this._stageInstanceUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the stage instance deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_StageInstanceDeleted(DiscordClient Client, StageInstanceDeleteEventArgs E)
            => this._stageInstanceDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadCreated(DiscordClient Client, ThreadCreateEventArgs E)
            => this._threadCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadUpdated(DiscordClient Client, ThreadUpdateEventArgs E)
            => this._threadUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadDeleted(DiscordClient Client, ThreadDeleteEventArgs E)
            => this._threadDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread list synced.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadListSynced(DiscordClient Client, ThreadListSyncEventArgs E)
            => this._threadListSynced.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread member updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadMemberUpdated(DiscordClient Client, ThreadMemberUpdateEventArgs E)
            => this._threadMemberUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Client_S the thread members updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_ThreadMembersUpdated(DiscordClient Client, ThreadMembersUpdateEventArgs E)
            => this._threadMembersUpdated.InvokeAsync(Client, E);


        /// <summary>
        /// Handles the scheduled event created.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildScheduledEventCreated(DiscordClient Client, GuildScheduledEventCreateEventArgs E)
            => this._guildScheduledEventCreated.InvokeAsync(Client, E);

        /// <summary>
        /// Handles the scheduled event updated.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildScheduledEventUpdated(DiscordClient Client, GuildScheduledEventUpdateEventArgs E)
            => this._guildScheduledEventUpdated.InvokeAsync(Client, E);

        /// <summary>
        /// Handles the scheduled event deleted.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildScheduledEventDeleted(DiscordClient Client, GuildScheduledEventDeleteEventArgs E)
            => this._guildScheduledEventDeleted.InvokeAsync(Client, E);

        /// <summary>
        /// Handles the scheduled event user added.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildScheduledEventUserAdded(DiscordClient Client, GuildScheduledEventUserAddEventArgs E)
        => this._guildScheduledEventUserAdded.InvokeAsync(Client, E);

        /// <summary>
        /// Handles the scheduled event user removed.
        /// </summary>
        /// <param name="Client">The client.</param>
        /// <param name="E">The events.</param>
        /// <returns>A Task.</returns>
        private Task Client_GuildScheduledEventUserRemoved(DiscordClient Client, GuildScheduledEventUserRemoveEventArgs E)
        => this._guildScheduledEventUserRemoved.InvokeAsync(Client, E);

        #endregion
    }
}
