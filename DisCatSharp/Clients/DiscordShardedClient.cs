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

#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DisCatSharp
{
    /// <summary>
    /// A Discord client that shards automatically.
    /// </summary>
    public sealed partial class DiscordShardedClient
    {
        #region Public Properties

        /// <summary>
        /// Gets the logger for this client.
        /// </summary>
        public ILogger<BaseDiscordClient> Logger { get; }

        /// <summary>
        /// Gets all client shards.
        /// </summary>
        public IReadOnlyDictionary<int, DiscordClient> ShardClients { get; }

        /// <summary>
        /// Gets the gateway info for the client's session.
        /// </summary>
        public GatewayInfo GatewayInfo { get; private set; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser { get; private set; }

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication { get; private set; }

        /// <summary>
        /// Gets the library team.
        /// </summary>
        public DisCatSharpTeam LibraryDeveloperTeam
            => this.GetShard(0).LibraryDeveloperTeam;

        /// <summary>
        /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
            => this._voiceRegionsLazy?.Value;

        #endregion

        #region Private Properties/Fields

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        private DiscordConfiguration Configuration { get; }

        /// <summary>
        /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
        /// </summary>
        private ConcurrentDictionary<string, DiscordVoiceRegion> _internalVoiceRegions;

        private readonly ConcurrentDictionary<int, DiscordClient> _shards = new();
        private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voiceRegionsLazy;
        private bool _isStarted;
        private readonly bool _manuallySharding;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes new auto-sharding Discord client.
        /// </summary>
        /// <param name="Config">Configuration to use.</param>
        public DiscordShardedClient(DiscordConfiguration Config)
        {
            this.InternalSetup();

            if (Config.ShardCount > 1)
                this._manuallySharding = true;

            this.Configuration = Config;
            this.ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(this._shards);

            if (this.Configuration.LoggerFactory == null)
            {
                this.Configuration.LoggerFactory = new DefaultLoggerFactory();
                this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this.Configuration.MinimumLogLevel, this.Configuration.LogTimestampFormat));
            }
            this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes and connects all shards.
        /// </summary>
        /// <exception cref="System.AggregateException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <returns></returns>
        public async Task StartAsync()
        {
            if (this._isStarted)
                throw new InvalidOperationException("This client has already been started.");

            this._isStarted = true;

            try
            {
                if (this.Configuration.TokenType != TokenType.Bot)
                    this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
                this.Logger.LogInformation(LoggerEvents.Startup, "Lib {0}, version {1}", this._botLibrary, this._versionString.Value);

                var shardc = await this.InitializeShardsAsync().ConfigureAwait(false);
                var connectTasks = new List<Task>();
                this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {0} shards.", shardc);

                for (var i = 0; i < shardc; i++)
                {
                    //This should never happen, but in case it does...
                    if (this.GatewayInfo.SessionBucket.MaxConcurrency < 1)
                        this.GatewayInfo.SessionBucket.MaxConcurrency = 1;

                    if (this.GatewayInfo.SessionBucket.MaxConcurrency == 1)
                        await this.ConnectShard(i).ConfigureAwait(false);
                    else
                    {
                        //Concurrent login.
                        connectTasks.Add(this.ConnectShard(i));

                        if (connectTasks.Count == this.GatewayInfo.SessionBucket.MaxConcurrency)
                        {
                            await Task.WhenAll(connectTasks).ConfigureAwait(false);
                            connectTasks.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await this.InternalStop(false).ConfigureAwait(false);

                var message = $"Shard initialization failed, check inner exceptions for details: ";

                this.Logger.LogCritical(LoggerEvents.ShardClientError, $"{message}\n{ex}");
                throw new AggregateException(message, ex);
            }
        }
        /// <summary>
        /// Disconnects and disposes of all shards.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Task Stop()
            => this.InternalStop();

        /// <summary>
        /// Gets a shard from a guild ID.
        /// <para>
        ///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
        ///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
        /// </para>
        /// </summary>
        /// <param name="GuildId">The guild ID for the shard.</param>
        /// <returns>The found <see cref="DiscordClient"/> shard. Otherwise <see langword="null"/> if the shard was not found for the guild ID.</returns>
        public DiscordClient GetShard(ulong GuildId)
        {
            var index = this._manuallySharding ? this.GetShardIdFromGuilds(GuildId) : Utilities.GetShardId(GuildId, this.ShardClients.Count);

            return index != -1 ? this._shards[index] : null;
        }

        /// <summary>
        /// Gets a shard from a guild.
        /// <para>
        ///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
        ///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
        /// </para>
        /// </summary>
        /// <param name="Guild">The guild for the shard.</param>
        /// <returns>The found <see cref="DiscordClient"/> shard. Otherwise <see langword="null"/> if the shard was not found for the guild.</returns>
        public DiscordClient GetShard(DiscordGuild Guild)
            => this.GetShard(Guild.Id);

        /// <summary>
        /// Updates playing statuses on all shards.
        /// </summary>
        /// <param name="Activity">Activity to set.</param>
        /// <param name="UserStatus">Status of the user.</param>
        /// <param name="IdleSince">Since when is the client performing the specified activity.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task UpdateStatusAsync(DiscordActivity Activity = null, UserStatus? UserStatus = null, DateTimeOffset? IdleSince = null)
        {
            var tasks = new List<Task>();
            foreach (var client in this._shards.Values)
                tasks.Add(client.UpdateStatus(Activity, UserStatus, IdleSince));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Initializes the shards async.
        /// </summary>
        /// <returns>A Task.</returns>
        internal async Task<int> InitializeShardsAsync()
        {
            if (this._shards.Count != 0)
                return this._shards.Count;

            this.GatewayInfo = await this.GetGatewayInfo().ConfigureAwait(false);
            var shardc = this.Configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this.Configuration.ShardCount;
            var lf = new ShardedLoggerFactory(this.Logger);
            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfiguration(this.Configuration)
                {
                    ShardId = i,
                    ShardCount = shardc,
                    LoggerFactory = lf
                };

                var client = new DiscordClient(cfg);
                if (!this._shards.TryAdd(i, client))
                    throw new InvalidOperationException("Could not initialize shards.");
            }

            return shardc;
        }

        #endregion

        #region Private Methods/Version Property

        /// <summary>
        /// Gets the gateway info async.
        /// </summary>
        /// <returns>A Task.</returns>
        private async Task<GatewayInfo> GetGatewayInfo()
        {
            var url = $"{Utilities.GetApiBaseUri(this.Configuration)}{Endpoints.Gateway}{Endpoints.Bot}";
            var http = new HttpClient();

            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this.Configuration));
            if (this.Configuration != null && this.Configuration.Override != null)
            {
                http.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", this.Configuration.Override);
            }

            this.Logger.LogDebug(LoggerEvents.ShardRest, $"Obtaining gateway information from GET {Endpoints.Gateway}{Endpoints.Bot}...");
            var resp = await http.GetAsync(url).ConfigureAwait(false);

            http.Dispose();

            if (!resp.IsSuccessStatusCode)
            {
                var ratelimited = await HandleHttpError(url, resp).ConfigureAwait(false);

                if (ratelimited)
                    return await this.GetGatewayInfo().ConfigureAwait(false);
            }

            var timer = new Stopwatch();
            timer.Start();

            var jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            var info = jo.ToObject<GatewayInfo>();

            //There is a delay from parsing here.
            timer.Stop();

            info.SessionBucket.resetAfter -= (int)timer.ElapsedMilliseconds;
            info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.resetAfter);

            return info;

            async Task<bool> HandleHttpError(string ReqUrl, HttpResponseMessage Msg)
            {
                var code = (int)Msg.StatusCode;

                if (code == 401 || code == 403)
                {
                    throw new Exception($"Authentication failed, check your token and try again: {code} {Msg.ReasonPhrase}");
                }
                else if (code == 429)
                {
                    this.Logger.LogError(LoggerEvents.ShardClientError, $"Ratelimit hit, requeuing request to {ReqUrl}");

                    var hs = Msg.Headers.ToDictionary(Xh => Xh.Key, Xh => string.Join("\n", Xh.Value), StringComparer.OrdinalIgnoreCase);
                    var waitInterval = 0;

                    if (hs.TryGetValue("Retry-After", out var retryAfterRaw))
                        waitInterval = int.Parse(retryAfterRaw, CultureInfo.InvariantCulture);

                    await Task.Delay(waitInterval).ConfigureAwait(false);
                    return true;
                }
                else if (code >= 500)
                {
                    throw new Exception($"Internal Server Error: {code} {Msg.ReasonPhrase}");
                }
                else
                {
                    throw new Exception($"An unsuccessful HTTP status code was encountered: {code} {Msg.ReasonPhrase}");
                }
            }
        }


        private readonly Lazy<string> _versionString = new(() =>
        {
            var a = typeof(DiscordShardedClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                return iv.InformationalVersion;

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
                vs = $"{vs}, CI build {v.Revision}";

            return vs;
        });

        private readonly string _botLibrary = "DisCatSharp";

        #endregion

        #region Private Connection Methods

        /// <summary>
        /// Connects the shard async.
        /// </summary>
        /// <param name="I">The i.</param>
        /// <returns>A Task.</returns>
        private async Task ConnectShard(int I)
        {
            if (!this._shards.TryGetValue(I, out var client))
                throw new Exception($"Could not initialize shard {I}.");

            if (this.GatewayInfo != null)
            {
                client.GatewayInfo = this.GatewayInfo;
                client.GatewayUri = new Uri(client.GatewayInfo.Url);
            }

            if (this.CurrentUser != null)
                client.CurrentUser = this.CurrentUser;

            if (this.CurrentApplication != null)
                client.CurrentApplication = this.CurrentApplication;

            if (this._internalVoiceRegions != null)
            {
                client.InternalVoiceRegions = this._internalVoiceRegions;
                client._voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
            }

            this.HookEventHandlers(client);

            client._isShard = true;
            await client.ConnectAsync().ConfigureAwait(false);
            this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booted shard {0}.", I);

            if (this.CurrentUser == null)
                this.CurrentUser = client.CurrentUser;

            if (this.CurrentApplication == null)
                this.CurrentApplication = client.CurrentApplication;

            if (this._internalVoiceRegions == null)
            {
                this._internalVoiceRegions = client.InternalVoiceRegions;
                this._voiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this._internalVoiceRegions));
            }
        }

        /// <summary>
        /// Internals the stop async.
        /// </summary>
        /// <param name="EnableLogger">If true, enable logger.</param>
        /// <returns>A Task.</returns>
        private Task InternalStop(bool EnableLogger = true)
        {
            if (!this._isStarted)
                throw new InvalidOperationException("This client has not been started.");

            if (EnableLogger)
                this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {0} shards.", this._shards.Count);

            this._isStarted = false;
            this._voiceRegionsLazy = null;

            this.GatewayInfo = null;
            this.CurrentUser = null;
            this.CurrentApplication = null;

            for (var i = 0; i < this._shards.Count; i++)
            {
                if (this._shards.TryGetValue(i, out var client))
                {
                    this.UnhookEventHandlers(client);

                    client.Dispose();

                    if (EnableLogger)
                        this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {0}.", i);
                }
            }

            this._shards.Clear();

            return Task.CompletedTask;
        }

        #endregion

        #region Event Handler Initialization/Registering

        /// <summary>
        /// Internals the setup.
        /// </summary>
        private void InternalSetup()
        {
            this._clientErrored = new AsyncEvent<DiscordClient, ClientErrorEventArgs>("CLIENT_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
            this._socketErrored = new AsyncEvent<DiscordClient, SocketErrorEventArgs>("SOCKET_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
            this._socketOpened = new AsyncEvent<DiscordClient, SocketEventArgs>("SOCKET_OPENED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._socketClosed = new AsyncEvent<DiscordClient, SocketCloseEventArgs>("SOCKET_CLOSED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._ready = new AsyncEvent<DiscordClient, ReadyEventArgs>("READY", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._resumed = new AsyncEvent<DiscordClient, ReadyEventArgs>("RESUMED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelCreated = new AsyncEvent<DiscordClient, ChannelCreateEventArgs>("CHANNEL_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelUpdated = new AsyncEvent<DiscordClient, ChannelUpdateEventArgs>("CHANNEL_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelDeleted = new AsyncEvent<DiscordClient, ChannelDeleteEventArgs>("CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._dmChannelDeleted = new AsyncEvent<DiscordClient, DmChannelDeleteEventArgs>("DM_CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._channelPinsUpdated = new AsyncEvent<DiscordClient, ChannelPinsUpdateEventArgs>("CHANNEL_PINS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildCreated = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildAvailable = new AsyncEvent<DiscordClient, GuildCreateEventArgs>("GUILD_AVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildUpdated = new AsyncEvent<DiscordClient, GuildUpdateEventArgs>("GUILD_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildDeleted = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildUnavailable = new AsyncEvent<DiscordClient, GuildDeleteEventArgs>("GUILD_UNAVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildDownloadCompleted = new AsyncEvent<DiscordClient, GuildDownloadCompletedEventArgs>("GUILD_DOWNLOAD_COMPLETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._inviteCreated = new AsyncEvent<DiscordClient, InviteCreateEventArgs>("INVITE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._inviteDeleted = new AsyncEvent<DiscordClient, InviteDeleteEventArgs>("INVITE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageCreated = new AsyncEvent<DiscordClient, MessageCreateEventArgs>("MESSAGE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._presenceUpdated = new AsyncEvent<DiscordClient, PresenceUpdateEventArgs>("PRESENCE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildBanAdded = new AsyncEvent<DiscordClient, GuildBanAddEventArgs>("GUILD_BAN_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildBanRemoved = new AsyncEvent<DiscordClient, GuildBanRemoveEventArgs>("GUILD_BAN_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildEmojisUpdated = new AsyncEvent<DiscordClient, GuildEmojisUpdateEventArgs>("GUILD_EMOJI_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildStickersUpdated = new AsyncEvent<DiscordClient, GuildStickersUpdateEventArgs>("GUILD_STICKER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationsUpdated = new AsyncEvent<DiscordClient, GuildIntegrationsUpdateEventArgs>("GUILD_INTEGRATIONS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberAdded = new AsyncEvent<DiscordClient, GuildMemberAddEventArgs>("GUILD_MEMBER_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberRemoved = new AsyncEvent<DiscordClient, GuildMemberRemoveEventArgs>("GUILD_MEMBER_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMemberUpdated = new AsyncEvent<DiscordClient, GuildMemberUpdateEventArgs>("GUILD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleCreated = new AsyncEvent<DiscordClient, GuildRoleCreateEventArgs>("GUILD_ROLE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleUpdated = new AsyncEvent<DiscordClient, GuildRoleUpdateEventArgs>("GUILD_ROLE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildRoleDeleted = new AsyncEvent<DiscordClient, GuildRoleDeleteEventArgs>("GUILD_ROLE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageUpdated = new AsyncEvent<DiscordClient, MessageUpdateEventArgs>("MESSAGE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageDeleted = new AsyncEvent<DiscordClient, MessageDeleteEventArgs>("MESSAGE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageBulkDeleted = new AsyncEvent<DiscordClient, MessageBulkDeleteEventArgs>("MESSAGE_BULK_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._interactionCreated = new AsyncEvent<DiscordClient, InteractionCreateEventArgs>("INTERACTION_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._componentInteractionCreated = new AsyncEvent<DiscordClient, ComponentInteractionCreateEventArgs>("COMPONENT_INTERACTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._contextMenuInteractionCreated = new AsyncEvent<DiscordClient, ContextMenuInteractionCreateEventArgs>("CONTEXT_MENU_INTERACTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._typingStarted = new AsyncEvent<DiscordClient, TypingStartEventArgs>("TYPING_STARTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._userSettingsUpdated = new AsyncEvent<DiscordClient, UserSettingsUpdateEventArgs>("USER_SETTINGS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._userUpdated = new AsyncEvent<DiscordClient, UserUpdateEventArgs>("USER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._voiceStateUpdated = new AsyncEvent<DiscordClient, VoiceStateUpdateEventArgs>("VOICE_STATE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._voiceServerUpdated = new AsyncEvent<DiscordClient, VoiceServerUpdateEventArgs>("VOICE_SERVER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildMembersChunk = new AsyncEvent<DiscordClient, GuildMembersChunkEventArgs>("GUILD_MEMBERS_CHUNKED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._unknownEvent = new AsyncEvent<DiscordClient, UnknownEventArgs>("UNKNOWN_EVENT", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionAdded = new AsyncEvent<DiscordClient, MessageReactionAddEventArgs>("MESSAGE_REACTION_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemoved = new AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>("MESSAGE_REACTION_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionsCleared = new AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>("MESSAGE_REACTIONS_CLEARED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._messageReactionRemovedEmoji = new AsyncEvent<DiscordClient, MessageReactionRemoveEmojiEventArgs>("MESSAGE_REACTION_REMOVED_EMOJI", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._webhooksUpdated = new AsyncEvent<DiscordClient, WebhooksUpdateEventArgs>("WEBHOOKS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._heartbeated = new AsyncEvent<DiscordClient, HeartbeatEventArgs>("HEARTBEATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandCreated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandUpdated = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandDeleted = new AsyncEvent<DiscordClient, ApplicationCommandEventArgs>("APPLICATION_COMMAND_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildApplicationCommandCountUpdated = new AsyncEvent<DiscordClient, GuildApplicationCommandCountEventArgs>("GUILD_APPLICATION_COMMAND_COUNTS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._applicationCommandPermissionsUpdated = new AsyncEvent<DiscordClient, ApplicationCommandPermissionsUpdateEventArgs>("APPLICATION_COMMAND_PERMISSIONS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationCreated = new AsyncEvent<DiscordClient, GuildIntegrationCreateEventArgs>("INTEGRATION_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationUpdated = new AsyncEvent<DiscordClient, GuildIntegrationUpdateEventArgs>("INTEGRATION_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildIntegrationDeleted = new AsyncEvent<DiscordClient, GuildIntegrationDeleteEventArgs>("INTEGRATION_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceCreated = new AsyncEvent<DiscordClient, StageInstanceCreateEventArgs>("STAGE_INSTANCE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceUpdated = new AsyncEvent<DiscordClient, StageInstanceUpdateEventArgs>("STAGE_INSTANCE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._stageInstanceDeleted = new AsyncEvent<DiscordClient, StageInstanceDeleteEventArgs>("STAGE_INSTANCE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadCreated = new AsyncEvent<DiscordClient, ThreadCreateEventArgs>("THREAD_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadUpdated = new AsyncEvent<DiscordClient, ThreadUpdateEventArgs>("THREAD_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadDeleted = new AsyncEvent<DiscordClient, ThreadDeleteEventArgs>("THREAD_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadListSynced = new AsyncEvent<DiscordClient, ThreadListSyncEventArgs>("THREAD_LIST_SYNCED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadMemberUpdated = new AsyncEvent<DiscordClient, ThreadMemberUpdateEventArgs>("THREAD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._threadMembersUpdated = new AsyncEvent<DiscordClient, ThreadMembersUpdateEventArgs>("THREAD_MEMBERS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._zombied = new AsyncEvent<DiscordClient, ZombiedEventArgs>("ZOMBIED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._payloadReceived = new AsyncEvent<DiscordClient, PayloadReceivedEventArgs>("PAYLOAD_RECEIVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventCreated = new AsyncEvent<DiscordClient, GuildScheduledEventCreateEventArgs>("GUILD_SCHEDULED_EVENT_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUpdated = new AsyncEvent<DiscordClient, GuildScheduledEventUpdateEventArgs>("GUILD_SCHEDULED_EVENT_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventDeleted = new AsyncEvent<DiscordClient, GuildScheduledEventDeleteEventArgs>("GUILD_SCHEDULED_EVENT_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUserAdded = new AsyncEvent<DiscordClient, GuildScheduledEventUserAddEventArgs>("GUILD_SCHEDULED_EVENT_USER_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._guildScheduledEventUserRemoved = new AsyncEvent<DiscordClient, GuildScheduledEventUserRemoveEventArgs>("GUILD_SCHEDULED_EVENT_USER_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
            this._embeddedActivityUpdated = new AsyncEvent<DiscordClient, EmbeddedActivityUpdateEventArgs>("EMBEDDED_ACTIVITY_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
        }

        /// <summary>
        /// Hooks the event handlers.
        /// </summary>
        /// <param name="Client">The client.</param>
        private void HookEventHandlers(DiscordClient Client)
        {
            Client.ClientErrored += this.Client_ClientError;
            Client.SocketErrored += this.Client_SocketError;
            Client.SocketOpened += this.Client_SocketOpened;
            Client.SocketClosed += this.Client_SocketClosed;
            Client.Ready += this.Client_Ready;
            Client.Resumed += this.Client_Resumed;
            Client.ChannelCreated += this.Client_ChannelCreated;
            Client.ChannelUpdated += this.Client_ChannelUpdated;
            Client.ChannelDeleted += this.Client_ChannelDeleted;
            Client.DmChannelDeleted += this.Client_DMChannelDeleted;
            Client.ChannelPinsUpdated += this.Client_ChannelPinsUpdated;
            Client.GuildCreated += this.Client_GuildCreated;
            Client.GuildAvailable += this.Client_GuildAvailable;
            Client.GuildUpdated += this.Client_GuildUpdated;
            Client.GuildDeleted += this.Client_GuildDeleted;
            Client.GuildUnavailable += this.Client_GuildUnavailable;
            Client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;
            Client.InviteCreated += this.Client_InviteCreated;
            Client.InviteDeleted += this.Client_InviteDeleted;
            Client.MessageCreated += this.Client_MessageCreated;
            Client.PresenceUpdated += this.Client_PresenceUpdate;
            Client.GuildBanAdded += this.Client_GuildBanAdd;
            Client.GuildBanRemoved += this.Client_GuildBanRemove;
            Client.GuildEmojisUpdated += this.Client_GuildEmojisUpdate;
            Client.GuildStickersUpdated += this.Client_GuildStickersUpdate;
            Client.GuildIntegrationsUpdated += this.Client_GuildIntegrationsUpdate;
            Client.GuildMemberAdded += this.Client_GuildMemberAdd;
            Client.GuildMemberRemoved += this.Client_GuildMemberRemove;
            Client.GuildMemberUpdated += this.Client_GuildMemberUpdate;
            Client.GuildRoleCreated += this.Client_GuildRoleCreate;
            Client.GuildRoleUpdated += this.Client_GuildRoleUpdate;
            Client.GuildRoleDeleted += this.Client_GuildRoleDelete;
            Client.MessageUpdated += this.Client_MessageUpdate;
            Client.MessageDeleted += this.Client_MessageDelete;
            Client.MessagesBulkDeleted += this.Client_MessageBulkDelete;
            Client.InteractionCreated += this.Client_InteractionCreate;
            Client.ComponentInteractionCreated += this.Client_ComponentInteractionCreate;
            Client.ContextMenuInteractionCreated += this.Client_ContextMenuInteractionCreate;
            Client.TypingStarted += this.Client_TypingStart;
            Client.UserSettingsUpdated += this.Client_UserSettingsUpdate;
            Client.UserUpdated += this.Client_UserUpdate;
            Client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
            Client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
            Client.GuildMembersChunked += this.Client_GuildMembersChunk;
            Client.UnknownEvent += this.Client_UnknownEvent;
            Client.MessageReactionAdded += this.Client_MessageReactionAdd;
            Client.MessageReactionRemoved += this.Client_MessageReactionRemove;
            Client.MessageReactionsCleared += this.Client_MessageReactionRemoveAll;
            Client.MessageReactionRemovedEmoji += this.Client_MessageReactionRemovedEmoji;
            Client.WebhooksUpdated += this.Client_WebhooksUpdate;
            Client.Heartbeated += this.Client_HeartBeated;
            Client.ApplicationCommandCreated += this.Client_ApplicationCommandCreated;
            Client.ApplicationCommandUpdated += this.Client_ApplicationCommandUpdated;
            Client.ApplicationCommandDeleted += this.Client_ApplicationCommandDeleted;
            Client.GuildApplicationCommandCountUpdated += this.Client_GuildApplicationCommandCountUpdated;
            Client.ApplicationCommandPermissionsUpdated += this.Client_ApplicationCommandPermissionsUpdated;
            Client.GuildIntegrationCreated += this.Client_GuildIntegrationCreated;
            Client.GuildIntegrationUpdated += this.Client_GuildIntegrationUpdated;
            Client.GuildIntegrationDeleted += this.Client_GuildIntegrationDeleted;
            Client.StageInstanceCreated += this.Client_StageInstanceCreated;
            Client.StageInstanceUpdated += this.Client_StageInstanceUpdated;
            Client.StageInstanceDeleted += this.Client_StageInstanceDeleted;
            Client.ThreadCreated += this.Client_ThreadCreated;
            Client.ThreadUpdated += this.Client_ThreadUpdated;
            Client.ThreadDeleted += this.Client_ThreadDeleted;
            Client.ThreadListSynced += this.Client_ThreadListSynced;
            Client.ThreadMemberUpdated += this.Client_ThreadMemberUpdated;
            Client.ThreadMembersUpdated += this.Client_ThreadMembersUpdated;
            Client.Zombied += this.Client_Zombied;
            Client.PayloadReceived += this.Client_PayloadReceived;
            Client.GuildScheduledEventCreated += this.Client_GuildScheduledEventCreated;
            Client.GuildScheduledEventUpdated += this.Client_GuildScheduledEventUpdated;
            Client.GuildScheduledEventDeleted += this.Client_GuildScheduledEventDeleted;
            Client.GuildScheduledEventUserAdded += this.Client_GuildScheduledEventUserAdded; ;
            Client.GuildScheduledEventUserRemoved += this.Client_GuildScheduledEventUserRemoved;
            Client.EmbeddedActivityUpdated += this.Client_EmbeddedActivityUpdated;
        }

        /// <summary>
        /// Unhooks the event handlers.
        /// </summary>
        /// <param name="Client">The client.</param>
        private void UnhookEventHandlers(DiscordClient Client)
        {
            Client.ClientErrored -= this.Client_ClientError;
            Client.SocketErrored -= this.Client_SocketError;
            Client.SocketOpened -= this.Client_SocketOpened;
            Client.SocketClosed -= this.Client_SocketClosed;
            Client.Ready -= this.Client_Ready;
            Client.Resumed -= this.Client_Resumed;
            Client.ChannelCreated -= this.Client_ChannelCreated;
            Client.ChannelUpdated -= this.Client_ChannelUpdated;
            Client.ChannelDeleted -= this.Client_ChannelDeleted;
            Client.DmChannelDeleted -= this.Client_DMChannelDeleted;
            Client.ChannelPinsUpdated -= this.Client_ChannelPinsUpdated;
            Client.GuildCreated -= this.Client_GuildCreated;
            Client.GuildAvailable -= this.Client_GuildAvailable;
            Client.GuildUpdated -= this.Client_GuildUpdated;
            Client.GuildDeleted -= this.Client_GuildDeleted;
            Client.GuildUnavailable -= this.Client_GuildUnavailable;
            Client.GuildDownloadCompleted -= this.Client_GuildDownloadCompleted;
            Client.InviteCreated -= this.Client_InviteCreated;
            Client.InviteDeleted -= this.Client_InviteDeleted;
            Client.MessageCreated -= this.Client_MessageCreated;
            Client.PresenceUpdated -= this.Client_PresenceUpdate;
            Client.GuildBanAdded -= this.Client_GuildBanAdd;
            Client.GuildBanRemoved -= this.Client_GuildBanRemove;
            Client.GuildEmojisUpdated -= this.Client_GuildEmojisUpdate;
            Client.GuildStickersUpdated -= this.Client_GuildStickersUpdate;
            Client.GuildIntegrationsUpdated -= this.Client_GuildIntegrationsUpdate;
            Client.GuildMemberAdded -= this.Client_GuildMemberAdd;
            Client.GuildMemberRemoved -= this.Client_GuildMemberRemove;
            Client.GuildMemberUpdated -= this.Client_GuildMemberUpdate;
            Client.GuildRoleCreated -= this.Client_GuildRoleCreate;
            Client.GuildRoleUpdated -= this.Client_GuildRoleUpdate;
            Client.GuildRoleDeleted -= this.Client_GuildRoleDelete;
            Client.MessageUpdated -= this.Client_MessageUpdate;
            Client.MessageDeleted -= this.Client_MessageDelete;
            Client.MessagesBulkDeleted -= this.Client_MessageBulkDelete;
            Client.InteractionCreated -= this.Client_InteractionCreate;
            Client.ComponentInteractionCreated -= this.Client_ComponentInteractionCreate;
            Client.ContextMenuInteractionCreated -= this.Client_ContextMenuInteractionCreate;
            Client.TypingStarted -= this.Client_TypingStart;
            Client.UserSettingsUpdated -= this.Client_UserSettingsUpdate;
            Client.UserUpdated -= this.Client_UserUpdate;
            Client.VoiceStateUpdated -= this.Client_VoiceStateUpdate;
            Client.VoiceServerUpdated -= this.Client_VoiceServerUpdate;
            Client.GuildMembersChunked -= this.Client_GuildMembersChunk;
            Client.UnknownEvent -= this.Client_UnknownEvent;
            Client.MessageReactionAdded -= this.Client_MessageReactionAdd;
            Client.MessageReactionRemoved -= this.Client_MessageReactionRemove;
            Client.MessageReactionsCleared -= this.Client_MessageReactionRemoveAll;
            Client.MessageReactionRemovedEmoji -= this.Client_MessageReactionRemovedEmoji;
            Client.WebhooksUpdated -= this.Client_WebhooksUpdate;
            Client.Heartbeated -= this.Client_HeartBeated;
            Client.ApplicationCommandCreated -= this.Client_ApplicationCommandCreated;
            Client.ApplicationCommandUpdated -= this.Client_ApplicationCommandUpdated;
            Client.ApplicationCommandDeleted -= this.Client_ApplicationCommandDeleted;
            Client.GuildApplicationCommandCountUpdated -= this.Client_GuildApplicationCommandCountUpdated;
            Client.ApplicationCommandPermissionsUpdated -= this.Client_ApplicationCommandPermissionsUpdated;
            Client.GuildIntegrationCreated -= this.Client_GuildIntegrationCreated;
            Client.GuildIntegrationUpdated -= this.Client_GuildIntegrationUpdated;
            Client.GuildIntegrationDeleted -= this.Client_GuildIntegrationDeleted;
            Client.StageInstanceCreated -= this.Client_StageInstanceCreated;
            Client.StageInstanceUpdated -= this.Client_StageInstanceUpdated;
            Client.StageInstanceDeleted -= this.Client_StageInstanceDeleted;
            Client.ThreadCreated -= this.Client_ThreadCreated;
            Client.ThreadUpdated -= this.Client_ThreadUpdated;
            Client.ThreadDeleted -= this.Client_ThreadDeleted;
            Client.ThreadListSynced -= this.Client_ThreadListSynced;
            Client.ThreadMemberUpdated -= this.Client_ThreadMemberUpdated;
            Client.ThreadMembersUpdated -= this.Client_ThreadMembersUpdated;
            Client.Zombied -= this.Client_Zombied;
            Client.PayloadReceived -= this.Client_PayloadReceived;
            Client.GuildScheduledEventCreated -= this.Client_GuildScheduledEventCreated;
            Client.GuildScheduledEventUpdated -= this.Client_GuildScheduledEventUpdated;
            Client.GuildScheduledEventDeleted -= this.Client_GuildScheduledEventDeleted;
            Client.GuildScheduledEventUserAdded -= this.Client_GuildScheduledEventUserAdded; ;
            Client.GuildScheduledEventUserRemoved -= this.Client_GuildScheduledEventUserRemoved;
            Client.EmbeddedActivityUpdated -= this.Client_EmbeddedActivityUpdated;
        }

        /// <summary>
        /// Gets the shard id from guilds.
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns>An int.</returns>
        private int GetShardIdFromGuilds(ulong Id)
        {
            foreach (var s in this._shards.Values)
            {
                if (s._guilds.TryGetValue(Id, out _))
                {
                    return s.ShardId;
                }
            }

            return -1;
        }

        #endregion

        #region Destructor

        ~DiscordShardedClient()
            => this.InternalStop(false).GetAwaiter().GetResult();

        #endregion
    }
}
