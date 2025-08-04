using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Sentry;

namespace DisCatSharp;

/// <summary>
///     A Discord API wrapper.
/// </summary>
public sealed partial class DiscordClient : BaseDiscordClient
{
	#region Internal Fields/Properties

	/// <summary>
	///     Gets whether this client is running as a shard.
	/// </summary>
	internal bool IsShard = false;

	/// <summary>
	///     Gets the message cache.
	/// </summary>
	internal RingBuffer<DiscordMessage>? MessageCache { get; } // TODO: Check usage

	/// <summary>
	///     Gets the list of extensions.
	/// </summary>
	private readonly List<BaseExtension> _extensions = [];

	/// <summary>
	///     Gets the status update.
	/// </summary>
	private StatusUpdate? _status;

	/// <summary>
	///     Gets the connection lock.
	/// </summary>
	private readonly ManualResetEventSlim _connectionLock = new(true);

	#endregion

	#region Public Fields/Properties

	/// <summary>
	///     Gets the gateway protocol version.
	/// </summary>
	public int GatewayVersion { get; internal set; }

	/// <summary>
	///     Gets the gateway session information for this client.
	/// </summary>
	public GatewayInfo? GatewayInfo { get; internal set; }

	/// <summary>
	///     Gets the gateway URL.
	/// </summary>
	public Uri GatewayUri { get; internal set; }

	/// <summary>
	///     Gets the total number of shards the bot is connected to.
	/// </summary>
	public int ShardCount => this.GatewayInfo?.ShardCount ?? this.Configuration.ShardCount;

	/// <summary>
	///     Gets the currently connected shard ID.
	/// </summary>
	public int ShardId
		=> this.Configuration.ShardId;

	/// <summary>
	///     Gets the intents configured for this client.
	/// </summary>
	public DiscordIntents Intents
		=> this.Configuration.Intents;

	/// <summary>
	///     Gets a dictionary of guilds that this client is in. The dictionary's key is the guild ID. Note that the
	///     guild objects in this dictionary will not be filled in if the specific guilds aren't available (the
	///     <see cref="GuildAvailable" /> or <see cref="GuildDownloadCompleted" /> events haven't been fired yet)
	/// </summary>
	public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

	/// <summary>
	///     Gets the guilds.
	/// </summary>
	internal readonly ConcurrentDictionary<ulong, DiscordGuild> GuildsInternal = [];

	/// <summary>
	///     Gets a dictionary of application emojis that this client has. The dictionary's key is the emoji ID.
	/// </summary>
	public override IReadOnlyDictionary<ulong, DiscordApplicationEmoji> Emojis { get; }

	/// <summary>
	///     Gets the application emojis.
	/// </summary>
	internal readonly ConcurrentDictionary<ulong, DiscordApplicationEmoji> EmojisInternal = [];

	/// <summary>
	///     Gets the websocket latency for this client.
	/// </summary>
	public int Ping
		=> Volatile.Read(ref this._ping);

	/// <summary>
	///     Gets the ping.
	/// </summary>
	private int _ping = 0;

	/// <summary>
	///     Gets the collection of presences held by this client.
	/// </summary>
	public IReadOnlyDictionary<ulong, DiscordPresence> Presences
		=> this._presencesLazy.Value;

	/// <summary>
	///     Gets the internal collection of presences.
	/// </summary>
	internal ConcurrentDictionary<ulong, DiscordPresence> PresencesInternal = [];

	/// <summary>
	///     Lazily gets the collection of presences held by this client.
	/// </summary>
	private Lazy<IReadOnlyDictionary<ulong, DiscordPresence>> _presencesLazy;

	/// <summary>
	///     Gets the collection of presences held by this client.
	/// </summary>
	public IReadOnlyDictionary<string, DiscordActivity> EmbeddedActivities
		=> this._embeddedActivitiesLazy.Value;

	/// <summary>
	///     Gets the internal collection of embedded activities.
	/// </summary>
	internal ConcurrentDictionary<string, DiscordActivity> EmbeddedActivitiesInternal = [];

	/// <summary>
	///     Lazily gets the collection of embedded activities held by this client.
	/// </summary>
	private Lazy<IReadOnlyDictionary<string, DiscordActivity>> _embeddedActivitiesLazy;

	/// <summary>
	///     Gets the cooldown buckets for commands.
	/// </summary>
	public ConcurrentDictionary<string, CooldownBucket> CommandCooldownBuckets { get; } = [];

	#endregion

	#region Constructor/Internal Setup

