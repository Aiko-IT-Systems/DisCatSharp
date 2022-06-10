// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

namespace DisCatSharp;

/// <summary>
/// A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient : BaseDiscordClient
{
	#region Internal Fields/Properties

	internal bool IsShard = false;
	/// <summary>
	/// Gets the message cache.
	/// </summary>
	internal RingBuffer<DiscordMessage> MessageCache { get; }

	private List<BaseExtension> _extensions = new();
	private StatusUpdate _status;

	/// <summary>
	/// Gets the connection lock.
	/// </summary>
	private readonly ManualResetEventSlim _connectionLock = new(true);

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
	internal ConcurrentDictionary<ulong, DiscordGuild> GuildsInternal = new();

	/// <summary>
	/// Gets the websocket latency for this client.
	/// </summary>
	public int Ping
		=> Volatile.Read(ref this._ping);

	private int _ping;

	/// <summary>
	/// Gets the collection of presences held by this client.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordPresence> Presences
		=> this._presencesLazy.Value;

	internal Dictionary<ulong, DiscordPresence> PresencesInternal = new();
	private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;

	/// <summary>
	/// Gets the collection of presences held by this client.
	/// </summary>
	public IReadOnlyDictionary<string, DiscordActivity> EmbeddedActivities
		=> this._embeddedActivitiesLazy.Value;

	internal Dictionary<string, DiscordActivity> EmbeddedActivitiesInternal = new();
	private Lazy<IReadOnlyDictionary<string, DiscordActivity>> _embeddedActivitiesLazy;
	#endregion

	#region Constructor/Internal Setup

	/// <summary>
	/// Initializes a new instance of <see cref="DiscordClient"/>.
	/// </summary>
	/// <param name="config">Specifies configuration parameters.</param>
	public DiscordClient(DiscordConfiguration config)
		: base(config)
	{
		if (this.Configuration.MessageCacheSize > 0)
		{
			var intents = this.Configuration.Intents;
			this.MessageCache = intents.HasIntent(DiscordIntents.GuildMessages) || intents.HasIntent(DiscordIntents.DirectMessages)
					? new RingBuffer<DiscordMessage>(this.Configuration.MessageCacheSize)
					: null;
		}

		this.InternalSetup();

		this.Guilds = new ReadOnlyConcurrentDictionary<ulong, DiscordGuild>(this.GuildsInternal);
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
		this._guildMemberTimeoutAdded = new AsyncEvent<DiscordClient, GuildMemberTimeoutAddEventArgs>("GUILD_MEMBER_TIMEOUT_ADDED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutChanged = new AsyncEvent<DiscordClient, GuildMemberTimeoutUpdateEventArgs>("GUILD_MEMBER_TIMEOUT_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutRemoved = new AsyncEvent<DiscordClient, GuildMemberTimeoutRemoveEventArgs>("GUILD_MEMBER_TIMEOUT_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._rateLimitHit = new AsyncEvent<DiscordClient, RateLimitExceptionEventArgs>("RATELIMIT_HIT", EventExecutionLimit, this.EventErrorHandler);

		this.GuildsInternal.Clear();

		this._presencesLazy = new Lazy<IReadOnlyDictionary<ulong, DiscordPresence>>(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this.PresencesInternal));
		this._embeddedActivitiesLazy = new Lazy<IReadOnlyDictionary<string, DiscordActivity>>(() => new ReadOnlyDictionary<string, DiscordActivity>(this.EmbeddedActivitiesInternal));
	}

	#endregion

	#region Client Extension Methods

	/// <summary>
	/// Registers an extension with this client.
	/// </summary>
	/// <param name="ext">Extension to register.</param>
	public void AddExtension(BaseExtension ext)
	{
		ext.Setup(this);
		this._extensions.Add(ext);
	}

	/// <summary>
	/// Retrieves a previously registered extension from this client.
	/// </summary>
	/// <typeparam name="T">The type of extension to retrieve.</typeparam>
	/// <returns>The requested extension.</returns>
	public T GetExtension<T>() where T : BaseExtension
		=> this._extensions.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

	#endregion

	#region Public Connection Methods

	/// <summary>
	/// Connects to the gateway.
	/// </summary>
	/// <param name="activity">The activity to set. Defaults to null.</param>
	/// <param name="status">The optional status to set. Defaults to null.</param>
	/// <param name="idlesince">Since when is the client performing the specified activity. Defaults to null.</param>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when an invalid token was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task ConnectAsync(DiscordActivity activity = null, UserStatus? status = null, DateTimeOffset? idlesince = null)
	{
		// Check if connection lock is already set, and set it if it isn't
		if (!this._connectionLock.Wait(0))
			throw new InvalidOperationException("This client is already connected.");
		this._connectionLock.Set();

		var w = 7500;
		var i = 5;
		var s = false;
		Exception cex = null;

		if (activity == null && status == null && idlesince == null)
			this._status = null;
		else
		{
			var sinceUnix = idlesince != null ? (long?)Utilities.GetUnixTime(idlesince.Value) : null;
			this._status = new StatusUpdate()
			{
				Activity = new TransportActivity(activity),
				Status = status ?? UserStatus.Online,
				IdleSince = sinceUnix,
				IsAfk = idlesince != null,
				ActivityInternal = activity
			};
		}

		if (!this.IsShard)
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
				FailConnection(this._connectionLock);
				throw new Exception("Authentication failed. Check your token and try again.", e);
			}
			catch (PlatformNotSupportedException)
			{
				FailConnection(this._connectionLock);
				throw;
			}
			catch (NotImplementedException)
			{
				FailConnection(this._connectionLock);
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
			this._connectionLock.Set();
			throw new Exception("Could not connect to Discord.", cex);
		}

		// non-closure, hence args
		static void FailConnection(ManualResetEventSlim cl) =>
			// unlock this (if applicable) so we can let others attempt to connect
			cl?.Set();
	}

	/// <summary>
	/// Reconnects to the gateway.
	/// </summary>
	/// <param name="startNewSession">Whether to start a new session.</param>
	public Task ReconnectAsync(bool startNewSession = false)
		=> this.InternalReconnectAsync(startNewSession, code: startNewSession ? 1000 : 4002);

	/// <summary>
	/// Disconnects from the gateway.
	/// </summary>
	public async Task DisconnectAsync()
	{
		this.Configuration.AutoReconnect = false;
		if (this.WebSocketClient != null)
			await this.WebSocketClient.DisconnectAsync().ConfigureAwait(false);
	}

	#endregion

	#region Public REST Methods

	/// <summary>
	/// Gets a user.
	/// </summary>
	/// <param name="userId">Id of the user</param>
	/// <param name="fetch">Whether to fetch the user again. Defaults to true.</param>
	/// <returns>The requested user.</returns>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordUser> GetUserAsync(ulong userId, bool fetch = true)
	{
		if (!fetch)
		{
			return this.TryGetCachedUserInternal(userId, out var usr) ? usr : new DiscordUser { Id = userId, Discord = this };
		}
		else
		{
			var usr = await this.ApiClient.GetUserAsync(userId).ConfigureAwait(false);
			usr = this.UserCache.AddOrUpdate(userId, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.BannerHash = usr.BannerHash;
				old.BannerColorInternal = usr.BannerColorInternal;
				old.Pronouns = usr.Pronouns;
				return old;
			});

			return usr;
		}
	}

	/// <summary>
	/// Gets a channel.
	/// </summary>
	/// <param name="id">The id of the channel to get.</param>
	/// <returns>The requested channel.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordChannel> GetChannelAsync(ulong id)
		=> this.InternalGetCachedChannel(id) ?? await this.ApiClient.GetChannelAsync(id).ConfigureAwait(false);

	/// <summary>
	/// Gets a thread.
	/// </summary>
	/// <param name="id">The id of the thread to get.</param>
	/// <returns>The requested thread.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordThreadChannel> GetThreadAsync(ulong id)
		=> this.InternalGetCachedThread(id) ?? await this.ApiClient.GetThreadAsync(id).ConfigureAwait(false);

	/// <summary>
	/// Sends a normal message.
	/// </summary>
	/// <param name="channel">The channel to send to.</param>
	/// <param name="content">The message content to send.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content)
		=> this.ApiClient.CreateMessageAsync(channel.Id, content, embeds: null, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message with an embed.
	/// </summary>
	/// <param name="channel">The channel to send to.</param>
	/// <param name="embed">The embed to attach to the message.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordEmbed embed)
		=> this.ApiClient.CreateMessageAsync(channel.Id, null, embed != null ? new[] { embed } : null, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message with content and an embed.
	/// </summary>
	/// <param name="channel">Channel to send to.</param>
	/// <param name="content">The message content to send.</param>
	/// <param name="embed">The embed to attach to the message.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, DiscordEmbed embed)
		=> this.ApiClient.CreateMessageAsync(channel.Id, content, embed != null ? new[] { embed } : null, sticker: null, replyMessageId: null, mentionReply: false, failOnInvalidReply: false);

	/// <summary>
	/// Sends a message with the <see cref="DisCatSharp.Entities.DiscordMessageBuilder"/>.
	/// </summary>
	/// <param name="channel">The channel to send the message to.</param>
	/// <param name="builder">The message builder.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is false and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordMessageBuilder builder)
		=> this.ApiClient.CreateMessageAsync(channel.Id, builder);

	/// <summary>
	/// Sends a message with an <see cref="System.Action{DiscordMessageBuilder}"/>.
	/// </summary>
	/// <param name="channel">The channel to send the message to.</param>
	/// <param name="action">The message builder.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.SendMessages"/> permission if TTS is false and <see cref="Permissions.SendTtsMessages"/> if TTS is true.</exception>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);

		return this.ApiClient.CreateMessageAsync(channel.Id, builder);
	}

	/// <summary>
	/// Creates a guild. This requires the bot to be in less than 10 guilds total.
	/// </summary>
	/// <param name="name">Name of the guild.</param>
	/// <param name="region">Voice region of the guild.</param>
	/// <param name="icon">Stream containing the icon for the guild.</param>
	/// <param name="verificationLevel">Verification level for the guild.</param>
	/// <param name="defaultMessageNotifications">Default message notification settings for the guild.</param>
	/// <param name="systemChannelFlags">System channel flags for the guild.</param>
	/// <returns>The created guild.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuild> CreateGuildAsync(string name, string region = null, Optional<Stream> icon = default, VerificationLevel? verificationLevel = null,
		DefaultMessageNotifications? defaultMessageNotifications = null, SystemChannelFlags? systemChannelFlags = null)
	{
		var iconb64 = ImageTool.Base64FromStream(icon);
		return this.ApiClient.CreateGuildAsync(name, region, iconb64, verificationLevel, defaultMessageNotifications, systemChannelFlags);
	}

	/// <summary>
	/// Creates a guild from a template. This requires the bot to be in less than 10 guilds total.
	/// </summary>
	/// <param name="code">The template code.</param>
	/// <param name="name">Name of the guild.</param>
	/// <param name="icon">Stream containing the icon for the guild.</param>
	/// <returns>The created guild.</returns>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuild> CreateGuildFromTemplateAsync(string code, string name, Optional<Stream> icon = default)
	{
		var iconb64 = ImageTool.Base64FromStream(icon);
		return this.ApiClient.CreateGuildFromTemplateAsync(code, name, iconb64);
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
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="routeParams">The route parameters.</param>
	/// <param name="jsonBody">The json body.</param>
	/// <param name="additionalHeaders">The additional headers.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit wait override.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the resource does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>A awaitable RestResponse</returns>
	[Obsolete("This is no longer needed. Use DiscordClient.RestClient instead.", false)]
	public async Task<RestResponse> ExecuteRawRequestAsync(RestRequestMethod method, string route, object routeParams, string jsonBody = null, Dictionary<string, string> additionalHeaders = null, double? ratelimitWaitOverride = null)
	{
		var bucket = this.ApiClient.Rest.GetBucket(method, route, routeParams, out var path);

		var url = Utilities.GetApiUriFor(path, this.Configuration);
		var res = await this.ApiClient.DoRequestAsync(this, bucket, url, method, route, additionalHeaders, DiscordJson.SerializeObject(jsonBody), ratelimitWaitOverride);

		return res;
	}

	/// <summary>
	/// Gets a guild.
	/// <para>Setting <paramref name="withCounts"/> to true will make a REST request.</para>
	/// </summary>
	/// <param name="id">The guild ID to search for.</param>
	/// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
	/// <returns>The requested Guild.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> GetGuildAsync(ulong id, bool? withCounts = null)
	{
		if (this.GuildsInternal.TryGetValue(id, out var guild) && (!withCounts.HasValue || !withCounts.Value))
			return guild;

		guild = await this.ApiClient.GetGuildAsync(id, withCounts).ConfigureAwait(false);
		var channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id).ConfigureAwait(false);
		foreach (var channel in channels) guild.ChannelsInternal[channel.Id] = channel;

		return guild;
	}

	/// <summary>
	/// Gets a guild preview.
	/// </summary>
	/// <param name="id">The guild ID.</param>
	/// <returns></returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id)
		=> this.ApiClient.GetGuildPreviewAsync(id);

	/// <summary>
	/// Gets an invite.
	/// </summary>
	/// <param name="code">The invite code.</param>
	/// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
	/// <param name="withExpiration">Whether to include the expiration date in the returned invite.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <returns>The requested Invite.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the invite does not exists.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null, bool? withExpiration = null, ulong? scheduledEventId = null)
		=> this.ApiClient.GetInviteAsync(code, withCounts, withExpiration, scheduledEventId);

	/// <summary>
	/// Gets a list of user connections.
	/// </summary>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
		=> this.ApiClient.GetUserConnectionsAsync();

	/// <summary>
	/// Gets a sticker.
	/// </summary>
	/// <returns>The requested sticker.</returns>
	/// <param name="id">The id of the sticker.</param>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the sticker does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordSticker> GetStickerAsync(ulong id)
		=> this.ApiClient.GetStickerAsync(id);


	/// <summary>
	/// Gets all nitro sticker packs.
	/// </summary>
	/// <returns>List of sticker packs.</returns>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
		=> this.ApiClient.GetStickerPacksAsync();


	/// <summary>
	/// Gets the In-App OAuth Url.
	/// </summary>
	/// <param name="scopes">Defaults to <see cref="DisCatSharp.Enums.OAuthScopes.BOT_DEFAULT"/>.</param>
	/// <param name="redir">Redirect Uri.</param>
	/// <param name="permissions">Defaults to <see cref="Permissions.None"/>.</param>
	/// <returns>The OAuth Url</returns>
	public Uri GetInAppOAuth(Permissions permissions = Permissions.None, OAuthScopes scopes = OAuthScopes.BOT_DEFAULT, string redir = null)
	{
		permissions &= PermissionMethods.FullPerms;
		// hey look, it's not all annoying and blue :P
		return new Uri(new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", this.CurrentApplication.Id.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", OAuth.ResolveScopes(scopes))
			.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
			.AddParameter("state", "")
			.AddParameter("redirect_uri", redir ?? "")
			.ToString());
	}

	/// <summary>
	/// Gets a webhook.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <returns>The requested webhook.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWebhook> GetWebhookAsync(ulong id)
		=> this.ApiClient.GetWebhookAsync(id);

	/// <summary>
	/// Gets a webhook.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <param name="token">The target webhook token.</param>
	/// <returns>The requested webhook.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
		=> this.ApiClient.GetWebhookWithTokenAsync(id, token);

	/// <summary>
	/// Updates current user's activity and status.
	/// </summary>
	/// <param name="activity">Activity to set.</param>
	/// <param name="userStatus">Status of the user.</param>
	/// <param name="idleSince">Since when is the client performing the specified activity.</param>
	/// <returns></returns>
	public Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
		=> this.InternalUpdateStatusAsync(activity, userStatus, idleSince);

	/// <summary>
	/// Edits current user.
	/// </summary>
	/// <param name="username">New username.</param>
	/// <param name="avatar">New avatar.</param>
	/// <returns>The modified user.</returns>
	/// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordUser> UpdateCurrentUserAsync(string username = null, Optional<Stream> avatar = default)
	{
		var av64 = ImageTool.Base64FromStream(avatar);

		var usr = await this.ApiClient.ModifyCurrentUserAsync(username, av64).ConfigureAwait(false);

		this.CurrentUser.Username = usr.Username;
		this.CurrentUser.Discriminator = usr.Discriminator;
		this.CurrentUser.AvatarHash = usr.AvatarHash;
		return this.CurrentUser;
	}

	/// <summary>
	/// Gets a guild template by the code.
	/// </summary>
	/// <param name="code">The code of the template.</param>
	/// <returns>The guild template for the code.</returns>
	/// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> GetTemplateAsync(string code)
		=> this.ApiClient.GetTemplateAsync(code);

	/// <summary>
	/// Gets all the global application commands for this application.
	/// </summary>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	/// <returns>A list of global application commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false) =>
		this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id, withLocalizations);

	/// <summary>
	/// Overwrites the existing global application commands. New commands are automatically created and missing commands are automatically deleted.
	/// </summary>
	/// <param name="commands">The list of commands to overwrite with.</param>
	/// <returns>The list of global commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
		this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

	/// <summary>
	/// Creates or overwrites a global application command.
	/// </summary>
	/// <param name="command">The command to create.</param>
	/// <returns>The created command.</returns>
	public Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
		this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

	/// <summary>
	/// Gets a global application command by its id.
	/// </summary>
	/// <param name="commandId">The id of the command to get.</param>
	/// <returns>The command with the id.</returns>
	public Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
		this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

	/// <summary>
	/// Edits a global application command.
	/// </summary>
	/// <param name="commandId">The id of the command to edit.</param>
	/// <param name="action">Action to perform.</param>
	/// <returns>The edited command.</returns>
	public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
	{
		var mdl = new ApplicationCommandEditModel();
		action(mdl);
		var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
		return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.NameLocalizations, mdl.DescriptionLocalizations, mdl.DefaultMemberPermissions, mdl.DmPermission, mdl.IsNsfw).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes a global application command.
	/// </summary>
	/// <param name="commandId">The id of the command to delete.</param>
	public Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
		this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

	/// <summary>
	/// Gets all the application commands for a guild.
	/// </summary>
	/// <param name="guildId">The id of the guild to get application commands for.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	/// <returns>A list of application commands in the guild.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId, bool withLocalizations = false) =>
		this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, withLocalizations);

	/// <summary>
	/// Overwrites the existing application commands in a guild. New commands are automatically created and missing commands are automatically deleted.
	/// </summary>
	/// <param name="guildId">The id of the guild.</param>
	/// <param name="commands">The list of commands to overwrite with.</param>
	/// <returns>The list of guild commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
		this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

	/// <summary>
	/// Creates or overwrites a guild application command.
	/// </summary>
	/// <param name="guildId">The id of the guild to create the application command in.</param>
	/// <param name="command">The command to create.</param>
	/// <returns>The created command.</returns>
	public Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
		this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

	/// <summary>
	/// Gets a application command in a guild by its id.
	/// </summary>
	/// <param name="guildId">The id of the guild the application command is in.</param>
	/// <param name="commandId">The id of the command to get.</param>
	/// <returns>The command with the id.</returns>
	public Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
		 this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

	/// <summary>
	/// Edits a application command in a guild.
	/// </summary>
	/// <param name="guildId">The id of the guild the application command is in.</param>
	/// <param name="commandId">The id of the command to edit.</param>
	/// <param name="action">Action to perform.</param>
	/// <returns>The edited command.</returns>
	public async Task<DiscordApplicationCommand> EditGuildApplicationCommandAsync(ulong guildId, ulong commandId, Action<ApplicationCommandEditModel> action)
	{
		var mdl = new ApplicationCommandEditModel();
		action(mdl);
		var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
		return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, guildId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.NameLocalizations, mdl.DescriptionLocalizations, mdl.DefaultMemberPermissions, mdl.DmPermission, mdl.IsNsfw).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes a application command in a guild.
	/// </summary>
	/// <param name="guildId">The id of the guild to delete the application command in.</param>
	/// <param name="commandId">The id of the command.</param>
	public Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
		this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

	/// <summary>
	/// Gets all command permissions for a guild.
	/// </summary>
	/// <param name="guildId">The target guild.</param>
	public Task<IReadOnlyList<DiscordGuildApplicationCommandPermission>> GetGuildApplicationCommandPermissionsAsync(ulong guildId) =>
		this.ApiClient.GetGuildApplicationCommandPermissionsAsync(this.CurrentApplication.Id, guildId);

	/// <summary>
	/// Gets the permissions for a guild command.
	/// </summary>
	/// <param name="guildId">The target guild.</param>
	/// <param name="commandId">The target command id.</param>
	public Task<DiscordGuildApplicationCommandPermission> GetApplicationCommandPermissionAsync(ulong guildId, ulong commandId) =>
		this.ApiClient.GetGuildApplicationCommandPermissionAsync(this.CurrentApplication.Id, guildId, commandId);

	#endregion

	#region Internal Caching Methods
	/// <summary>
	/// Gets the internal cached threads.
	/// </summary>
	/// <param name="threadId">The target thread id.</param>
	/// <returns>The requested thread.</returns>
	internal DiscordThreadChannel InternalGetCachedThread(ulong threadId)
	{
		foreach (var guild in this.Guilds.Values)
			if (guild.Threads.TryGetValue(threadId, out var foundThread))
				return foundThread;

		return null;
	}


	/// <summary>
	/// Gets the internal cached scheduled event.
	/// </summary>
	/// <param name="scheduledEventId">The target scheduled event id.</param>
	/// <returns>The requested scheduled event.</returns>
	internal DiscordScheduledEvent InternalGetCachedScheduledEvent(ulong scheduledEventId)
	{
		foreach (var guild in this.Guilds.Values)
			if (guild.ScheduledEvents.TryGetValue(scheduledEventId, out var foundScheduledEvent))
				return foundScheduledEvent;

		return null;
	}

	/// <summary>
	/// Gets the internal cached channel.
	/// </summary>
	/// <param name="channelId">The target channel id.</param>
	/// <returns>The requested channel.</returns>
	internal DiscordChannel InternalGetCachedChannel(ulong channelId)
	{
		foreach (var guild in this.Guilds.Values)
			if (guild.Channels.TryGetValue(channelId, out var foundChannel))
				return foundChannel;

		return null;
	}

	/// <summary>
	/// Gets the internal cached guild.
	/// </summary>
	/// <param name="guildId">The target guild id.</param>
	/// <returns>The requested guild.</returns>
	internal DiscordGuild InternalGetCachedGuild(ulong? guildId)
	{
		if (this.GuildsInternal != null && guildId.HasValue)
		{
			if (this.GuildsInternal.TryGetValue(guildId.Value, out var guild))
				return guild;
		}

		return null;
	}

	/// <summary>
	/// Updates a message.
	/// </summary>
	/// <param name="message">The message to update.</param>
	/// <param name="author">The author to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <param name="member">The member to update.</param>
	private void UpdateMessage(DiscordMessage message, TransportUser author, DiscordGuild guild, TransportMember member)
	{
		if (author != null)
		{
			var usr = new DiscordUser(author) { Discord = this };

			if (member != null)
				member.User = author;

			message.Author = this.UpdateUser(usr, guild?.Id, guild, member);
		}

		var channel = this.InternalGetCachedChannel(message.ChannelId);

		if (channel != null) return;

		channel = !message.GuildId.HasValue
			? new DiscordDmChannel
			{
				Id = message.ChannelId,
				Discord = this,
				Type = ChannelType.Private
			}
			: new DiscordChannel
			{
				Id = message.ChannelId,
				Discord = this
			};

		message.Channel = channel;
	}

	/// <summary>
	/// Updates a scheduled event.
	/// </summary>
	/// <param name="scheduledEvent">The scheduled event to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <returns>The updated scheduled event.</returns>
	private DiscordScheduledEvent UpdateScheduledEvent(DiscordScheduledEvent scheduledEvent, DiscordGuild guild)
	{
		if (scheduledEvent != null)
		{
			_ = guild.ScheduledEventsInternal.AddOrUpdate(scheduledEvent.Id, scheduledEvent, (id, old) =>
			{
				old.Discord = this;
				old.Description = scheduledEvent.Description;
				old.ChannelId = scheduledEvent.ChannelId;
				old.EntityId = scheduledEvent.EntityId;
				old.EntityType = scheduledEvent.EntityType;
				old.EntityMetadata = scheduledEvent.EntityMetadata;
				old.PrivacyLevel = scheduledEvent.PrivacyLevel;
				old.Name = scheduledEvent.Name;
				old.Status = scheduledEvent.Status;
				old.UserCount = scheduledEvent.UserCount;
				old.ScheduledStartTimeRaw = scheduledEvent.ScheduledStartTimeRaw;
				old.ScheduledEndTimeRaw = scheduledEvent.ScheduledEndTimeRaw;
				return old;
			});
		}

		return scheduledEvent;
	}

	/// <summary>
	/// Updates a user.
	/// </summary>
	/// <param name="usr">The user to update.</param>
	/// <param name="guildId">The guild id to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <param name="mbr">The member to update.</param>
	/// <returns>The updated user.</returns>
	private DiscordUser UpdateUser(DiscordUser usr, ulong? guildId, DiscordGuild guild, TransportMember mbr)
	{
		if (mbr != null)
		{
			if (mbr.User != null)
			{
				usr = new DiscordUser(mbr.User) { Discord = this };

				_ = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.BannerHash = usr.BannerHash;
					old.BannerColorInternal = usr.BannerColorInternal;
					old.Pronouns = usr.Pronouns;
					return old;
				});

				usr = new DiscordMember(mbr) { Discord = this, GuildId = guildId.Value };
			}

			var intents = this.Configuration.Intents;

			DiscordMember member = default;

			if (!intents.HasAllPrivilegedIntents() || guild.IsLarge) // we have the necessary privileged intents, no need to worry about caching here unless guild is large.
			{
				if (guild?.MembersInternal.TryGetValue(usr.Id, out member) == false)
				{
					if (intents.HasIntent(DiscordIntents.GuildMembers) || this.Configuration.AlwaysCacheMembers) // member can be updated by events, so cache it
					{
						guild.MembersInternal.TryAdd(usr.Id, (DiscordMember)usr);
					}
				}
				else if (intents.HasIntent(DiscordIntents.GuildPresences) || this.Configuration.AlwaysCacheMembers) // we can attempt to update it if it's already in cache.
				{
					if (!intents.HasIntent(DiscordIntents.GuildMembers)) // no need to update if we already have the member events
					{
						_ = guild.MembersInternal.TryUpdate(usr.Id, (DiscordMember)usr, member);
					}
				}
			}
		}
		else if (usr.Username != null) // check if not a skeleton user
		{
			_ = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.BannerHash = usr.BannerHash;
				old.BannerColorInternal = usr.BannerColorInternal;
				old.Pronouns = usr.Pronouns;
				return old;
			});
		}

		return usr;
	}

	/// <summary>
	/// Updates the cached scheduled events in a guild.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="rawEvents">The raw events.</param>
	private void UpdateCachedScheduledEvent(DiscordGuild guild, JArray rawEvents)
	{
		if (this._disposed)
			return;

		if (rawEvents != null)
		{
			guild.ScheduledEventsInternal.Clear();

			foreach (var xj in rawEvents)
			{
				var xtm = xj.ToDiscordObject<DiscordScheduledEvent>();

				xtm.Discord = this;

				guild.ScheduledEventsInternal[xtm.Id] = xtm;
			}
		}
	}

	/// <summary>
	/// Updates the cached guild.
	/// </summary>
	/// <param name="newGuild">The new guild.</param>
	/// <param name="rawMembers">The raw members.</param>
	private void UpdateCachedGuild(DiscordGuild newGuild, JArray rawMembers)
	{
		if (this._disposed)
			return;

		if (!this.GuildsInternal.ContainsKey(newGuild.Id))
			this.GuildsInternal[newGuild.Id] = newGuild;

		var guild = this.GuildsInternal[newGuild.Id];

		if (newGuild.ChannelsInternal != null && !newGuild.ChannelsInternal.IsEmpty)
		{
			foreach (var channel in newGuild.ChannelsInternal.Values)
			{
				if (guild.ChannelsInternal.TryGetValue(channel.Id, out _)) continue;

				foreach (var overwrite in channel.PermissionOverwritesInternal)
				{
					overwrite.Discord = this;
					overwrite.ChannelId = channel.Id;
				}

				guild.ChannelsInternal[channel.Id] = channel;
			}
		}

		if (newGuild.ThreadsInternal != null && !newGuild.ThreadsInternal.IsEmpty)
		{
			foreach (var thread in newGuild.ThreadsInternal.Values)
			{
				if (guild.ThreadsInternal.TryGetValue(thread.Id, out _)) continue;

				guild.ThreadsInternal[thread.Id] = thread;
			}
		}

		if (newGuild.ScheduledEventsInternal != null && !newGuild.ScheduledEventsInternal.IsEmpty)
		{
			foreach (var @event in newGuild.ScheduledEventsInternal.Values)
			{
				if (guild.ScheduledEventsInternal.TryGetValue(@event.Id, out _)) continue;

				guild.ScheduledEventsInternal[@event.Id] = @event;
			}
		}

		foreach (var newEmoji in newGuild.EmojisInternal.Values)
			_ = guild.EmojisInternal.GetOrAdd(newEmoji.Id, _ => newEmoji);

		foreach (var newSticker in newGuild.StickersInternal.Values)
			_ = guild.StickersInternal.GetOrAdd(newSticker.Id, _ => newSticker);

		foreach (var newStageInstance in newGuild.StageInstancesInternal.Values)
			_ = guild.StageInstancesInternal.GetOrAdd(newStageInstance.Id, _ => newStageInstance);

		if (rawMembers != null)
		{
			guild.MembersInternal.Clear();

			foreach (var xj in rawMembers)
			{
				var xtm = xj.ToDiscordObject<TransportMember>();

				var xu = new DiscordUser(xtm.User) { Discord = this };
				_ = this.UserCache.AddOrUpdate(xtm.User.Id, xu, (id, old) =>
				{
					old.Username = xu.Username;
					old.Discriminator = xu.Discriminator;
					old.AvatarHash = xu.AvatarHash;
					old.PremiumType = xu.PremiumType;
					return old;
				});

				guild.MembersInternal[xtm.User.Id] = new DiscordMember(xtm) { Discord = this, GuildId = guild.Id };
			}
		}

		foreach (var role in newGuild.RolesInternal.Values)
		{
			if (guild.RolesInternal.TryGetValue(role.Id, out _)) continue;

			role.GuildId = guild.Id;
			guild.RolesInternal[role.Id] = role;
		}

		guild.Name = newGuild.Name;
		guild.AfkChannelId = newGuild.AfkChannelId;
		guild.AfkTimeout = newGuild.AfkTimeout;
		guild.DefaultMessageNotifications = newGuild.DefaultMessageNotifications;
		guild.RawFeatures = newGuild.RawFeatures;
		guild.IconHash = newGuild.IconHash;
		guild.MfaLevel = newGuild.MfaLevel;
		guild.OwnerId = newGuild.OwnerId;
		guild.VoiceRegionId = newGuild.VoiceRegionId;
		guild.SplashHash = newGuild.SplashHash;
		guild.VerificationLevel = newGuild.VerificationLevel;
		guild.WidgetEnabled = newGuild.WidgetEnabled;
		guild.WidgetChannelId = newGuild.WidgetChannelId;
		guild.ExplicitContentFilter = newGuild.ExplicitContentFilter;
		guild.PremiumTier = newGuild.PremiumTier;
		guild.PremiumSubscriptionCount = newGuild.PremiumSubscriptionCount;
		guild.PremiumProgressBarEnabled = newGuild.PremiumProgressBarEnabled;
		guild.BannerHash = newGuild.BannerHash;
		guild.Description = newGuild.Description;
		guild.VanityUrlCode = newGuild.VanityUrlCode;
		guild.SystemChannelId = newGuild.SystemChannelId;
		guild.SystemChannelFlags = newGuild.SystemChannelFlags;
		guild.DiscoverySplashHash = newGuild.DiscoverySplashHash;
		guild.MaxMembers = newGuild.MaxMembers;
		guild.MaxPresences = newGuild.MaxPresences;
		guild.ApproximateMemberCount = newGuild.ApproximateMemberCount;
		guild.ApproximatePresenceCount = newGuild.ApproximatePresenceCount;
		guild.MaxVideoChannelUsers = newGuild.MaxVideoChannelUsers;
		guild.PreferredLocale = newGuild.PreferredLocale;
		guild.RulesChannelId = newGuild.RulesChannelId;
		guild.PublicUpdatesChannelId = newGuild.PublicUpdatesChannelId;
		guild.ApplicationId = newGuild.ApplicationId;

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
	/// <param name="message">The message.</param>
	/// <param name="author">The author.</param>
	/// <param name="member">The member.</param>
	private void PopulateMessageReactionsAndCache(DiscordMessage message, TransportUser author, TransportMember member)
	{
		var guild = message.Channel?.Guild ?? this.InternalGetCachedGuild(message.GuildId);

		this.UpdateMessage(message, author, guild, member);

		if (message.ReactionsInternal == null)
			message.ReactionsInternal = new List<DiscordReaction>();
		foreach (var xr in message.ReactionsInternal)
			xr.Emoji.Discord = this;

		if (this.Configuration.MessageCacheSize > 0 && message.Channel != null)
			this.MessageCache?.Add(message);
	}


	#endregion

	#region Disposal

	~DiscordClient()
	{
		this.Dispose();
	}

	/// <summary>
	/// Whether the client is disposed.
	/// </summary>

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

		this.GuildsInternal = null;
		this._heartbeatTask = null;
	}

	#endregion
}
