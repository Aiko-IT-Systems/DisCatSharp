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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DisCatSharp
{
    /// <summary>
    /// A Discord API wrapper.
    /// </summary>
    public sealed partial class DiscordClient : BaseDiscordClient
    {
        #region Internal Fields/Properties

        internal bool _isShard = false;
        /// <summary>
        /// Gets the message cache.
        /// </summary>
        internal RingBuffer<DiscordMessage> MessageCache { get; }

        private List<BaseExtension> _extensions = new();
        private StatusUpdate _status = null;

        /// <summary>
        /// Gets the connection lock.
        /// </summary>
        private ManualResetEventSlim ConnectionLock { get; } = new ManualResetEventSlim(true);

        #endregion

        #region Public Fields/Properties
        /// <summary>
        /// Gets the gateway protocol version.
        /// </summary>
        public int GatewayVersion { get; internal set; }

        /// <summary>
        /// Gets the gateway session information for this client.
        /// </summary>
        public GatewayInfo GatewayInfo { get; internal set; }

        /// <summary>
        /// Gets the gateway URL.
        /// </summary>
        public Uri GatewayUri { get; internal set; }

        /// <summary>
        /// Gets the total number of shards the bot is connected to.
        /// </summary>
        public int ShardCount => this.GatewayInfo != null
            ? this.GatewayInfo.ShardCount
            : this.Configuration.ShardCount;

        /// <summary>
        /// Gets the currently connected shard ID.
        /// </summary>
        public int ShardId
            => this.Configuration.ShardId;

        /// <summary>
        /// Gets the intents configured for this client.
        /// </summary>
        public DiscordIntents Intents
            => this.Configuration.Intents;

        /// <summary>
        /// Gets a dictionary of guilds that this client is in. The dictionary's key is the guild ID. Note that the
        /// guild objects in this dictionary will not be filled in if the specific guilds aren't available (the
        /// <see cref="GuildAvailable"/> or <see cref="GuildDownloadCompleted"/> events haven't been fired yet)
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }
        internal ConcurrentDictionary<ulong, DiscordGuild> _guilds = new();

        /// <summary>
        /// Gets the WS latency for this client.
        /// </summary>
        public int Ping
            => Volatile.Read(ref this._ping);

        private int _ping;

        /// <summary>
        /// Gets the collection of presences held by this client.
        /// </summary>
        public IReadOnlyDictionary<ulong, DiscordPresence> Presences
            => this._presencesLazy.Value;

        internal Dictionary<ulong, DiscordPresence> _presences = new();
        private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;

        /// <summary>
        /// Gets the collection of presences held by this client.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordActivity> EmbeddedActivities
            => this._embeddedActivitiesLazy.Value;

        internal Dictionary<string, DiscordActivity> _embeddedActivities = new();
        private Lazy<IReadOnlyDictionary<string, DiscordActivity>> _embeddedActivitiesLazy;
        #endregion

        #region Constructor/Internal Setup

        /// <summary>
        /// Initializes a new instance of <see cref="DiscordClient"/>.
        /// </summary>
        /// <param name="Config">Specifies configuration parameters.</param>
        public DiscordClient(DiscordConfiguration Config)
            : base(Config)
        {
            if (this.Configuration.MessageCacheSize > 0)
            {
                var intents = this.Configuration.Intents;
                this.MessageCache = intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages)
                        ? new RingBuffer<DiscordMessage>(this.Configuration.MessageCacheSize)
                        : null;
            }

            this.InternalSetup();

            this.Guilds = new ReadOnlyConcurrentDictionary<ulong, DiscordGuild>(this._guilds);
        }

        /// <summary>
        /// Internal setup of the Client.
        /// </summary>
        internal void InternalSetup()
        {
            this._clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", EventExecutionLimit, this.Goof);
            this._socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", EventExecutionLimit, this.Goof);
            this._socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", EventExecutionLimit, this.EventErrorHandler);
            this._socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", EventExecutionLimit, this.EventErrorHandler);
            this._ready = new AsyncEvent<DiscordClient, ReadyEventArgs>("READY", EventExecutionLimit, this.EventErrorHandler);
            this._resumed = new AsyncEvent<DiscordClient, ReadyEventArgs>("RESUMED", EventExecutionLimit, this.EventErrorHandler);
            this._channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", EventExecutionLimit, this.EventErrorHandler);
            this._guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", EventExecutionLimit, this.EventErrorHandler);
            this._guildDownloadCompletedEv = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", EventExecutionLimit, this.EventErrorHandler);
            this._inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADD", EventExecutionLimit, this.EventErrorHandler);
            this._guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", EventExecutionLimit, this.EventErrorHandler);
            this._guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildStickersUpdated = new AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs>("GUILD_STICKER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADD", EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._messageAcknowledged = new AsyncEvent<DiscordClient, MessageAcknowledgeEventArgs>("MESSAGE_ACKNOWLEDGED", EventExecutionLimit, this.EventErrorHandler);
            this._messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._messagesBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._interactionCreated = new AsyncEvent<DiscordClient, InteractionCreateEventArgs>("INTERACTION_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._componentInteractionCreated = new AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs>("COMPONENT_INTERACTED", EventExecutionLimit, this.EventErrorHandler);
            this._contextMenuInteractionCreated = new AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs>("CONTEXT_MENU_INTERACTED", EventExecutionLimit, this.EventErrorHandler);
            this._typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", EventExecutionLimit, this.EventErrorHandler);
            this._userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildMembersChunked = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", EventExecutionLimit, this.EventErrorHandler);
            this._unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", EventExecutionLimit, this.EventErrorHandler);
            this._webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandCreated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandUpdated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandDeleted = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._guildApplicationCommandCountUpdated = new AsyncEvent<DiscordClient, GuildApplicationCommandCountEventArgs>("GUILD_APPLICATION_COMMAND_COUNTS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandPermissionsUpdated = new AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs>("APPLICATION_COMMAND_PERMISSIONS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationCreated = new AsyncEvent<DiscordClient, GuildIntegrationCreateEventArgs>("INTEGRATION_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationUpdated = new AsyncEvent<DiscordClient, GuildIntegrationUpdateEventArgs>("INTEGRATION_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationDeleted = new AsyncEvent<DiscordClient, GuildIntegrationDeleteEventArgs>("INTEGRATION_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceCreated = new AsyncEvent<DiscordClient, StageInstanceCreateEventArgs>("STAGE_INSTANCE_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceUpdated = new AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs>("STAGE_INSTANCE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceDeleted = new AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs>("STAGE_INSTANCE_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._threadCreated = new AsyncEvent<DiscordClient, ThreadCreateEventArgs>("THREAD_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._threadUpdated = new AsyncEvent<DiscordClient, ThreadUpdateEventArgs>("THREAD_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._threadDeleted = new AsyncEvent<DiscordClient, ThreadDeleteEventArgs>("THREAD_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._threadListSynced = new AsyncEvent<DiscordClient, ThreadListSyncEventArgs>("THREAD_LIST_SYNCED", EventExecutionLimit, this.EventErrorHandler);
            this._threadMemberUpdated = new AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs>("THREAD_MEMBER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._threadMembersUpdated = new AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs>("THREAD_MEMBERS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._zombied = new AsyncEvent<DiscordClient, ZombiedEventArgs>("ZOMBIED", EventExecutionLimit, this.EventErrorHandler);
            this._payloadReceived = new AsyncEvent<DiscordClient, PayloadReceivedEventArgs>("PAYLOAD_RECEIVED", EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventCreated = new AsyncEvent<DiscordClient, GuildScheduledEventCreateEventArgs>("GUILD_SCHEDULED_EVENT_CREATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUpdated = new AsyncEvent<DiscordClient, GuildScheduledEventUpdateEventArgs>("GUILD_SCHEDULED_EVENT_UPDATED", EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventDeleted = new AsyncEvent<DiscordClient, GuildScheduledEventDeleteEventArgs>("GUILD_SCHEDULED_EVENT_DELETED", EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUserAdded = new AsyncEvent<DiscordClient, GuildScheduledEventUserAddEventArgs>("GUILD_SCHEDULED_EVENT_USER_ADDED", EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUserRemoved = new AsyncEvent<DiscordClient, GuildScheduledEventUserRemoveEventArgs>("GUILD_SCHEDULED_EVENT_USER_REMOVED", EventExecutionLimit, this.EventErrorHandler);
            this._embeddedActivityUpdated = new AsyncEvent<DiscordClient, EmbeddedActivityUpdateEventArgs>("EMBEDDED_ACTIVITY_UPDATED", EventExecutionLimit, this.EventErrorHandler);

            this._guilds.Clear();

            this._presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this._presences));
            this._embeddedActivitiesLazy = new Lazy<IReadOnlyDictionary<string, DiscordActivity>>(() => new ReadOnlyDictionary<string, DiscordActivity>(this._embeddedActivities));
        }

        #endregion

        #region Client Extension Methods

        /// <summary>
        /// Registers an extension with this client.
        /// </summary>
        /// <param name="Ext">Extension to register.</param>
        public void AddExtension(BaseExtension Ext)
        {
            Ext.Setup(this);
            this._extensions.Add(Ext);
        }

        /// <summary>
        /// Retrieves a previously-registered extension from this client.
        /// </summary>
        /// <typeparam name="T">Type of extension to retrieve.</typeparam>
        /// <returns>The requested extension.</returns>
        public T GetExtension<T>() where T : BaseExtension
            => this._extensions.FirstOrDefault(X => X.GetType() == typeof(T)) as T;

        #endregion

        #region Public Connection Methods

        /// <summary>
        /// Connects to the gateway.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task ConnectAsync(DiscordActivity Activity = null, UserStatus? Status = null, DateTimeOffset? Idlesince = null)
        {
            // Check if connection lock is already set, and set it if it isn't
            if (!this.ConnectionLock.Wait(0))
                throw new InvalidOperationException("This client is already connected.");
            this.ConnectionLock.Set();

            var w = 7500;
            var i = 5;
            var s = false;
            Exception cex = null;

            if (Activity == null && Status == null && Idlesince == null)
                this._status = null;
            else
            {
                var sinceUnix = Idlesince != null ? (long?)Utilities.GetUnixTime(Idlesince.Value) : null;
                this._status = new StatusUpdate()
                {
                    Activity = new TransportActivity(Activity),
                    Status = Status ?? UserStatus.Online,
                    IdleSince = sinceUnix,
                    IsAfk = Idlesince != null,
                    _activity = Activity
                };
            }

            if (!this._isShard)
            {
                if (this.Configuration.TokenType != TokenType.Bot)
                    this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
                this.Logger.LogInformation(LoggerEvents.Startup, "Lib {0}, version {1}", this.BotLibrary, this.VersionString);
            }

            while (i-- > 0 || this.Configuration.ReconnectIndefinitely)
            {
                try
                {
                    await this.InternalConnectAsync().ConfigureAwait(false);
                    s = true;
                    break;
                }
                catch (UnauthorizedException e)
                {
                    FailConnection(this.ConnectionLock);
                    throw new Exception("Authentication failed. Check your token and try again.", e);
                }
                catch (PlatformNotSupportedException)
                {
                    FailConnection(this.ConnectionLock);
                    throw;
                }
                catch (NotImplementedException)
                {
                    FailConnection(this.ConnectionLock);
                    throw;
                }
                catch (Exception ex)
                {
                    FailConnection(null);

                    cex = ex;
                    if (i <= 0 && !this.Configuration.ReconnectIndefinitely) break;

                    this.Logger.LogError(LoggerEvents.ConnectionFailure, ex, "Connection attempt failed, retrying in {0}s", w / 1000);
                    await Task.Delay(w).ConfigureAwait(false);

                    if (i > 0)
                        w *= 2;
                }
            }

            if (!s && cex != null)
            {
                this.ConnectionLock.Set();
                throw new Exception("Could not connect to Discord.", cex);
            }

            // non-closure, hence args
            static void FailConnection(ManualResetEventSlim Cl) =>
                // unlock this (if applicable) so we can let others attempt to connect
                Cl?.Set();
        }

        /// <summary>
        /// Reconnects to the gateway.
        /// </summary>
        /// <param name="StartNewSession">If true, start new session.</param>
        public Task Reconnect(bool StartNewSession = false)
            => this.InternalReconnect(StartNewSession, Code: StartNewSession ? 1000 : 4002);

        /// <summary>
        /// Disconnects from the gateway.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            this.Configuration.AutoReconnect = false;
            if (this._webSocketClient != null)
                await this._webSocketClient.Disconnect().ConfigureAwait(false);
        }

        #endregion

        #region Public REST Methods
        /// <summary>
        /// Gets a user.
        /// </summary>
        /// <param name="UserId">Id of the user</param>
        /// <param name="Fetch">Whether to fetch the user again (Defaults to false).</param>
        /// <returns>The requested user.</returns>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordUser> GetUserAsync(ulong UserId, bool Fetch = true)
        {
            if (!Fetch)
            {
                return this.TryGetCachedUserInternal(UserId, out var usr) ? usr : new DiscordUser { Id = UserId, Discord = this };
            }
            else
            {
                var usr = await this.ApiClient.GetUser(UserId).ConfigureAwait(false);
                usr = this.UserCache.AddOrUpdate(UserId, usr, (Id, Old) =>
                {
                    Old.Username = usr.Username;
                    Old.Discriminator = usr.Discriminator;
                    Old.AvatarHash = usr.AvatarHash;
                    Old.BannerHash = usr.BannerHash;
                    Old._bannerColor = usr._bannerColor;
                    return Old;
                });

                return usr;
            }
        }

        /// <summary>
        /// Gets a channel.
        /// </summary>
        /// <param name="Id">The id of the channel to get.</param>
        /// <returns>The requested channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordChannel> GetChannelAsync(ulong Id)
            => this.InternalGetCachedChannel(Id) ?? await this.ApiClient.GetChannelAsync(Id).ConfigureAwait(false);

        /// <summary>
        /// Gets a thread.
        /// </summary>
        /// <param name="Id">The id of the thread to get.</param>
        /// <returns>The requested thread.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordThreadChannel> GetThreadAsync(ulong Id)
            => this.InternalGetCachedThread(Id) ?? await this.ApiClient.GetThreadAsync(Id).ConfigureAwait(false);

        /// <summary>
        /// Sends a normal message.
        /// </summary>
        /// <param name="Channel">Channel to send to.</param>
        /// <param name="Content">Message content to send.</param>
        /// <returns>The message that was sent.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> SendMessage(DiscordChannel Channel, string Content)
            => this.ApiClient.CreateMessageAsync(Channel.Id, Content, Embeds: null, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);

        /// <summary>
        /// Sends a message with an embed.
        /// </summary>
        /// <param name="Channel">Channel to send to.</param>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The message that was sent.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> SendMessage(DiscordChannel Channel, DiscordEmbed Embed)
            => this.ApiClient.CreateMessageAsync(Channel.Id, null, Embed != null ? new[] { Embed } : null, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);

        /// <summary>
        /// Sends a message with content and an embed.
        /// </summary>
        /// <param name="Channel">Channel to send to.</param>
        /// <param name="Content">Message content to send.</param>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The message that was sent.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> SendMessage(DiscordChannel Channel, string Content, DiscordEmbed Embed)
            => this.ApiClient.CreateMessageAsync(Channel.Id, Content, Embed != null ? new[] { Embed } : null, Sticker: null, ReplyMessageId: null, MentionReply: false, FailOnInvalidReply: false);

        /// <summary>
        /// Sends a message with the <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
        /// </summary>
        /// <param name="Channel">Channel to send the message to.</param>
        /// <param name="Builder">The message builder.</param>
        /// <returns>The message that was sent.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is false and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> SendMessage(DiscordChannel Channel, DiscordMessageBuilder Builder)
            => this.ApiClient.CreateMessageAsync(Channel.Id, Builder);

        /// <summary>
        /// Sends a message with an <see cref="System.Action{DiscordMessageBuilder}"/>.
        /// </summary>
        /// <param name="Channel">Channel to send the message to.</param>
        /// <param name="Action">The message builder.</param>
        /// <returns>The message that was sent.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is false and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordMessage> SendMessage(DiscordChannel Channel, Action<DiscordMessageBuilder> Action)
        {
            var builder = new DiscordMessageBuilder();
            Action(builder);

            return this.ApiClient.CreateMessageAsync(Channel.Id, builder);
        }

        /// <summary>
        /// Creates a guild. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="Name">Name of the guild.</param>
        /// <param name="Region">Voice region of the guild.</param>
        /// <param name="Icon">Stream containing the icon for the guild.</param>
        /// <param name="VerificationLevel">Verification level for the guild.</param>
        /// <param name="DefaultMessageNotifications">Default message notification settings for the guild.</param>
        /// <param name="SystemChannelFlags">System channel flags fopr the guild.</param>
        /// <returns>The created guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuild> CreateGuild(string Name, string Region = null, Optional<Stream> Icon = default, VerificationLevel? VerificationLevel = null,
            DefaultMessageNotifications? DefaultMessageNotifications = null, SystemChannelFlags? SystemChannelFlags = null)
        {
            var iconb64 = Optional.FromNoValue<string>();
            if (Icon.HasValue && Icon.Value != null)
                using (var imgtool = new ImageTool(Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (Icon.HasValue)
                iconb64 = null;

            return this.ApiClient.CreateGuildAsync(Name, Region, iconb64, VerificationLevel, DefaultMessageNotifications, SystemChannelFlags);
        }

        /// <summary>
        /// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
        /// </summary>
        /// <param name="Code">The template code.</param>
        /// <param name="Name">Name of the guild.</param>
        /// <param name="Icon">Stream containing the icon for the guild.</param>
        /// <returns>The created guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuild> CreateGuildFromTemplate(string Code, string Name, Optional<Stream> Icon = default)
        {
            var iconb64 = Optional.FromNoValue<string>();
            if (Icon.HasValue && Icon.Value != null)
                using (var imgtool = new ImageTool(Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (Icon.HasValue)
                iconb64 = null;

            return this.ApiClient.CreateGuildFromTemplateAsync(Code, Name, iconb64);
        }

        /// <summary>
        /// Executes a raw request.
        /// </summary>
        /// <example>
        /// <c>
        /// var request = await Client.ExecuteRawRequestAsync(RestRequestMethod.GET, $"{Endpoints.CHANNELS}/243184972190742178964/{Endpoints.INVITES}");
        /// List&lt;DiscordInvite&gt; invites = DiscordJson.ToDiscordObject&lt;List&lt;DiscordInvite&gt;&gt;(request.Response);
        /// </c>
        /// </example>
        /// <param name="Method">The method.</param>
        /// <param name="Route">The route.</param>
        /// <param name="RouteParams">The route parameters.</param>
        /// <param name="JsonBody">The json body.</param>
        /// <param name="AdditionalHeaders">The addditional headers.</param>
        /// <param name="RatelimitWaitOverride">The ratelimit wait override.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the ressource does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <returns>A awaitable RestResponse</returns>
        public async Task<RestResponse> ExecuteRawRequestAsync(RestRequestMethod Method, string Route, object RouteParams, string JsonBody = null, Dictionary<string, string> AdditionalHeaders = null, double? RatelimitWaitOverride = null)
        {

            var bucket = this.ApiClient.Rest.GetBucket(Method, Route, RouteParams, out var path);

            var url = Utilities.GetApiUriFor(path, this.Configuration);
            var res = await this.ApiClient.DoRequest(this, bucket, url, Method, Route, AdditionalHeaders, DiscordJson.SerializeObject(JsonBody), RatelimitWaitOverride);

            return res;
        }

        /// <summary>
        /// Gets a guild.
        /// <para>Setting <paramref name="WithCounts"/> to true will make a REST request.</para>
        /// </summary>
        /// <param name="Id">The guild ID to search for.</param>
        /// <param name="WithCounts">Whether to include approximate presence and member counts in the returned guild.</param>
        /// <returns>The requested Guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordGuild> GetGuildAsync(ulong Id, bool? WithCounts = null)
        {
            if (this._guilds.TryGetValue(Id, out var guild) && (!WithCounts.HasValue || !WithCounts.Value))
                return guild;

            guild = await this.ApiClient.GetGuildAsync(Id, WithCounts).ConfigureAwait(false);
            var channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id).ConfigureAwait(false);
            foreach (var channel in channels) guild._channels[channel.Id] = channel;

            return guild;
        }

        /// <summary>
        /// Gets a guild preview.
        /// </summary>
        /// <param name="Id">The guild ID.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildPreview> GetGuildPreview(ulong Id)
            => this.ApiClient.GetGuildPreviewAsync(Id);

        /// <summary>
        /// Gets an invite.
        /// </summary>
        /// <param name="Code">The invite code.</param>
        /// <param name="WithCounts">Whether to include presence and total member counts in the returned invite.</param>
        /// <param name="WithExpiration">Whether to include the expiration date in the returned invite.</param>
        /// <param name="ScheduledEventId">The scheduled event id.</param>
        /// <returns>The requested Invite.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordInvite> GetInviteByCode(string Code, bool? WithCounts = null, bool? WithExpiration = null, ulong? ScheduledEventId = null)
            => this.ApiClient.GetInviteAsync(Code, WithCounts, WithExpiration, ScheduledEventId);

        /// <summary>
        /// Gets a list of connections.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordConnection>> GetConnections()
            => this.ApiClient.GetUsersConnectionsAsync();

        /// <summary>
        /// Gets a sticker.
        /// </summary>
        /// <returns>The requested sticker.</returns>
        /// <param name="Id">The id of the sticker.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the sticker does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordSticker> GetSticker(ulong Id)
            => this.ApiClient.GetStickerAsync(Id);


        /// <summary>
        /// Gets all nitro sticker packs.
        /// </summary>
        /// <returns>List of sticker packs.</returns>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacks()
            => this.ApiClient.GetStickerPacksAsync();


        /// <summary>
        /// Gets the In-App OAuth Url.
        /// </summary>
        /// <param name="Scopes">Defaults to <see cref="OAuthScopes.BotDefault"/>.</param>
        /// <param name="Redir">Redirect Uri.</param>
        /// <param name="Permissions">Defaults to <see cref="Permissions.None"/>.</param>
        /// <returns>The OAuth Url</returns>
        public Uri GetInAppOAuth(Permissions Permissions = Permissions.None, OAuthScopes Scopes = OAuthScopes.BotDefault, string Redir = null)
        {
            Permissions &= PermissionMethods.FullPerms;
            // hey look, it's not all annoying and blue :P
            return new Uri(new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.Oauth2}{Endpoints.Authorize}")
                .AddParameter("client_id", this.CurrentApplication.Id.ToString(CultureInfo.InvariantCulture))
                .AddParameter("scope", OAuth.ResolveScopes(Scopes))
                .AddParameter("permissions", ((long)Permissions).ToString(CultureInfo.InvariantCulture))
                .AddParameter("state", "")
                .AddParameter("redirect_uri", Redir ?? "")
                .ToString());
        }

        /// <summary>
        /// Gets a webhook.
        /// </summary>
        /// <param name="Id">The target webhook id.</param>
        /// <returns>The requested webhook.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordWebhook> GetWebhook(ulong Id)
            => this.ApiClient.GetWebhookAsync(Id);

        /// <summary>
        /// Gets a webhook.
        /// </summary>
        /// <param name="Id">The target webhook id.</param>
        /// <param name="Token">The target webhook token.</param>
        /// <returns>The requested webhook.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordWebhook> GetWebhookWithToken(ulong Id, string Token)
            => this.ApiClient.GetWebhookWithTokenAsync(Id, Token);

        /// <summary>
        /// Updates current user's activity and status.
        /// </summary>
        /// <param name="Activity">Activity to set.</param>
        /// <param name="UserStatus">Status of the user.</param>
        /// <param name="IdleSince">Since when is the client performing the specified activity.</param>
        /// <returns></returns>
        public Task UpdateStatus(DiscordActivity Activity = null, UserStatus? UserStatus = null, DateTimeOffset? IdleSince = null)
            => this.InternalUpdateStatusAsync(Activity, UserStatus, IdleSince);

        /// <summary>
        /// Edits current user.
        /// </summary>
        /// <param name="Username">New username.</param>
        /// <param name="Avatar">New avatar.</param>
        /// <returns>The modified user.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordUser> UpdateCurrentUserAsync(string Username = null, Optional<Stream> Avatar = default)
        {
            var av64 = Optional.FromNoValue<string>();
            if (Avatar.HasValue && Avatar.Value != null)
                using (var imgtool = new ImageTool(Avatar.Value))
                    av64 = imgtool.GetBase64();
            else if (Avatar.HasValue)
                av64 = null;

            var usr = await this.ApiClient.ModifyCurrentUserAsync(Username, av64).ConfigureAwait(false);

            this.CurrentUser.Username = usr.Username;
            this.CurrentUser.Discriminator = usr.Discriminator;
            this.CurrentUser.AvatarHash = usr.AvatarHash;
            return this.CurrentUser;
        }

        /// <summary>
        /// Gets a guild template by the code.
        /// </summary>
        /// <param name="Code">The code of the template.</param>
        /// <returns>The guild template for the code.</returns>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildTemplate> GetTemplate(string Code)
            => this.ApiClient.GetTemplateAsync(Code);

        /// <summary>
        /// Gets all the global application commands for this application.
        /// </summary>
        /// <returns>A list of global application commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommands() =>
            this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id);

        /// <summary>
        /// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
        /// </summary>
        /// <param name="Commands">The list of commands to overwrite with.</param>
        /// <returns>The list of global commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommands(IEnumerable<DiscordApplicationCommand> Commands) =>
            this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, Commands);

        /// <summary>
        /// Creates or overwrites a global application command.
        /// </summary>
        /// <param name="Command">The command to create.</param>
        /// <returns>The created command.</returns>
        public Task<DiscordApplicationCommand> CreateGlobalApplicationCommand(DiscordApplicationCommand Command) =>
            this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, Command);

        /// <summary>
        /// Gets a global application command by its id.
        /// </summary>
        /// <param name="CommandId">The id of the command to get.</param>
        /// <returns>The command with the id.</returns>
        public Task<DiscordApplicationCommand> GetGlobalApplicationCommand(ulong CommandId) =>
            this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, CommandId);

        /// <summary>
        /// Edits a global application command.
        /// </summary>
        /// <param name="CommandId">The id of the command to edit.</param>
        /// <param name="Action">Action to perform.</param>
        /// <returns>The edited command.</returns>
        public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong CommandId, Action<ApplicationCommandEditModel> Action)
        {
            var mdl = new ApplicationCommandEditModel();
            Action(mdl);
            var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
            return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, CommandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NameLocalizations, mdl.DescriptionLocalizations).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a global application command.
        /// </summary>
        /// <param name="CommandId">The id of the command to delete.</param>
        public Task DeleteGlobalApplicationCommand(ulong CommandId) =>
            this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, CommandId);

        /// <summary>
        /// Gets all the application commands for a guild.
        /// </summary>
        /// <param name="GuildId">The id of the guild to get application commands for.</param>
        /// <returns>A list of application commands in the guild.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommands(ulong GuildId) =>
            this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, GuildId);

        /// <summary>
        /// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
        /// </summary>
        /// <param name="GuildId">The id of the guild.</param>
        /// <param name="Commands">The list of commands to overwrite with.</param>
        /// <returns>The list of guild commands.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommands(ulong GuildId, IEnumerable<DiscordApplicationCommand> Commands) =>
            this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, GuildId, Commands);

        /// <summary>
        /// Creates or overwrites a guild application command.
        /// </summary>
        /// <param name="GuildId">The id of the guild to create the application command in.</param>
        /// <param name="Command">The command to create.</param>
        /// <returns>The created command.</returns>
        public Task<DiscordApplicationCommand> CreateGuildApplicationCommand(ulong GuildId, DiscordApplicationCommand Command) =>
            this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, GuildId, Command);

        /// <summary>
        /// Gets a application command in a guild by its id.
        /// </summary>
        /// <param name="GuildId">The id of the guild the application command is in.</param>
        /// <param name="CommandId">The id of the command to get.</param>
        /// <returns>The command with the id.</returns>
        public Task<DiscordApplicationCommand> GetGuildApplicationCommand(ulong GuildId, ulong CommandId) =>
             this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, GuildId, CommandId);

        /// <summary>
        /// Edits a application command in a guild.
        /// </summary>
        /// <param name="GuildId">The id of the guild the application command is in.</param>
        /// <param name="CommandId">The id of the command to edit.</param>
        /// <param name="Action">Action to perform.</param>
        /// <returns>The edited command.</returns>
        public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong GuildId, ulong CommandId, Action<ApplicationCommandEditModel> Action)
        {
            var mdl = new ApplicationCommandEditModel();
            Action(mdl);
            var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
            return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, GuildId, CommandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NameLocalizations, mdl.DescriptionLocalizations).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a application command in a guild.
        /// </summary>
        /// <param name="GuildId">The id of the guild to delete the application command in.</param>
        /// <param name="CommandId">The id of the command.</param>
        public Task DeleteGuildApplicationCommand(ulong GuildId, ulong CommandId) =>
            this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, GuildId, CommandId);

        /// <summary>
        /// Gets all command permissions for a guild.
        /// </summary>
        /// <param name="GuildId">The target guild.</param>
        public Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissions(ulong GuildId) =>
            this.ApiClient.GetGuildApplicationCommandPermissionsAsync(this.CurrentApplication.Id, GuildId);

        /// <summary>
        /// Gets the permissions for a guild command.
        /// </summary>
        /// <param name="GuildId">The target guild.</param>
        /// <param name="CommandId">The target command id.</param>
        public Task<DiscordGuildApplicationCommandPermission> GetApplicationCommandPermission(ulong GuildId, ulong CommandId) =>
            this.ApiClient.GetApplicationCommandPermissionAsync(this.CurrentApplication.Id, GuildId, CommandId);

        /// <summary>
        /// Overwrites the existing permissions for a application command in a guild. New permissions are automatically created and missing permissions are deleted.
        /// A command takes up to 10 permission overwrites.
        /// </summary>
        /// <param name="GuildId">The id of the guild.</param>
        /// <param name="CommandId">The id of the command.</param>
        /// <param name="Permissions">List of permissions.</param>
        public Task<DiscordGuildApplicationCommandPermission> OverwriteGuildApplicationCommandPermissions(ulong GuildId, ulong CommandId, IEnumerable<DiscordApplicationCommandPermission> Permissions) =>
            this.ApiClient.OverwriteGuildApplicationCommandPermissionsAsync(this.CurrentApplication.Id, GuildId, CommandId, Permissions);

        /// <summary>
        /// Overwrites the existing application command permissions in a guild. New permissions are automatically created and missing permissions are deleted.
        /// Each command takes up to 10 permission overwrites.
        /// </summary>
        /// <param name="GuildId">The id of the guild.</param>
        /// <param name="PermissionsOverwrites">The list of permissions to overwrite with.</param>
        public Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> BulkOverwriteGuildApplicationCommands(ulong GuildId, IEnumerable<DiscordGuildApplicationCommandPermission> PermissionsOverwrites) =>
            this.ApiClient.BulkOverwriteApplicationCommandPermissionsAsync(this.CurrentApplication.Id, GuildId, PermissionsOverwrites);
        #endregion

        #region Internal Caching Methods
        /// <summary>
        /// Gets the internal chached threads.
        /// </summary>
        /// <param name="ThreadId">The target thread id.</param>
        /// <returns>The requested thread.</returns>
        internal DiscordThreadChannel InternalGetCachedThread(ulong ThreadId)
        {
            foreach (var guild in this.Guilds.Values)
                if (guild.Threads.TryGetValue(ThreadId, out var foundThread))
                    return foundThread;

            return null;
        }


        /// <summary>
        /// Gets the internal chached scheduled event.
        /// </summary>
        /// <param name="ScheduledEventId">The target scheduled event id.</param>
        /// <returns>The requested scheduled event.</returns>
        internal DiscordScheduledEvent InternalGetCachedScheduledEvent(ulong ScheduledEventId)
        {
            foreach (var guild in this.Guilds.Values)
                if (guild.ScheduledEvents.TryGetValue(ScheduledEventId, out var foundScheduledEvent))
                    return foundScheduledEvent;

            return null;
        }

        /// <summary>
        /// Gets the internal chached channel.
        /// </summary>
        /// <param name="ChannelId">The target channel id.</param>
        /// <returns>The requested channel.</returns>
        internal DiscordChannel InternalGetCachedChannel(ulong ChannelId)
        {
            foreach (var guild in this.Guilds.Values)
                if (guild.Channels.TryGetValue(ChannelId, out var foundChannel))
                    return foundChannel;

            return null;
        }

        /// <summary>
        /// Gets the internal chached guild.
        /// </summary>
        /// <param name="GuildId">The target guild id.</param>
        /// <returns>The requested guild.</returns>
        internal DiscordGuild InternalGetCachedGuild(ulong? GuildId)
        {
            if (this._guilds != null && GuildId.HasValue)
            {
                if (this._guilds.TryGetValue(GuildId.Value, out var guild))
                    return guild;
            }

            return null;
        }

        /// <summary>
        /// Updates a message.
        /// </summary>
        /// <param name="Message">The message to update.</param>
        /// <param name="Author">The author to update.</param>
        /// <param name="Guild">The guild to update.</param>
        /// <param name="Member">The member to update.</param>
        private void UpdateMessage(DiscordMessage Message, TransportUser Author, DiscordGuild Guild, TransportMember Member)
        {
            if (Author != null)
            {
                var usr = new DiscordUser(Author) { Discord = this };

                if (Member != null)
                    Member.User = Author;

                Message.Author = this.UpdateUser(usr, Guild?.Id, Guild, Member);
            }

            var channel = this.InternalGetCachedChannel(Message.ChannelId);

            if (channel != null) return;

            channel = !Message.GuildId.HasValue
                ? new DiscordDmChannel
                {
                    Id = Message.ChannelId,
                    Discord = this,
                    Type = ChannelType.Private
                }
                : new DiscordChannel
                {
                    Id = Message.ChannelId,
                    Discord = this
                };

            Message.Channel = channel;
        }

        /// <summary>
        /// Updates a scheduled event.
        /// </summary>
        /// <param name="ScheduledEvent">The scheduled event to update.</param>
        /// <param name="Guild">The guild to update.</param>
        /// <returns>The updated scheduled event.</returns>
        private DiscordScheduledEvent UpdateScheduledEvent(DiscordScheduledEvent ScheduledEvent, DiscordGuild Guild)
        {
            if (ScheduledEvent != null)
            {
                _ = Guild._scheduledEvents.AddOrUpdate(ScheduledEvent.Id, ScheduledEvent, (Id, Old) =>
                {
                    Old.Discord = this;
                    Old.Description = ScheduledEvent.Description;
                    Old.ChannelId = ScheduledEvent.ChannelId;
                    Old.EntityId = ScheduledEvent.EntityId;
                    Old.EntityType = ScheduledEvent.EntityType;
                    Old.EntityMetadata = ScheduledEvent.EntityMetadata;
                    Old.PrivacyLevel = ScheduledEvent.PrivacyLevel;
                    Old.Name = ScheduledEvent.Name;
                    Old.Status = ScheduledEvent.Status;
                    Old.UserCount = ScheduledEvent.UserCount;
                    Old.ScheduledStartTimeRaw = ScheduledEvent.ScheduledStartTimeRaw;
                    Old.ScheduledEndTimeRaw = ScheduledEvent.ScheduledEndTimeRaw;
                    return Old;
                });
            }

            return ScheduledEvent;
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        /// <param name="Usr">The user to update.</param>
        /// <param name="GuildId">The guild id to update.</param>
        /// <param name="Guild">The guild to update.</param>
        /// <param name="Mbr">The member to update.</param>
        /// <returns>The updated user.</returns>
        private DiscordUser UpdateUser(DiscordUser Usr, ulong? GuildId, DiscordGuild Guild, TransportMember Mbr)
        {
            if (Mbr != null)
            {
                if (Mbr.User != null)
                {
                    Usr = new DiscordUser(Mbr.User) { Discord = this };

                    _ = this.UserCache.AddOrUpdate(Usr.Id, Usr, (Id, Old) =>
                    {
                        Old.Username = Usr.Username;
                        Old.Discriminator = Usr.Discriminator;
                        Old.AvatarHash = Usr.AvatarHash;
                        return Old;
                    });

                    Usr = new DiscordMember(Mbr) { Discord = this, _guildId = GuildId.Value };
                }

                var intents = this.Configuration.Intents;

                DiscordMember member = default;

                if (!intents.HasAllPrivilegedIntents() || Guild.IsLarge) // we have the necessary privileged intents, no need to worry about caching here unless guild is large.
                {
                    if (Guild?._members.TryGetValue(Usr.Id, out member) == false)
                    {
                        if (intents.HasIntent(DiscordIntents.GuildMembers) || this.Configuration.AlwaysCacheMembers) // member can be updated by events, so cache it
                        {
                            Guild._members.TryAdd(Usr.Id, (DiscordMember)Usr);
                        }
                    }
                    else if (intents.HasIntent(DiscordIntents.GuildPresences) || this.Configuration.AlwaysCacheMembers) // we can attempt to update it if it's already in cache.
                    {
                        if (!intents.HasIntent(DiscordIntents.GuildMembers)) // no need to update if we already have the member events
                        {
                            _ = Guild._members.TryUpdate(Usr.Id, (DiscordMember)Usr, member);
                        }
                    }
                }
            }
            else if (Usr.Username != null) // check if not a skeleton user
            {
                _ = this.UserCache.AddOrUpdate(Usr.Id, Usr, (Id, Old) =>
                {
                    Old.Username = Usr.Username;
                    Old.Discriminator = Usr.Discriminator;
                    Old.AvatarHash = Usr.AvatarHash;
                    return Old;
                });
            }

            return Usr;
        }

        /// <summary>
        /// Updates the cached events in a guild.
        /// </summary>
        /// <param name="Guild">The guild.</param>
        /// <param name="RawEvents">The raw events.</param>
        private void UpdateCachedScheduledEvent(DiscordGuild Guild, JArray RawEvents)
        {
            if (this._disposed)
                return;

            if (RawEvents != null)
            {
                Guild._scheduledEvents.Clear();

                foreach (var xj in RawEvents)
                {
                    var xtm = xj.ToDiscordObject<DiscordScheduledEvent>();

                    xtm.Discord = this;

                    Guild._scheduledEvents[xtm.Id] = xtm;
                }
            }
        }

        /// <summary>
        /// Updates the cached guild.
        /// </summary>
        /// <param name="NewGuild">The new guild.</param>
        /// <param name="RawMembers">The raw members.</param>
        private void UpdateCachedGuild(DiscordGuild NewGuild, JArray RawMembers)
        {
            if (this._disposed)
                return;

            if (!this._guilds.ContainsKey(NewGuild.Id))
                this._guilds[NewGuild.Id] = NewGuild;

            var guild = this._guilds[NewGuild.Id];

            if (NewGuild._channels != null && NewGuild._channels.Count > 0)
            {
                foreach (var channel in NewGuild._channels.Values)
                {
                    if (guild._channels.TryGetValue(channel.Id, out _)) continue;

                    foreach (var overwrite in channel._permissionOverwrites)
                    {
                        overwrite.Discord = this;
                        overwrite._channelId = channel.Id;
                    }

                    guild._channels[channel.Id] = channel;
                }
            }

            if (NewGuild._threads != null && NewGuild._threads.Count > 0)
            {
                foreach (var thread in NewGuild._threads.Values)
                {
                    if (guild._threads.TryGetValue(thread.Id, out _)) continue;

                    guild._threads[thread.Id] = thread;
                }
            }

            if (NewGuild._scheduledEvents != null && NewGuild._scheduledEvents.Count > 0)
            {
                foreach (var sEvent in NewGuild._scheduledEvents.Values)
                {
                    if (guild._scheduledEvents.TryGetValue(sEvent.Id, out _)) continue;

                    guild._scheduledEvents[sEvent.Id] = sEvent;
                }
            }

            foreach (var newEmoji in NewGuild._emojis.Values)
                _ = guild._emojis.GetOrAdd(newEmoji.Id, _ => newEmoji);

            foreach (var newSticker in NewGuild._stickers.Values)
                _ = guild._stickers.GetOrAdd(newSticker.Id, _ => newSticker);

            foreach (var newStageInstance in NewGuild._stageInstances.Values)
                _ = guild._stageInstances.GetOrAdd(newStageInstance.Id, _ => newStageInstance);

            if (RawMembers != null)
            {
                guild._members.Clear();

                foreach (var xj in RawMembers)
                {
                    var xtm = xj.ToDiscordObject<TransportMember>();

                    var xu = new DiscordUser(xtm.User) { Discord = this };
                    _ = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (Id, Old) =>
                    {
                        Old.Username = xu.Username;
                        Old.Discriminator = xu.Discriminator;
                        Old.AvatarHash = xu.AvatarHash;
                        Old.PremiumType = xu.PremiumType;
                        return Old;
                    });

                    guild._members[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, _guildId = guild.Id };
                }
            }

            foreach (var role in NewGuild._roles.Values)
            {
                if (guild._roles.TryGetValue(role.Id, out _)) continue;

                role._guildId = guild.Id;
                guild._roles[role.Id] = role;
            }

            guild.Name = NewGuild.Name;
            guild.AfkChannelId = NewGuild.AfkChannelId;
            guild.AfkTimeout = NewGuild.AfkTimeout;
            guild.DefaultMessageNotifications = NewGuild.DefaultMessageNotifications;
            guild.RawFeatures = NewGuild.RawFeatures;
            guild.IconHash = NewGuild.IconHash;
            guild.MfaLevel = NewGuild.MfaLevel;
            guild.OwnerId = NewGuild.OwnerId;
            guild.VoiceRegionId = NewGuild.VoiceRegionId;
            guild.SplashHash = NewGuild.SplashHash;
            guild.VerificationLevel = NewGuild.VerificationLevel;
            guild.WidgetEnabled = NewGuild.WidgetEnabled;
            guild.WidgetChannelId = NewGuild.WidgetChannelId;
            guild.ExplicitContentFilter = NewGuild.ExplicitContentFilter;
            guild.PremiumTier = NewGuild.PremiumTier;
            guild.PremiumSubscriptionCount = NewGuild.PremiumSubscriptionCount;
            guild.PremiumProgressBarEnabled = NewGuild.PremiumProgressBarEnabled;
            guild.BannerHash = NewGuild.BannerHash;
            guild.Description = NewGuild.Description;
            guild.VanityUrlCode = NewGuild.VanityUrlCode;
            guild.SystemChannelId = NewGuild.SystemChannelId;
            guild.SystemChannelFlags = NewGuild.SystemChannelFlags;
            guild.DiscoverySplashHash = NewGuild.DiscoverySplashHash;
            guild.MaxMembers = NewGuild.MaxMembers;
            guild.MaxPresences = NewGuild.MaxPresences;
            guild.ApproximateMemberCount = NewGuild.ApproximateMemberCount;
            guild.ApproximatePresenceCount = NewGuild.ApproximatePresenceCount;
            guild.MaxVideoChannelUsers = NewGuild.MaxVideoChannelUsers;
            guild.PreferredLocale = NewGuild.PreferredLocale;
            guild.RulesChannelId = NewGuild.RulesChannelId;
            guild.PublicUpdatesChannelId = NewGuild.PublicUpdatesChannelId;
            guild.ApplicationId = NewGuild.ApplicationId;

            // fields not sent for update:
            // - guild.Channels
            // - voice states
            // - guild.JoinedAt = new_guild.JoinedAt;
            // - guild.Large = new_guild.Large;
            // - guild.MemberCount = Math.Max(new_guild.MemberCount, guild._members.Count);
            // - guild.Unavailable = new_guild.Unavailable;
        }

        /// <summary>
        /// Populates the message reactions and cache.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="Author">The author.</param>
        /// <param name="Member">The member.</param>
        private void PopulateMessageReactionsAndCache(DiscordMessage Message, TransportUser Author, TransportMember Member)
        {
            var guild = Message.Channel?.Guild ?? this.InternalGetCachedGuild(Message.GuildId);

            this.UpdateMessage(Message, Author, guild, Member);

            if (Message._reactions == null)
                Message._reactions = new List<DiscordReaction>();
            foreach (var xr in Message._reactions)
                xr.Emoji.Discord = this;

            if (this.Configuration.MessageCacheSize > 0 && Message.Channel != null)
                this.MessageCache?.Add(Message);
        }


        #endregion

        #region Disposal

        ~DiscordClient()
        {
            this.Dispose();
        }


        private bool _disposed;
        /// <summary>
        /// Disposes the client.
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            GC.SuppressFinalize(this);

            this.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            this.ApiClient.Rest.Dispose();
            this.CurrentUser = null;

            var extensions = this._extensions; // prevent _extensions being modified during dispose
            this._extensions = null;
            foreach (var extension in extensions)
                if (extension is IDisposable disposable)
                    disposable.Dispose();

            try
            {
                this._cancelTokenSource?.Cancel();
                this._cancelTokenSource?.Dispose();
            }
            catch { }

            this._guilds = null;
            this._heartbeatTask = null;
        }

        #endregion
    }
}