	/// <summary>
	///     Initializes a new instance of <see cref="DiscordClient" />.
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
		this.Emojis = new ReadOnlyConcurrentDictionary<ulong, DiscordApplicationEmoji>(this.EmojisInternal);
	}

	/// <summary>
	///     Internal setup of the Client.
	/// </summary>
	internal void InternalSetup()
	{
		this._clientErrored = new("CLIENT_ERRORED", EventExecutionLimit, this.Goof);
		this._socketErrored = new("SOCKET_ERRORED", EventExecutionLimit, this.Goof);
		this._socketOpened = new("SOCKET_OPENED", EventExecutionLimit, this.EventErrorHandler);
		this._socketClosed = new("SOCKET_CLOSED", EventExecutionLimit, this.EventErrorHandler);
		this.ReadyEv = new("READY", EventExecutionLimit, this.EventErrorHandler);
		this._resumed = new("RESUMED", EventExecutionLimit, this.EventErrorHandler);
		this._channelCreated = new("CHANNEL_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._channelUpdated = new("CHANNEL_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._channelDeleted = new("CHANNEL_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._dmChannelDeleted = new("DM_CHANNEL_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._channelPinsUpdated = new("CHANNEL_PINS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildCreated = new("GUILD_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildAvailable = new("GUILD_AVAILABLE", EventExecutionLimit, this.EventErrorHandler);
		this._guildUpdated = new("GUILD_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildDeleted = new("GUILD_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._guildUnavailable = new("GUILD_UNAVAILABLE", EventExecutionLimit, this.EventErrorHandler);
		this.GuildDownloadCompletedEv = new("GUILD_DOWNLOAD_COMPLETED", EventExecutionLimit, this.EventErrorHandler);
		this._inviteCreated = new("INVITE_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._inviteDeleted = new("INVITE_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._messageCreated = new("MESSAGE_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._presenceUpdated = new("PRESENCE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildBanAdded = new("GUILD_BAN_ADD", EventExecutionLimit, this.EventErrorHandler);
		this._guildBanRemoved = new("GUILD_BAN_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._guildEmojisUpdated = new("GUILD_EMOJI_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildStickersUpdated = new("GUILD_STICKER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationsUpdated = new("GUILD_INTEGRATIONS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberAdded = new("GUILD_MEMBER_ADD", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberRemoved = new("GUILD_MEMBER_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberUpdated = new("GUILD_MEMBER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleCreated = new("GUILD_ROLE_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleUpdated = new("GUILD_ROLE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleDeleted = new("GUILD_ROLE_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._messageAcknowledged = new("MESSAGE_ACKNOWLEDGED", EventExecutionLimit, this.EventErrorHandler);
		this._messageUpdated = new("MESSAGE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._messageDeleted = new("MESSAGE_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._messagesBulkDeleted = new("MESSAGE_BULK_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._interactionCreated = new("INTERACTION_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._componentInteractionCreated = new("COMPONENT_INTERACTED", EventExecutionLimit, this.EventErrorHandler);
		this._contextMenuInteractionCreated = new("CONTEXT_MENU_INTERACTED", EventExecutionLimit, this.EventErrorHandler);
		this._typingStarted = new("TYPING_STARTED", EventExecutionLimit, this.EventErrorHandler);
		this._userSettingsUpdated = new("USER_SETTINGS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._userUpdated = new("USER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._voiceStateUpdated = new("VOICE_STATE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._voiceServerUpdated = new("VOICE_SERVER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._voiceChannelEffectSend = new("VOICE_CHANNEL_EFFECT_SEND", EventExecutionLimit, this.EventErrorHandler);
		this._guildMembersChunked = new("GUILD_MEMBERS_CHUNKED", EventExecutionLimit, this.EventErrorHandler);
		this._unknownEvent = new("UNKNOWN_EVENT", EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionAdded = new("MESSAGE_REACTION_ADDED", EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionRemoved = new("MESSAGE_REACTION_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionsCleared = new("MESSAGE_REACTIONS_CLEARED", EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionRemovedEmoji = new("MESSAGE_REACTION_REMOVED_EMOJI", EventExecutionLimit, this.EventErrorHandler);
		this._webhooksUpdated = new("WEBHOOKS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._heartbeated = new("HEARTBEATED", EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandCreated = new("APPLICATION_COMMAND_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandUpdated = new("APPLICATION_COMMAND_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandDeleted = new("APPLICATION_COMMAND_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._guildApplicationCommandCountUpdated = new("GUILD_APPLICATION_COMMAND_COUNTS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandPermissionsUpdated = new("APPLICATION_COMMAND_PERMISSIONS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationCreated = new("INTEGRATION_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationUpdated = new("INTEGRATION_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationDeleted = new("INTEGRATION_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceCreated = new("STAGE_INSTANCE_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceUpdated = new("STAGE_INSTANCE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceDeleted = new("STAGE_INSTANCE_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._threadCreated = new("THREAD_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._threadUpdated = new("THREAD_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._threadDeleted = new("THREAD_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._threadListSynced = new("THREAD_LIST_SYNCED", EventExecutionLimit, this.EventErrorHandler);
		this._threadMemberUpdated = new("THREAD_MEMBER_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._threadMembersUpdated = new("THREAD_MEMBERS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._zombied = new("ZOMBIED", EventExecutionLimit, this.EventErrorHandler);
		this._payloadReceived = new("PAYLOAD_RECEIVED", EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventCreated = new("GUILD_SCHEDULED_EVENT_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUpdated = new("GUILD_SCHEDULED_EVENT_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventDeleted = new("GUILD_SCHEDULED_EVENT_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUserAdded = new("GUILD_SCHEDULED_EVENT_USER_ADDED", EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUserRemoved = new("GUILD_SCHEDULED_EVENT_USER_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._embeddedActivityUpdated = new("EMBEDDED_ACTIVITY_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutAdded = new("GUILD_MEMBER_TIMEOUT_ADDED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutChanged = new("GUILD_MEMBER_TIMEOUT_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutRemoved = new("GUILD_MEMBER_TIMEOUT_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this.RateLimitHitInternal = new("RATELIMIT_HIT", EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleCreated = new("AUTO_MODERATION_RULE_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleUpdated = new("AUTO_MODERATION_RULE_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleDeleted = new("AUTO_MODERATION_RULE_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._automodActionExecuted = new("AUTO_MODERATION_ACTION_EXECUTED", EventExecutionLimit, this.EventErrorHandler);
		this._guildAuditLogEntryCreated = new("GUILD_AUDIT_LOG_ENTRY_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._voiceChannelStatusUpdated = new("VOICE_CHANNEL_STATUS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._entitlementCreated = new("ENTITLEMENT_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._entitlementUpdated = new("ENTITLEMENT_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._entitlementDeleted = new("ENTITLEMENT_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._subscriptionCreated = new("SUBSCRIPTION_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._subscriptionUpdated = new("SUBSCRIPTION_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._messagePollVoteAdded = new("MESSAGE_POLL_VOTE_ADDED", EventExecutionLimit, this.EventErrorHandler);
		this._messagePollVoteRemoved = new("MESSAGE_POLL_VOTE_REMOVED", EventExecutionLimit, this.EventErrorHandler);
		this._guildSoundboardSoundCreated = new("GUILD_SOUNDBOARD_SOUND_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildSoundboardSoundUpdated = new("GUILD_SOUNDBOARD_SOUND_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildSoundboardSoundDeleted = new("GUILD_SOUNDBOARD_SOUND_DELETED", EventExecutionLimit, this.EventErrorHandler);
		this._guildSoundboardSoundsUpdated = new("GUILD_SOUNDBOARD_SOUNDS_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._soundboardSounds = new("SOUNDBOARD_SOUNDS", EventExecutionLimit, this.EventErrorHandler);
		this._guildJoinRequestCreated = new("GUILD_JOIN_REQUEST_CREATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildJoinRequestUpdated = new("GUILD_JOIN_REQUEST_UPDATED", EventExecutionLimit, this.EventErrorHandler);
		this._guildJoinRequestDeleted = new("GUILD_JOIN_REQUEST_DELETED", EventExecutionLimit, this.EventErrorHandler);

		this.GuildsInternal.Clear();
		this.EmojisInternal.Clear();

		this._presencesLazy = new(() => new ReadOnlyDictionary<ulong, DiscordPresence>(this.PresencesInternal));
		this._embeddedActivitiesLazy = new(() => new ReadOnlyDictionary<string, DiscordActivity>(this.EmbeddedActivitiesInternal));
	}

	#endregion

	#region Client Extension Methods

	/// <summary>
	///     Registers an extension with this client.
	/// </summary>
	/// <param name="ext">Extension to register.</param>
	public void AddExtension(BaseExtension ext)
	{
		ext.Setup(this);
		this._extensions.Add(ext);
	}

	/// <summary>
	///     Retrieves a previously registered extension from this client.
	/// </summary>
	/// <typeparam name="T">The type of extension to retrieve.</typeparam>
	/// <returns>The requested extension.</returns>
	public T? GetExtension<T>() where T : BaseExtension
		=> this._extensions.FirstOrDefault(x => x.GetType() == typeof(T)) as T;

	#endregion

	#region Public Connection Methods

	/// <summary>
	///     Connects to the gateway.
	/// </summary>
	/// <param name="activity">The activity to set. Defaults to null.</param>
	/// <param name="status">The optional status to set. Defaults to null.</param>
	/// <param name="idlesince">Since when is the client performing the specified activity. Defaults to null.</param>
	/// <exception cref="UnauthorizedException">Thrown when an invalid token was provided.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task ConnectAsync(DiscordActivity? activity = null, UserStatus? status = null, DateTimeOffset? idlesince = null)
	{
		// Check if connection lock is already set, and set it if it isn't
		if (!this._connectionLock.Wait(0))
			throw new InvalidOperationException("This client is already connected.");

		this._connectionLock.Set();

		var w = 7500;
		var i = 5;
		var s = false;
		Exception? cex = null;

		if (activity is null && status is null && idlesince is null)
			this._status = null;
		else
		{
			var sinceUnix = idlesince != null ? (long?)Utilities.GetUnixTime(idlesince.Value) : null;
			this._status = new()
			{
				Activities = activity is not null ? [new(activity)] : null,
				Status = status ?? UserStatus.Online,
				IdleSince = sinceUnix,
				IsAfk = idlesince is not null,
				ActivitiesInternal = activity is not null ? [activity] : null
			};
		}

		if (!this.IsShard)
		{
			if (this.Configuration.TokenType != TokenType.Bot)
				this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful");
			this.Logger.LogInformation(LoggerEvents.Startup, "Lib {LibraryName}, version {LibraryVersion}", this.BotLibrary, this.VersionString);
		}

		if (!this.Configuration.DisableUpdateCheck)
		{
			this.Logger.LogInformation("Checking versions..");
			await Utilities.CheckVersionAsync(this, true, this.IsShard, githubToken: this.Configuration.UpdateCheckGitHubToken, includePrerelease: this.Configuration.IncludePrereleaseInUpdateCheck, checkMode: this.Configuration.UpdateCheckMode);
			foreach (var extension in this._extensions.Where(extension => extension.HasVersionCheckSupport))
				await Utilities.CheckVersionAsync(this, true, this.IsShard, extension.RepositoryOwner, extension.Repository, extension.PackageId, extension.VersionString, this.Configuration.UpdateCheckGitHubToken, this.Configuration.IncludePrereleaseInUpdateCheck, this.Configuration.UpdateCheckMode);
			this.Logger.LogInformation("Done");
		}
		else
			this.Logger.LogInformation("Skipped version check");

		while (i-- > 0 || this.Configuration.ReconnectIndefinitely)
			try
			{
				await this.InternalConnectAsync().ConfigureAwait(false);
				s = true;
				break;
			}
			catch (UnauthorizedException e)
			{
				FailConnection(this._connectionLock);
				throw new("Authentication failed. Check your token and try again.", e);
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

				this.Logger.LogError(LoggerEvents.ConnectionFailure, ex, "Connection attempt failed, retrying in {Seconds}s", w / 1000);
				await Task.Delay(w, this._cancelToken).ConfigureAwait(false);

				if (i > 0)
					w *= 2;
			}

		if (!s && cex is not null)
		{
			this._connectionLock.Set();
			throw new("Could not connect to Discord.", cex);
		}

		if (this.Configuration is { AutoFetchSkuIds: false, AutoFetchApplicationEmojis: false })
			return;

		if (this.Configuration.AutoFetchSkuIds)
			try
			{
				var skus = await this.ApiClient.GetSkusAsync(this.CurrentApplication.Id).ConfigureAwait(false);
				if (!skus.Any())
					return;

				this.Configuration.SkuId = skus.FirstOrDefault(x => x.Type is SkuType.Subscription)?.Id;
			}
			catch (Exception ex)
			{
				this.Logger.LogError(LoggerEvents.Startup, ex, "Failed to fetch SKU IDs");
			}

		if (this.Configuration.AutoFetchApplicationEmojis)
			try
			{
				await this.ApiClient.GetApplicationEmojisAsync(this.CurrentApplication.Id);
			}
			catch (Exception ex)
			{
				this.Logger.LogError(LoggerEvents.Startup, ex, "Failed to fetch application emojis");
			}

		return;

		// non-closure, hence args
		static void FailConnection(ManualResetEventSlim? cl) =>
			// unlock this (if applicable) so we can let others attempt to connect
			cl?.Set();
	}

	/// <summary>
	///     Reconnects to the gateway.
	/// </summary>
	/// <param name="startNewSession">Whether to start a new session.</param>
	public Task ReconnectAsync(bool startNewSession = true)
		=> this.InternalReconnectAsync(startNewSession, startNewSession ? 1000 : 4002);

	/// <summary>
	///     Disconnects from the gateway.
	/// </summary>
	public async Task DisconnectAsync()
	{
		this.Configuration.AutoReconnect = false;
		if (this.WebSocketClient is not null)
			await this.WebSocketClient.DisconnectAsync().ConfigureAwait(false);
	}

	#endregion

	#region Public REST Methods

	/// <summary>
	///     Requests soundboard sounds over the gateway.
	/// </summary>
	/// <param name="guildIds">The guild ids to request sounds from.</param>
	public async Task RequestSoundboardSoundsAsync(IEnumerable<ulong> guildIds)
	{
		var payload = new DiscordDispatchPayload
		{
			OpCode = 31,
			Payload = new RequestSoundboardSoundsPayload
			{
				GuildIds = guildIds
			}
		};

		await this.WsSendAsync(DiscordJson.SerializeObject(payload)).ConfigureAwait(false);
	}

	/// <summary>
	///     Requests and waits for guild soundboard sounds.
	/// </summary>
	/// <param name="guildIds">The guild ids to request sounds from.</param>
	/// <returns>The requested <see cref="DiscordSoundboardSound" />'s as key-value-pair, where the key is a guild id.</returns>
	public async Task<IReadOnlyDictionary<ulong, IReadOnlyList<DiscordSoundboardSound>>> RequestAndWaitForSoundboardSoundsAsync(IEnumerable<ulong> guildIds)
	{
		var targetGuildIds = guildIds.ToList();

		Dictionary<ulong, IReadOnlyList<DiscordSoundboardSound>> guildSoundsKvp = [];
		this.SoundboardSounds += (_, e) =>
		{
			if (targetGuildIds.Contains(e.GuildId))
				guildSoundsKvp.TryAdd(e.GuildId, e.Sounds);
			return Task.CompletedTask;
		};

		await this.RequestSoundboardSoundsAsync(targetGuildIds);

		while (guildSoundsKvp.Keys.Count != targetGuildIds.Count)
			await Task.Delay(TimeSpan.FromSeconds(1), this._cancelToken);

		return guildSoundsKvp.AsReadOnly();
	}

	/// <summary>
	///     Gets a user.
	/// </summary>
	/// <param name="userId">Id of the user</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// <returns>The requested user.</returns>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordUser> GetUserAsync(ulong userId, bool fetch = false)
	{
		if (!fetch && this.TryGetCachedUserInternal(userId, out var cachedUsr))
			return cachedUsr;

		var usr = await this.ApiClient.GetUserAsync(userId).ConfigureAwait(false);
		usr = this.UserCache.AddOrUpdate(userId, usr, (id, old) =>
		{
			old.Username = usr.Username;
			old.Discriminator = usr.Discriminator;
			old.AvatarHash = usr.AvatarHash;
			old.BannerHash = usr.BannerHash;
			old.BannerColorInternal = usr.BannerColorInternal;
			old.AvatarDecorationData = usr.AvatarDecorationData;
			old.ThemeColorsInternal = usr.ThemeColorsInternal;
			old.Collectibles = usr.Collectibles;
			old.IsSystem = usr.IsSystem;
			old.IsBot = usr.IsBot;
			old.Pronouns = usr.Pronouns;
			old.GlobalName = usr.GlobalName;
			old.PrimaryGuild = usr.PrimaryGuild;
			return old;
		});

		return usr;
	}

	/// <summary>
	///     Gets a applications rpc information.
	/// </summary>
	/// <param name="applicationId">Id of the application</param>
	/// <returns>The requested application.</returns>
	/// <exception cref="NotFoundException">Thrown when the application does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordRpcApplication> GetRpcApplicationAsync(ulong applicationId)
		=> await this.ApiClient.GetApplicationRpcInfoAsync(applicationId).ConfigureAwait(false);

	/// <summary>
	///     Gets the current applications information.
	/// </summary>
	/// <returns>The requested application.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordApplication> GetCurrentApplicationInfoAsync()
	{
		var tapp = await this.ApiClient.GetCurrentApplicationInfoAsync().ConfigureAwait(false);
		return new(tapp);
	}

	/// <summary>
	///     Tries to get a user.
	/// </summary>
	/// <param name="userId">Id of the user.</param>
	/// <param name="user">The user, if found.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to true.</param>
	/// <returns>True if found, otherwise false.</returns>
	public bool TryGetUser(ulong userId, [NotNullWhen(true)] out DiscordUser? user, bool fetch = true)
	{
		try
		{
			user = this.GetUserAsync(userId, fetch).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			user = null;
			return false;
		}
	}

	/// <summary>
	///     Gets the published store sku listings (premium application subscription).
	/// </summary>
	/// <param name="applicationId">The application id to fetch the listings for.</param>
	/// <returns>A list of published listings with <see cref="DiscordStoreSku" />s.</returns>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<IReadOnlyList<DiscordStoreSku>> GetPublishedListingsAsync(ulong applicationId)
		=> await this.ApiClient.GetPublishedListingsAsync(applicationId).ConfigureAwait(false);

	/// <summary>
	///     Gets the applications skus.
	/// </summary>
	/// <returns>A list of published listings with <see cref="DiscordSku" />s.</returns>
	/// <exception cref="NotFoundException">Thrown when the skus do not exist.</exception>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<IReadOnlyList<DiscordSku>> GetSkusAsync()
		=> await this.ApiClient.GetSkusAsync(this.CurrentApplication.Id).ConfigureAwait(false);

	/// <summary>
	///     Gets the application's entitlements.
	/// </summary>
	/// <param name="guildId">Filter returned entitlements to a specific guild id.</param>
	/// <param name="userId">Filter returned entitlements to a specific user id.</param>
	/// <param name="skuIds">Optional list of SKU IDs to check entitlements for.</param>
	/// <param name="before">Retrieve entitlements before this entitlement ID.</param>
	/// <param name="after">Retrieve entitlements after this entitlement ID.</param>
	/// <param name="limit">Number of entitlements to return, 1-100, default 100.</param>
	/// <param name="excludeEnded">
	///     Whether or not ended entitlements should be omitted. Defaults to false, ended entitlements
	///     are included by default.
	/// </param>
	/// <param name="excludeDeleted">
	///     Whether or not deleted entitlements should be omitted. Defaults to true, deleted
	///     entitlements are not included by default.
	/// </param>
	/// <returns>A list of <see cref="DiscordEntitlement" />.</returns>
	/// <exception cref="NotFoundException">Thrown when the entitlements do not exist.</exception>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<IReadOnlyList<DiscordEntitlement>> GetEntitlementsAsync(ulong? guildId = null, ulong? userId = null, List<ulong>? skuIds = null, ulong? before = null, ulong? after = null, int limit = 100, bool? excludeEnded = null, bool? excludeDeleted = null)
		=> await this.ApiClient.GetEntitlementsAsync(this.CurrentApplication.Id, guildId, userId, skuIds, before, after, limit, excludeEnded, excludeDeleted).ConfigureAwait(false);

	/// <summary>
	///     Gets an entitlement.
	/// </summary>
	/// <param name="entitlementId">The entitlement id to fetch.</param>
	/// <returns>The requested <see cref="DiscordEntitlement" />.</returns>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<DiscordEntitlement?> GetEntitlementAsync(ulong entitlementId)
		=> await this.ApiClient.GetEntitlementAsync(this.CurrentApplication.Id, entitlementId).ConfigureAwait(false);

	/// <summary>
	///    Consumes an entitlement.
	/// </summary>
	/// <param name="entitlementId">The entitlement id to consume.</param>
	/// <returns>Whether the entitlement was consumed.</returns>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<bool> ConsumeEntitlementAsync(ulong entitlementId)
		=> await this.ApiClient.ConsumeEntitlementAsync(this.CurrentApplication.Id, entitlementId).ConfigureAwait(false);

	/// <summary>
	///     Gets the subscriptions of an sku.
	/// </summary>
	/// <param name="skuId">The sku id to fetch the subscriptions for.</param>
	/// <param name="userId">The user ID for which to return subscriptions. Required except for OAuth queries.</param>
	/// <param name="before">List subscriptions before this ID.</param>
	/// <param name="after">List subscriptions after this ID.</param>
	/// <param name="limit">Number of results to return (1-100).</param>
	/// <returns>A list of <see cref="DiscordSubscription" />.</returns>
	/// <exception cref="NotFoundException">Thrown when the subscriptions do not exist.</exception>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<IReadOnlyList<DiscordSubscription>> GetSkuSubscriptionsAsync(ulong skuId, ulong userId, ulong? before = null, ulong? after = null, int limit = 100)
		=> await this.ApiClient.GetSkuSubscriptionsAsync(skuId, userId, before, after, limit).ConfigureAwait(false);

	/// <summary>
	///     Gets a subscription of an sku.
	/// </summary>
	/// <param name="skuId">The sku id to fetch the subscription for.</param>
	/// <param name="subscriptionId">The subscription id to fetch.</param>
	/// <returns>The requested <see cref="DiscordSubscription" />.</returns>
	public async Task<DiscordSubscription?> GetSkuSubscriptionAsync(ulong skuId, ulong subscriptionId)
		=> await this.ApiClient.GetSkuSubscriptionAsync(skuId, subscriptionId).ConfigureAwait(false);

	/// <summary>
	///     Creates a test entitlement.
	/// </summary>
	/// <param name="skuId">The sku id to create the entitlement for.</param>
	/// <param name="ownerId">The owner id to create the entitlement for.</param>
	/// <param name="ownerType">The owner type to create the entitlement for.</param>
	/// <returns>A partial <see cref="DiscordEntitlement" />.</returns>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task<DiscordEntitlement> CreateTestEntitlementAsync(ulong skuId, ulong ownerId, EntitlementOwnerType ownerType)
		=> await this.ApiClient.CreateTestEntitlementAsync(this.CurrentApplication.Id, skuId, ownerId, ownerType).ConfigureAwait(false);

	/// <summary>
	///     Deletes a test entitlement.
	/// </summary>
	/// <param name="entitlementId">The entitlement id to delete.</param>
	[RequiresFeature(Features.MonetizedApplication)]
	public async Task DeleteTestEntitlementAsync(ulong entitlementId)
		=> await this.ApiClient.DeleteTestEntitlementAsync(this.CurrentApplication.Id, entitlementId).ConfigureAwait(false);

	/// <summary>
	///     Gets the applications role connection metadata.
	/// </summary>
	/// <returns>A list of metadata records or <see langword="null" />.</returns>
	public async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> GetRoleConnectionMetadata()
		=> await this.ApiClient.GetRoleConnectionMetadataRecords(this.CurrentApplication.Id).ConfigureAwait(false);

	/// <summary>
	///     Updates the applications role connection metadata.
	/// </summary>
	/// <param name="metadata">A list of metadata objects. Max 5.</param>
	public async Task<IReadOnlyList<DiscordApplicationRoleConnectionMetadata>> UpdateRoleConnectionMetadata(IEnumerable<DiscordApplicationRoleConnectionMetadata> metadata)
		=> await this.ApiClient.UpdateRoleConnectionMetadataRecords(this.CurrentApplication.Id, metadata).ConfigureAwait(false);

	/// <summary>
	///     Removes all global application commands.
	/// </summary>
	public async Task RemoveGlobalApplicationCommandsAsync()
		=> await this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);

	/// <summary>
	///     Removes all global application commands for a specific guild id.
	/// </summary>
	/// <param name="guildId">The target guild id.</param>
	public async Task RemoveGuildApplicationCommandsAsync(ulong guildId)
		=> await this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, Array.Empty<DiscordApplicationCommand>()).ConfigureAwait(false);

	/// <summary>
	///     Removes all global application commands for a specific guild.
	/// </summary>
	/// <param name="guild">The target guild.</param>
	public async Task RemoveGuildApplicationCommandsAsync(DiscordGuild guild)
		=> await this.RemoveGuildApplicationCommandsAsync(guild.Id).ConfigureAwait(false);

	/// <summary>
	///     Gets the application emojis.
	///     <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// </summary>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordApplicationEmoji>> GetApplicationEmojisAsync(bool fetch = false)
		=> (fetch ? null : this.InternalGetCachedApplicationEmojis()) ?? await this.ApiClient.GetApplicationEmojisAsync(this.CurrentApplication.Id).ConfigureAwait(false);

	/// <summary>
	///     Gets an application emoji.
	/// </summary>
	/// <param name="id">The emoji id.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordApplicationEmoji?> GetApplicationEmojiAsync(ulong id, bool fetch = false)
		=> (fetch ? null : this.InternalGetCachedApplicationEmoji(id)) ?? await this.ApiClient.GetApplicationEmojiAsync(this.CurrentApplication.Id, id).ConfigureAwait(false);

	/// <summary>
	///     Creates an application emoji.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="image">The image.</param>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordApplicationEmoji> CreateApplicationEmojiAsync(string name, Stream image)
	{
		var imageb64 = MediaTool.Base64FromStream(image);
		return await this.ApiClient.CreateApplicationEmojiAsync(this.CurrentApplication.Id, name, imageb64);
	}

	/// <summary>
	///     Modifies an application emoji.
	/// </summary>
	/// <param name="id">The emoji id.</param>
	/// <param name="name">The name.</param>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordApplicationEmoji> ModifyApplicationEmojiAsync(ulong id, string name)
		=> this.ApiClient.ModifyApplicationEmojiAsync(this.CurrentApplication.Id, id, name);

	/// <summary>
	///     Deletes an application emoji.
	/// </summary>
	/// <param name="id">The emoji id.</param>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteApplicationEmojiAsync(ulong id)
		=> this.ApiClient.DeleteApplicationEmojiAsync(this.CurrentApplication.Id, id);

	/// <summary>
	///     Gets a channel.
	/// </summary>
	/// <param name="id">The id of the channel to get.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// <returns>The requested channel.</returns>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordChannel> GetChannelAsync(ulong id, bool fetch = false)
		=> (fetch ? null : this.InternalGetCachedChannel(id)) ?? await this.ApiClient.GetChannelAsync(id).ConfigureAwait(false);

	/// <summary>
	///     Tries to get a channel.
	/// </summary>
	/// <param name="id">The id of the channel to get.</param>
	/// <param name="channel">The queried channel, if found.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to true.</param>
	/// <returns>True if channel found, otherwise false.</returns>
	public bool TryGetChannel(ulong id, [NotNullWhen(true)] out DiscordChannel? channel, bool fetch = true)
	{
		try
		{
			channel = this.GetChannelAsync(id, fetch).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			channel = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a thread.
	/// </summary>
	/// <param name="id">The id of the thread to get.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// <returns>The requested thread.</returns>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordXThreadChannel> GetThreadAsync(ulong id, bool fetch = false)
		=> (fetch ? null : this.InternalGetCachedThread(id)) ?? await this.ApiClient.GetThreadAsync(id).ConfigureAwait(false);

	/// <summary>
	///     Tries to get a thread.
	/// </summary>
	/// <param name="id">The id of the thread to get.</param>
	/// <param name="thread">The thread, if found.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to true.</param>
	/// <returns>True if found, otherwise false.</returns>
	public bool TryGetThread(ulong id, [NotNullWhen(true)] out DiscordXThreadChannel? thread, bool fetch = true)
	{
		try
		{
			thread = this.GetThreadAsync(id, fetch).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			thread = null;
			return false;
		}
	}

	/// <summary>
	///     Sends a normal message.
	/// </summary>
	/// <param name="channel">The channel to send to.</param>
	/// <param name="content">The message content to send.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content)
		=> this.ApiClient.CreateMessageAsync(channel.Id, content, null, null, null, false, false);

	/// <summary>
	///     Sends a message with an embed.
	/// </summary>
	/// <param name="channel">The channel to send to.</param>
	/// <param name="embed">The embed to attach to the message.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordEmbed embed)
		=> this.ApiClient.CreateMessageAsync(channel.Id, null, embed != null
			? new[] { embed }
			: null, null, null, false, false);

	/// <summary>
	///     Sends a message with content and an embed.
	/// </summary>
	/// <param name="channel">Channel to send to.</param>
	/// <param name="content">The message content to send.</param>
	/// <param name="embed">The embed to attach to the message.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public
		Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string content, DiscordEmbed embed)
		=> this.ApiClient.CreateMessageAsync(channel.Id, content, embed != null
			? new[] { embed }
			: null, null, null, false, false);

	/// <summary>
	///     Sends a message with the <see cref="DisCatSharp.Entities.DiscordMessageBuilder" />.
	/// </summary>
	/// <param name="channel">The channel to send the message to.</param>
	/// <param name="builder">The message builder.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission if TTS is false and <see cref="Permissions.SendTtsMessages" />
	///     if TTS is true.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public
		Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, DiscordMessageBuilder builder)
		=> this.ApiClient.CreateMessageAsync(channel.Id, builder);

	/// <summary>
	///     Sends a message with an <see cref="Action{DiscordMessageBuilder}" />.
	/// </summary>
	/// <param name="channel">The channel to send the message to.</param>
	/// <param name="action">The message builder.</param>
	/// <returns>The message that was sent.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.SendMessages" /> permission if TTS is false and <see cref="Permissions.SendTtsMessages" />
	///     if TTS is true.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, Action<DiscordMessageBuilder> action)
	{
		var builder = new DiscordMessageBuilder();
		action(builder);

		return this.ApiClient.CreateMessageAsync(channel.Id, builder);
	}

	/// <summary>
	///     Gets a guild.
	///     <para>Setting <paramref name="withCounts" /> to true will make a REST request.</para>
	/// </summary>
	/// <param name="id">The guild ID to search for.</param>
	/// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to false.</param>
	/// <returns>The requested Guild.</returns>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> GetGuildAsync(ulong id, bool? withCounts = null, bool fetch = false)
	{
		if (!fetch && this.GuildsInternal.TryGetValue(id, out var guild) && (!withCounts.HasValue || !withCounts.Value))
			return guild;

		guild = await this.ApiClient.GetGuildAsync(id, withCounts).ConfigureAwait(false);
		var channels = await this.ApiClient.GetGuildChannelsAsync(guild.Id).ConfigureAwait(false);
		foreach (var channel in channels) guild.ChannelsInternal[channel.Id] = channel;

		return guild;
	}

	/// <summary>
	///     Tries to get a guild.
	///     <para>Setting <paramref name="withCounts" /> to true will make a REST request.</para>
	/// </summary>
	/// <param name="id">The guild ID to search for.</param>
	/// <param name="guild">The guild, if found.</param>
	/// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
	/// <param name="fetch">Whether to ignore the cache. Defaults to true.</param>
	/// <returns>True if the guild was found, otherwise false.</returns>
	public bool TryGetGuild(ulong id, [NotNullWhen(true)] out DiscordGuild? guild, bool? withCounts = null, bool fetch = true)
	{
		try
		{
			guild = this.GetGuildAsync(id, withCounts, fetch).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			guild = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a guild preview.
	/// </summary>
	/// <param name="id">The guild ID.</param>
	/// <returns>A preview of the requested guild.</returns>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildPreview> GetGuildPreviewAsync(ulong id)
		=> this.ApiClient.GetGuildPreviewAsync(id);

	/// <summary>
	///     Tries to get a guild preview.
	/// </summary>
	/// <param name="id">The guild ID.</param>
	/// <param name="preview">The preview, if found.</param>
	/// <returns>True if the preview was found, otherwise false.</returns>
	public bool TryGetGuildPreview(ulong id, [NotNullWhen(true)] out DiscordGuildPreview? preview)
	{
		try
		{
			preview = this.ApiClient.GetGuildPreviewAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			preview = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a guild widget.
	/// </summary>
	/// <param name="id">The Guild Id.</param>
	/// <returns>A guild widget.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWidget> GetGuildWidgetAsync(ulong id)
		=> this.ApiClient.GetGuildWidgetAsync(id);

	/// <summary>
	///     Tries to get a guild widget.
	/// </summary>
	/// <param name="id">The Guild Id.</param>
	/// <param name="widget">The widget, if found.</param>
	/// <returns>True if the widget was found, otherwise false.</returns>
	public bool TryGetGuildWidget(ulong id, [NotNullWhen(true)] out DiscordWidget? widget)
	{
		try
		{
			widget = this.ApiClient.GetGuildWidgetAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			widget = null;
			return false;
		}
		catch (UnauthorizedException)
		{
			widget = null;
			return false;
		}
	}

	/// <summary>
	///     Gets an invite.
	/// </summary>
	/// <param name="code">The invite code.</param>
	/// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="withPermissions">Whether to include the invite's permissions.</param>
	/// <returns>The requested invite.</returns>
	/// <exception cref="NotFoundException">Thrown when the invite does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInvite> GetInviteByCodeAsync(string code, bool? withCounts = null, ulong? scheduledEventId = null, bool? withPermissions = null)
		=> this.ApiClient.GetInviteAsync(code, withCounts, scheduledEventId, withPermissions);

	/// <summary>
	///     Tries to get an invite.
	/// </summary>
	/// <param name="code">The invite code.</param>
	/// <param name="invite">The invite, if found.</param>
	/// <param name="withCounts">Whether to include presence and total member counts in the returned invite.</param>
	/// <param name="scheduledEventId">The scheduled event id.</param>
	/// <param name="withPermissions">Whether to include the invite's permissions.</param>
	/// <returns>True if the invite was found, otherwise false.</returns>
	public bool TryGetInviteByCode(string code, [NotNullWhen(true)] out DiscordInvite? invite, bool? withCounts = null, ulong? scheduledEventId = null, bool? withPermissions = null)
	{
		try
		{
			invite = this.GetInviteByCodeAsync(code, withCounts, scheduledEventId, withPermissions).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			invite = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a list of user connections.
	/// </summary>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordConnection>> GetConnectionsAsync()
		=> this.ApiClient.GetUserConnectionsAsync();

	/// <summary>
	///     Gets a sticker.
	/// </summary>
	/// <returns>The requested sticker.</returns>
	/// <param name="id">The id of the sticker.</param>
	/// <exception cref="NotFoundException">Thrown when the sticker does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordSticker> GetStickerAsync(ulong id)
		=> this.ApiClient.GetStickerAsync(id);

	/// <summary>
	///     Tries to get a sticker.
	/// </summary>
	/// <returns>True if found, otherwise false.</returns>
	/// <param name="id">The id of the sticker.</param>
	/// <param name="sticker">The sticker, if found.</param>
	public bool TryGetSticker(ulong id, [NotNullWhen(true)] out DiscordSticker? sticker)
	{
		try
		{
			sticker = this.GetStickerAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			sticker = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a sticker pack.
	/// </summary>
	/// <param name="id">The sticker pack's id.</param>
	/// <returns>The sticker pack.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordStickerPack> GetStickerPackAsync(ulong id)
		=> this.ApiClient.GetStickerPackAsync(id);

	/// <summary>
	///     Gets all nitro sticker packs.
	/// </summary>
	/// <returns>List of sticker packs.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordStickerPack>> GetStickerPacksAsync()
		=> this.ApiClient.GetStickerPacksAsync();

	/// <summary>
	///     Gets the In-App OAuth Url for the current application.
	/// </summary>
	/// <param name="scopes">
	///     The scopes to send with the request. Defaults to
	///     <see cref="DisCatSharp.Enums.OAuthScopes.BOT_DEFAULT" />. Mutually exclusive to <paramref name="manual_scopes" />.
	/// </param>
	/// <param name="redir">Redirect Uri.</param>
	/// <param name="permissions">Defaults to <see cref="Permissions.None" />.</param>
	/// <param name="user_install">Whether to install as user app.</param>
	/// <param name="guild_id">The guild id to pre-select.</param>
	/// <param name="state">The state to send with the request.</param>
	/// <param name="access_type">The access type to send with the request (offline|online).</param>
	/// <param name="response_type">The response type to send with the request (code|token).</param>
	/// <param name="prompt">Whether to prompt the user for authorization.</param>
	/// <param name="manual_scopes">The scopes to send with the request. Mutually exclusive to <paramref name="scopes" />.</param>
	/// <returns>The OAuth Url</returns>
	public Uri GetInAppOAuth(Permissions permissions = Permissions.None, OAuthScopes scopes = OAuthScopes.BOT_DEFAULT, string? redir = null, bool user_install = false, ulong? guild_id = null, string? state = null, string? access_type = null, string? response_type = null, bool prompt = true, string? manual_scopes = null)
	{
		permissions &= PermissionMethods.FullPerms;
		return new(new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", this.CurrentApplication.Id.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", manual_scopes ?? OAuth.ResolveScopes(scopes))
			.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
			.AddParameter("state", state ?? string.Empty)
			.AddParameter("redirect_uri", redir ?? string.Empty)
			.AddParameter("integration_type", user_install ? "1" : "0")
			.AddParameter("guild_id", guild_id.HasValue ? guild_id.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)
			.AddParameter("access_type", access_type ?? string.Empty)
			.AddParameter("response_type", response_type ?? string.Empty)
			.AddParameter("prompt", prompt ? "consent" : "none")
			.ToString());
	}

	/// <summary>
	///     Generates an In-App OAuth Url for a specific <paramref name="bot" />.
	/// </summary>
	/// <param name="bot">The bot to generate the url for.</param>
	/// <param name="scopes">
	///     The scopes to send with the request. Defaults to
	///     <see cref="DisCatSharp.Enums.OAuthScopes.BOT_DEFAULT" />. Mutually exclusive to <paramref name="manual_scopes" />.
	/// </param>
	/// <param name="redir">Redirect Uri.</param>
	/// <param name="permissions">Defaults to <see cref="Permissions.None" />.</param>
	/// <param name="user_install">Whether to install as user app.</param>
	/// <param name="guild_id">The guild id to pre-select.</param>
	/// <param name="state">The state to send with the request.</param>
	/// <param name="access_type">The access type to send with the request (offline|online).</param>
	/// <param name="response_type">The response type to send with the request (code|token).</param>
	/// <param name="prompt">Whether to prompt the user for authorization.</param>
	/// <param name="manual_scopes">The scopes to send with the request. Mutually exclusive to <paramref name="scopes" />.</param>
	/// <returns>The OAuth Url</returns>
	public Uri GenerateInAppOauthFor(DiscordUser bot, Permissions permissions = Permissions.None, OAuthScopes scopes = OAuthScopes.BOT_DEFAULT, string? redir = null, bool user_install = false, ulong? guild_id = null, string? state = null, string? access_type = null, string? response_type = null, bool prompt = true, string? manual_scopes = null)
	{
		if (!bot.IsBot)
			throw new ArgumentException("The user must be a bot.", nameof(bot));

		permissions &= PermissionMethods.FullPerms;
		return new(new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", bot.Id.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", manual_scopes ?? OAuth.ResolveScopes(scopes))
			.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
			.AddParameter("state", state ?? string.Empty)
			.AddParameter("redirect_uri", redir ?? string.Empty)
			.AddParameter("integration_type", user_install ? "1" : "0")
			.AddParameter("guild_id", guild_id.HasValue ? guild_id.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)
			.AddParameter("access_type", access_type ?? string.Empty)
			.AddParameter("response_type", response_type ?? string.Empty)
			.AddParameter("prompt", prompt ? "consent" : "none")
			.ToString());
	}

	/// <summary>
	///     Gets a webhook.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <returns>The requested webhook.</returns>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWebhook> GetWebhookAsync(ulong id)
		=> this.ApiClient.GetWebhookAsync(id);

	/// <summary>
	///     Tries to get a webhook.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <param name="webhook">The webhook, if found.</param>
	/// <returns>True if found, otherwise false.</returns>
	public bool TryGetWebhook(ulong id, [NotNullWhen(true)] out DiscordWebhook? webhook)
	{
		try
		{
			webhook = this.GetWebhookAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			webhook = null;
			return false;
		}
	}

	/// <summary>
	///     Gets a webhook with a token.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <param name="token">The target webhook token.</param>
	/// <returns>The requested webhook.</returns>
	/// <exception cref="NotFoundException">Thrown when the webhook does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordWebhook> GetWebhookWithTokenAsync(ulong id, string token)
		=> this.ApiClient.GetWebhookWithTokenAsync(id, token);

	/// <summary>
	///     Tries to get a webhook with a token.
	/// </summary>
	/// <param name="id">The target webhook id.</param>
	/// <param name="token">The target webhook token.</param>
	/// <param name="webhook">The webhook, if found.</param>
	/// <returns>True if found, otherwise false.</returns>
	public bool TryGetWebhookWithToken(ulong id, string token, [NotNullWhen(true)] out DiscordWebhook? webhook)
	{
		try
		{
			webhook = this.GetWebhookWithTokenAsync(id, token).ConfigureAwait(false).GetAwaiter().GetResult();
			return true;
		}
		catch (NotFoundException)
		{
			webhook = null;
			return false;
		}
	}

	/// <summary>
	///     Updates current user's activity and status.
	/// </summary>
	/// <param name="activity">Activity to set.</param>
	/// <param name="userStatus">Status of the user.</param>
	/// <param name="idleSince">Since when is the client performing the specified activity.</param>
	/// <returns></returns>
	public Task UpdateStatusAsync(DiscordActivity? activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
		=> this.InternalUpdateStatusAsync(activity is not null ? [activity] : null, userStatus, idleSince);

	/// <summary>
	///     Edits current user.
	/// </summary>
	/// <param name="username">New username.</param>
	/// <param name="avatar">New avatar.</param>
	/// <param name="banner">New banner.</param>
	/// <returns>The modified user.</returns>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordUser> UpdateCurrentUserAsync(string? username = null, Optional<Stream?> avatar = default, Optional<Stream?> banner = default)
	{
		var av64 = MediaTool.Base64FromStream(avatar);
		var ba64 = MediaTool.Base64FromStream(banner);

		var usr = await this.ApiClient.ModifyCurrentUserAsync(username ?? this.CurrentUser.Username, av64, ba64).ConfigureAwait(false);

		this.CurrentUser.Username = usr.Username;
		this.CurrentUser.Discriminator = usr.Discriminator;
		this.CurrentUser.AvatarHash = usr.AvatarHash;
		this.CurrentUser.BannerHash = usr.BannerHash;
		return this.CurrentUser;
	}

	/// <summary>
	///     Gets a guild template by the code.
	/// </summary>
	/// <param name="code">The code of the template.</param>
	/// <returns>The guild template for the code.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> GetTemplateAsync(string code)
		=> this.ApiClient.GetTemplateAsync(code);

	/// <summary>
	///     Gets all the global application commands for this application.
	/// </summary>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	/// <returns>A list of global application commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false) =>
		this.ApiClient.GetGlobalApplicationCommandsAsync(this.CurrentApplication.Id, withLocalizations);

	/// <summary>
	///     Overwrites the existing global application commands. New commands are automatically created and missing commands
	///     are automatically deleted.
	/// </summary>
	/// <param name="commands">The list of commands to overwrite with.</param>
	/// <returns>The list of global commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGlobalApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
		this.ApiClient.BulkOverwriteGlobalApplicationCommandsAsync(this.CurrentApplication.Id, commands);

	/// <summary>
	///     Creates or overwrites a global application command.
	/// </summary>
	/// <param name="command">The command to create.</param>
	/// <returns>The created command.</returns>
	public Task<DiscordApplicationCommand> CreateGlobalApplicationCommandAsync(DiscordApplicationCommand command) =>
		this.ApiClient.CreateGlobalApplicationCommandAsync(this.CurrentApplication.Id, command);

	/// <summary>
	///     Gets a global application command by its id.
	/// </summary>
	/// <param name="commandId">The id of the command to get.</param>
	/// <returns>The command with the id.</returns>
	public Task<DiscordApplicationCommand> GetGlobalApplicationCommandAsync(ulong commandId) =>
		this.ApiClient.GetGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

	/// <summary>
	///     Edits a global application command.
	/// </summary>
	/// <param name="commandId">The id of the command to edit.</param>
	/// <param name="action">Action to perform.</param>
	/// <returns>The edited command.</returns>
	public async Task<DiscordApplicationCommand> EditGlobalApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
	{
		var mdl = new ApplicationCommandEditModel();
		action(mdl);
		var applicationId = this.CurrentApplication?.Id ?? (await this.GetCurrentApplicationAsync().ConfigureAwait(false)).Id;
		return await this.ApiClient.EditGlobalApplicationCommandAsync(applicationId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.NameLocalizations, mdl.DescriptionLocalizations, mdl.DefaultMemberPermissions, mdl.IsNsfw, mdl.AllowedContexts, mdl.IntegrationTypes).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes a global application command.
	/// </summary>
	/// <param name="commandId">The id of the command to delete.</param>
	public Task DeleteGlobalApplicationCommandAsync(ulong commandId) =>
		this.ApiClient.DeleteGlobalApplicationCommandAsync(this.CurrentApplication.Id, commandId);

	/// <summary>
	///     Gets all the application commands for a guild.
	/// </summary>
	/// <param name="guildId">The id of the guild to get application commands for.</param>
	/// <param name="withLocalizations">Whether to get the full localization dict.</param>
	/// <returns>A list of application commands in the guild.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> GetGuildApplicationCommandsAsync(ulong guildId, bool withLocalizations = false) =>
		this.ApiClient.GetGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, withLocalizations);

	/// <summary>
	///     Overwrites the existing application commands in a guild. New commands are automatically created and missing
	///     commands are automatically deleted.
	/// </summary>
	/// <param name="guildId">The id of the guild.</param>
	/// <param name="commands">The list of commands to overwrite with.</param>
	/// <returns>The list of guild commands.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteGuildApplicationCommandsAsync(ulong guildId, IEnumerable<DiscordApplicationCommand> commands) =>
		this.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.CurrentApplication.Id, guildId, commands);

	/// <summary>
	///     Creates or overwrites a guild application command.
	/// </summary>
	/// <param name="guildId">The id of the guild to create the application command in.</param>
	/// <param name="command">The command to create.</param>
	/// <returns>The created command.</returns>
	public Task<DiscordApplicationCommand> CreateGuildApplicationCommandAsync(ulong guildId, DiscordApplicationCommand command) =>
		this.ApiClient.CreateGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, command);

	/// <summary>
	///     Gets a application command in a guild by its id.
	/// </summary>
	/// <param name="guildId">The id of the guild the application command is in.</param>
	/// <param name="commandId">The id of the command to get.</param>
	/// <returns>The command with the id.</returns>
	public Task<DiscordApplicationCommand> GetGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
		this.ApiClient.GetGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

	/// <summary>
	///     Edits a application command in a guild.
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
		return await this.ApiClient.EditGuildApplicationCommandAsync(applicationId, guildId, commandId, mdl.Name, mdl.Description, mdl.Options, mdl.NameLocalizations, mdl.DescriptionLocalizations, mdl.DefaultMemberPermissions, mdl.IsNsfw, mdl.AllowedContexts, mdl.IntegrationTypes).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes a application command in a guild.
	/// </summary>
	/// <param name="guildId">The id of the guild to delete the application command in.</param>
	/// <param name="commandId">The id of the command.</param>
	public Task DeleteGuildApplicationCommandAsync(ulong guildId, ulong commandId) =>
		this.ApiClient.DeleteGuildApplicationCommandAsync(this.CurrentApplication.Id, guildId, commandId);

	/// <summary>
	///     Gets all default soundboard sounds available for all users.
	/// </summary>
	/// <returns>A list of <see cref="DiscordSoundboardSound" /> objects that are available by default.</returns>
	public Task<IReadOnlyList<DiscordSoundboardSound>> ListDefaultSoundboardSoundsAsync()
		=> this.ApiClient.ListDefaultSoundboardSoundsAsync();

	#endregion

	#region Internal Caching Methods

	/// <summary>
	///     Gets the internal cached threads.
	/// </summary>
	/// <param name="threadId">The target thread id.</param>
	/// <returns>The requested thread.</returns>
	internal DiscordXThreadChannel? InternalGetCachedThread(ulong threadId)
	{
		if (this.GuildsInternal == null || this.GuildsInternal.Count is 0)
			return null;

		foreach (var guild in this.GuildsInternal.Values)
			if (guild.Threads.TryGetValue(threadId, out var foundThread))
				return foundThread;

		return null;
	}

	/// <summary>
	///     Gets an internal cached emoji.
	/// </summary>
	/// <param name="emojiId">The target emoji id.</param>
	/// <returns>The requested emoji.</returns>
	internal DiscordApplicationEmoji? InternalGetCachedApplicationEmoji(ulong emojiId)
		=> this.EmojisInternal is null || this.EmojisInternal.Count is 0 ? null : this.EmojisInternal.GetValueOrDefault(emojiId);

	/// <summary>
	///     Gets the internal cached emojis.
	/// </summary>
	/// <returns>The requested emoji.</returns>
	internal IReadOnlyList<DiscordApplicationEmoji>? InternalGetCachedApplicationEmojis()
		=> this.EmojisInternal is null || this.EmojisInternal.Count is 0 ? null : this.EmojisInternal.Values.ToList();

	/// <summary>
	///     Gets the internal cached scheduled event.
	/// </summary>
	/// <param name="scheduledEventId">The target scheduled event id.</param>
	/// <returns>The requested scheduled event.</returns>
	internal DiscordScheduledEvent? InternalGetCachedScheduledEvent(ulong scheduledEventId)
	{
		if (this.GuildsInternal == null || this.GuildsInternal.Count is 0)
			return null;

		foreach (var guild in this.GuildsInternal.Values)
			if (guild.ScheduledEvents.TryGetValue(scheduledEventId, out var foundScheduledEvent))
				return foundScheduledEvent;

		return null;
	}

	/// <summary>
	///     Gets the internal cached channel.
	/// </summary>
	/// <param name="channelId">The target channel id.</param>
	/// <param name="guildId">The target guild id.</param>
	/// <returns>The requested channel.</returns>
	internal DiscordChannel? InternalGetCachedChannel(ulong channelId, ulong? guildId = null)
	{
		if (this.GuildsInternal == null || this.GuildsInternal.Count is 0)
			return null;

		foreach (var guild in this.GuildsInternal.Values)
			if (guild.Channels.TryGetValue(channelId, out var foundChannel))
			{
				if (guildId.HasValue)
					foundChannel.GuildId = guildId;
				return foundChannel;
			}

		return null;
	}

	/// <summary>
	///     Gets the internal cached guild.
	/// </summary>
	/// <param name="guildId">The target guild id.</param>
	/// <returns>The requested guild.</returns>
	internal DiscordGuild? InternalGetCachedGuild(ulong? guildId)
	{
		if (this.GuildsInternal is { Count: not 0 } && guildId.HasValue)
			if (this.GuildsInternal.TryGetValue(guildId.Value, out var guild))
				return guild;

		return null;
	}

	/// <summary>
	///     Updates a message.
	/// </summary>
	/// <param name="message">The message to update.</param>
	/// <param name="author">The author to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <param name="member">The member to update.</param>
	private void UpdateMessage(DiscordMessage message, TransportUser? author, DiscordGuild? guild, TransportMember? member)
	{
		if (author is not null)
		{
			var usr = new DiscordUser(author)
			{
				Discord = this
			};

			if (member is not null)
				member.User = author;

			message.Author = this.UpdateUser(usr, guild?.Id, guild, member);
		}

		var channel = this.InternalGetCachedChannel(message.ChannelId);

		if (channel is not null)
			return;

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
	///     Updates a scheduled event.
	/// </summary>
	/// <param name="scheduledEvent">The scheduled event to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <returns>The updated scheduled event.</returns>
	private DiscordScheduledEvent UpdateScheduledEvent(DiscordScheduledEvent scheduledEvent, DiscordGuild guild)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (scheduledEvent != null)
			_ = guild.ScheduledEventsInternal.AddOrUpdate(scheduledEvent.Id, scheduledEvent, (id, old) =>
			{
				old.Discord = this;
				old.Description = scheduledEvent.Description;
				old.ChannelId = scheduledEvent.ChannelId;
				old.EntityId = scheduledEvent.EntityId;
				old.EntityType = scheduledEvent.EntityType;
				old.EntityMetadata = scheduledEvent.EntityMetadata;
				old.Name = scheduledEvent.Name;
				old.Status = scheduledEvent.Status;
				old.UserCount = scheduledEvent.UserCount;
				old.ScheduledStartTimeRaw = scheduledEvent.ScheduledStartTimeRaw;
				old.ScheduledEndTimeRaw = scheduledEvent.ScheduledEndTimeRaw;
				return old;
			});

		return scheduledEvent;
	}

	/// <summary>
	///     Updates a user.
	/// </summary>
	/// <param name="usr">The user to update.</param>
	/// <param name="guildId">The guild id to update.</param>
	/// <param name="guild">The guild to update.</param>
	/// <param name="mbr">The member to update.</param>
	/// <returns>The updated user.</returns>
	private DiscordUser UpdateUser(DiscordUser usr, ulong? guildId, DiscordGuild? guild, TransportMember? mbr)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (mbr is not null && guild is not null)
		{
			if (mbr.User is not null)
			{
				usr = new(mbr.User)
				{
					Discord = this
				};

				_ = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.BannerHash = usr.BannerHash;
					old.BannerColorInternal = usr.BannerColorInternal;
					old.AvatarDecorationData = usr.AvatarDecorationData;
					old.Collectibles = usr.Collectibles;
					old.IsSystem = usr.IsSystem;
					old.IsBot = usr.IsBot;
					old.ThemeColorsInternal = usr.ThemeColorsInternal;
					old.Pronouns = usr.Pronouns;
					old.Locale = usr.Locale;
					old.GlobalName = usr.GlobalName;
					old.PrimaryGuild = usr.PrimaryGuild;
					return old;
				});

				usr = new DiscordMember(mbr)
				{
					Discord = this,
					GuildId = guildId.Value
				};
			}

			var intents = this.Configuration.Intents;

			if (intents.HasAllPrivilegedIntents() && !guild.IsLarge) // we have the necessary privileged intents, no need to worry about caching here unless guild is large.
				return usr;

			if (!guild.MembersInternal.TryGetValue(usr.Id, out var member))
			{
				if (intents.HasIntent(DiscordIntents.GuildMembers) || this.Configuration.AlwaysCacheMembers) // member can be updated by events, so cache it
					guild.MembersInternal.TryAdd(usr.Id, (DiscordMember)usr);
			}
			else if (intents.HasIntent(DiscordIntents.GuildPresences) || this.Configuration.AlwaysCacheMembers) // we can attempt to update it if it's already in cache.
				if (!intents.HasIntent(DiscordIntents.GuildMembers)) // no need to update if we already have the member events
					_ = guild.MembersInternal.TryUpdate(usr.Id, (DiscordMember)usr, member);
		}
		else if (usr.Username is not null) // check if not a skeleton user
			_ = this.UserCache.AddOrUpdate(usr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				old.BannerHash = usr.BannerHash;
				old.BannerColorInternal = usr.BannerColorInternal;
				old.AvatarDecorationData = usr.AvatarDecorationData;
				old.ThemeColorsInternal = usr.ThemeColorsInternal;
				old.Collectibles = usr.Collectibles;
				old.IsSystem = usr.IsSystem;
				old.IsBot = usr.IsBot;
				old.Pronouns = usr.Pronouns;
				old.Locale = usr.Locale;
				old.GlobalName = usr.GlobalName;
				old.PrimaryGuild = usr.PrimaryGuild;
				return old;
			});

		return usr;
	}

	/// <summary>
	///     Updates the cached scheduled events in a guild.
	/// </summary>
	/// <param name="guild">The guild.</param>
	/// <param name="rawEvents">The raw events.</param>
	private void UpdateCachedScheduledEvents(DiscordGuild guild, JArray? rawEvents)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (rawEvents is not null)
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
	///     Updates the cached application emojis.
	/// </summary>
	/// <param name="rawEmojis">The raw emojis.</param>
	internal override IReadOnlyDictionary<ulong, DiscordApplicationEmoji> UpdateCachedApplicationEmojis(JArray? rawEmojis)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (rawEmojis is not null)
		{
			this.EmojisInternal.Clear();

			foreach (var xj in rawEmojis)
			{
				var xtm = xj.ToDiscordObject<DiscordApplicationEmoji>();

				xtm.Discord = this;

				if (xtm.User is not null)
				{
					xtm.User.Discord = this;
					this.UpdateUser(xtm.User, null, null, null);
				}

				this.EmojisInternal[xtm.Id] = xtm;
			}
		}

		return this.Emojis;
	}

	/// <summary>
	///     Updates a cached application emoji.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	internal override DiscordApplicationEmoji UpdateCachedApplicationEmoji(DiscordApplicationEmoji emoji)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		if (emoji != null)
		{
			_ = this.EmojisInternal.AddOrUpdate(emoji.Id, emoji, (id, old) =>
			{
				old.Discord = this;
				old.Name = emoji.Name;
				old.RolesInternal = emoji.RolesInternal;
				old.RequiresColons = emoji.RequiresColons;
				old.IsManaged = emoji.IsManaged;
				old.IsAnimated = emoji.IsAnimated;
				old.IsAvailable = emoji.IsAvailable;
				old.User = emoji.User;
				return old;
			});
			if (emoji.User is null)
				return emoji;

			emoji.User.Discord = this;
			this.UpdateUser(emoji.User, null, null, null);
		}

		return emoji;
	}

	/// <summary>
	///     Updates the cached guild.
	/// </summary>
	/// <param name="newGuild">The new guild.</param>
	/// <param name="rawMembers">The raw members.</param>
	private void UpdateCachedGuild(DiscordGuild newGuild, JArray? rawMembers)
	{
		ObjectDisposedException.ThrowIf(this._disposed, this);

		this.GuildsInternal.TryAdd(newGuild.Id, newGuild);

		var guild = this.GuildsInternal[newGuild.Id];

		if (newGuild.ChannelsInternal is { IsEmpty: false })
			foreach (var channel in newGuild.ChannelsInternal.Values)
			{
				if (guild.ChannelsInternal.TryGetValue(channel.Id, out _))
					continue;

				channel.Initialize(this);

				guild.ChannelsInternal[channel.Id] = channel;
			}

		if (newGuild.ThreadsInternal is { IsEmpty: false })
			foreach (var thread in newGuild.ThreadsInternal.Values)
			{
				if (guild.ThreadsInternal.TryGetValue(thread.Id, out _))
					continue;

				guild.ThreadsInternal[thread.Id] = thread;
			}

		if (newGuild.ScheduledEventsInternal is { IsEmpty: false })
			foreach (var @event in newGuild.ScheduledEventsInternal.Values)
			{
				if (guild.ScheduledEventsInternal.TryGetValue(@event.Id, out _))
					continue;

				guild.ScheduledEventsInternal[@event.Id] = @event;
			}

		if (newGuild.SoundboardSoundsInternal is { IsEmpty: false })
			foreach (var sound in newGuild.SoundboardSoundsInternal.Values)
			{
				if (guild.SoundboardSoundsInternal.TryGetValue(sound.Id, out _))
					continue;

				guild.SoundboardSoundsInternal[sound.Id] = sound;
			}

		foreach (var newEmoji in newGuild.EmojisInternal.Values)
			_ = guild.EmojisInternal.GetOrAdd(newEmoji.Id, newEmoji);

		foreach (var newSticker in newGuild.StickersInternal.Values)
			_ = guild.StickersInternal.GetOrAdd(newSticker.Id, newSticker);

		foreach (var newStageInstance in newGuild.StageInstancesInternal.Values)
			_ = guild.StageInstancesInternal.GetOrAdd(newStageInstance.Id, newStageInstance);

		if (rawMembers is not null)
		{
			guild.MembersInternal.Clear();

			foreach (var xj in rawMembers)
			{
				var xtm = xj.ToDiscordObject<TransportMember>();

				ArgumentNullException.ThrowIfNull(xtm.User);
				var usr = new DiscordUser(xtm.User)
				{
					Discord = this
				};
				_ = this.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.BannerHash = usr.BannerHash;
					old.BannerColorInternal = usr.BannerColorInternal;
					old.Collectibles = usr.Collectibles;
					old.IsSystem = usr.IsSystem;
					old.IsBot = usr.IsBot;
					old.AvatarDecorationData = usr.AvatarDecorationData;
					old.ThemeColorsInternal = usr.ThemeColorsInternal;
					old.Pronouns = usr.Pronouns;
					old.Locale = usr.Locale;
					old.GlobalName = usr.GlobalName;
					old.PrimaryGuild = usr.PrimaryGuild;
					return old;
				});

				guild.MembersInternal[xtm.User.Id] = new(xtm)
				{
					Discord = this,
					GuildId = guild.Id
				};
			}
		}

		foreach (var role in newGuild.RolesInternal.Values)
		{
			if (guild.RolesInternal.TryGetValue(role.Id, out _))
				continue;

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
	///     Populates the message reactions and cache.
	/// </summary>
	/// <param name="message">The message.</param>
	/// <param name="author">The author.</param>
	/// <param name="member">The member.</param>
	private void PopulateMessageReactionsAndCache(DiscordMessage message, TransportUser author, TransportMember? member)
	{
		var guild = message.Channel?.Guild ?? this.InternalGetCachedGuild(message.GuildId);

		this.UpdateMessage(message, author, guild, member);

		message.ReactionsInternal ??= [];
		foreach (var xr in message.ReactionsInternal)
			xr.Emoji.Discord = this;

		if (this.Configuration.MessageCacheSize > 0 && message.Channel is not null)
			this.MessageCache?.Add(message);
	}

	#endregion

	#region Disposal

	/// <summary>
	///     Disposes the client.
	/// </summary>
	~DiscordClient()
	{
		this.Dispose();
	}

	/// <summary>
	///     Whether the client is disposed.
	/// </summary>
	private volatile bool _disposed;

	/// <summary>
	///     Disposes the client.
	/// </summary>
	public override void Dispose()
	{
		if (this._disposed)
			return;

		lock (this)
		{
			if (this._disposed)
				return;

			this._disposed = true;
		}

		try
		{
			this.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
		catch
		{ }

		this.ApiClient.Rest.Dispose();
		this.CurrentUser = null!;
		this.CommandCooldownBuckets.Clear();

		var extensions = this._extensions; // prevent _extensions being modified during dispose
		this._extensions.Clear();
		foreach (var extension in extensions)
		{
			if (extension is IDisposable disposable)
				disposable.Dispose();
		}

		try
		{
			this._cancelTokenSource?.Cancel();
			this._cancelTokenSource?.Dispose();
		}
		catch
		{ }

		this.GuildsInternal.Clear();
		this.EmojisInternal.Clear();
		this._heartbeatTask?.Dispose();

		if (this.Configuration.EnableSentry)
			SentrySdk.EndSession();

		GC.SuppressFinalize(this);
	}

	#endregion
}
