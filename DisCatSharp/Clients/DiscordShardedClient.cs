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

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using Sentry;

namespace DisCatSharp;

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
	/// Gets the bot library name.
	/// </summary>
	public string BotLibrary
		=> "DisCatSharp";

	/// <summary>
	/// Gets the current application.
	/// </summary>
	public DiscordApplication CurrentApplication { get; private set; }

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
	private readonly DiscordConfiguration _configuration;

	/// <summary>
	/// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
	/// </summary>
	private ConcurrentDictionary<string, DiscordVoiceRegion> _internalVoiceRegions;

	/// <summary>
	/// Gets a list of shards.
	/// </summary>
	private readonly ConcurrentDictionary<int, DiscordClient> _shards = new();

	/// <summary>
	/// Gets a lazy list of voice regions.
	/// </summary>
	private Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voiceRegionsLazy;

	/// <summary>
	/// Whether the shard client is started.
	/// </summary>
	private bool _isStarted;

	/// <summary>
	/// Whether manual sharding is enabled.
	/// </summary>
	private readonly bool _manuallySharding;

#endregion

#region Constructor

	/// <summary>
	/// Initializes a new auto-sharding Discord client.
	/// </summary>
	/// <param name="config">The configuration to use.</param>
	public DiscordShardedClient(DiscordConfiguration config)
	{
		this.InternalSetup();

		if (config.ShardCount > 1)
			this._manuallySharding = true;

		this._configuration = config;
		this.ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(this._shards);

		if (this._configuration.CustomSentryDsn != null)
			BaseDiscordClient.SentryDsn = this._configuration.CustomSentryDsn;

		if (this._configuration.LoggerFactory == null && !this._configuration.EnableSentry)
		{
			this._configuration.LoggerFactory = new DefaultLoggerFactory();
			this._configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this._configuration.MinimumLogLevel, this._configuration.LogTimestampFormat));
		}
		else if (this._configuration.LoggerFactory == null && this._configuration.EnableSentry)
		{
			var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>(string.Empty, x =>
			{
#pragma warning disable CS0618 // Type or member is obsolete
				x.TimestampFormat = this._configuration.LogTimestampFormat;
#pragma warning restore CS0618 // Type or member is obsolete
				x.LogToStandardErrorThreshold = this._configuration.MinimumLogLevel;
			});
			var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
			var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
			/*
			var configureFormatterOptions = new ConfigureNamedOptions<ConsoleFormatterOptions>(string.Empty, x => { x.TimestampFormat = this.Configuration.LogTimestampFormat; });
			var formatterFactory = new OptionsFactory<ConsoleFormatterOptions>(new[] { configureFormatterOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleFormatterOptions>>());
			var formatterMonitor = new OptionsMonitor<ConsoleFormatterOptions>(formatterFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleFormatterOptions>>(), new OptionsCache<ConsoleFormatterOptions>());
			*/

			var l = new ConsoleLoggerProvider(optionsMonitor);
			this._configuration.LoggerFactory = new LoggerFactory();
			this._configuration.LoggerFactory.AddProvider(l);
		}

		if (this._configuration is { LoggerFactory: not null, EnableSentry: true })
			this._configuration.LoggerFactory.AddSentry(o =>
			{
				var a = typeof(DiscordClient).GetTypeInfo().Assembly;
				var vs = "";
				var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
				if (iv != null)
					vs = iv.InformationalVersion;
				else
				{
					var v = a.GetName().Version;
					vs = v?.ToString(3);
				}

				o.InitializeSdk = true;
				o.Dsn = BaseDiscordClient.SentryDsn;
				o.DetectStartupTime = StartupTimeDetectionMode.Fast;
				o.DiagnosticLevel = SentryLevel.Debug;
				o.Environment = "dev";
				o.IsGlobalModeEnabled = false;
				o.TracesSampleRate = 1.0;
				o.ReportAssembliesMode = ReportAssembliesMode.InformationalVersion;
				o.AddInAppInclude("DisCatSharp");
				o.AttachStacktrace = true;
				o.StackTraceMode = StackTraceMode.Enhanced;
				o.Release = $"{this.BotLibrary}@{vs}";
				o.SendClientReports = true;
				if (!this._configuration.DisableExceptionFilter)
					o.AddExceptionFilter(new DisCatSharpExceptionFilter(this._configuration));
				o.IsEnvironmentUser = false;
				o.UseAsyncFileIO = true;
				o.Debug = this._configuration.SentryDebug;
				o.EnableScopeSync = true;
				o.SetBeforeSend((e, _) =>
				{
					if (!this._configuration.DisableExceptionFilter)
					{
						if (e.Exception != null)
						{
							if (!this._configuration.TrackExceptions.Contains(e.Exception.GetType()))
								return null;
						}
						else if (e.Extra.Count == 0 || !e.Extra.ContainsKey("Found Fields"))
							return null;
					}

					if (!e.HasUser())
						if (this._configuration.AttachUserInfo && this.CurrentUser! != null!)
							e.User = new()
							{
								Id = this.CurrentUser.Id.ToString(),
								Username = this.CurrentUser.UsernameWithDiscriminator,
								Other = new Dictionary<string, string>()
								{
									{ "developer", this._configuration.DeveloperUserId?.ToString() ?? "not_given" },
									{ "email", this._configuration.FeedbackEmail ?? "not_given" }
								}
							};

					if (!e.Extra.ContainsKey("Found Fields"))
						e.SetFingerprint(BaseDiscordClient.GenerateSentryFingerPrint(e));
					return e;
				});
			});

		this._configuration.HasShardLogger = true;

		this.Logger ??= this._configuration.LoggerFactory!.CreateLogger<BaseDiscordClient>();
	}

#endregion

#region Public Methods

	/// <summary>
	/// Initializes and connects all shards.
	/// </summary>
	/// <exception cref="AggregateException"></exception>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task StartAsync()
	{
		if (this._isStarted)
			throw new InvalidOperationException("This client has already been started.");

		this._isStarted = true;

		try
		{
			if (this._configuration.TokenType != TokenType.Bot)
				this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful");
			this.Logger.LogInformation(LoggerEvents.Startup, "Lib {LibraryName}, version {LibraryVersion}", this._botLibrary, this._versionString.Value);

			var shardc = await this.InitializeShardsAsync().ConfigureAwait(false);
			var connectTasks = new List<Task>();
			this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {NumShards} shards", shardc);

			for (var i = 0; i < shardc; i++)
			{
				//This should never happen, but in case it does...
				if (this.GatewayInfo.SessionBucket.MaxConcurrency < 1)
					this.GatewayInfo.SessionBucket.MaxConcurrency = 1;

				if (this.GatewayInfo.SessionBucket.MaxConcurrency == 1)
					await this.ConnectShardAsync(i).ConfigureAwait(false);
				else
				{
					//Concurrent login.
					connectTasks.Add(this.ConnectShardAsync(i));

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
			await this.InternalStopAsync(false).ConfigureAwait(false);

			var message = $"Shard initialization failed, check inner exceptions for details: ";

			this.Logger.LogCritical(LoggerEvents.ShardClientError, "{Message}\n{Ex}", message, ex);
			throw new AggregateException(message, ex);
		}
	}

	/// <summary>
	/// Disconnects and disposes all shards.
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	public Task StopAsync()
		=> this.InternalStopAsync();

	/// <summary>
	/// Gets a shard from a guild id.
	/// <para>
	///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
	///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
	/// </para>
	/// </summary>
	/// <param name="guildId">The guild ID for the shard.</param>
	/// <returns>The found <see cref="DiscordClient"/> shard. Otherwise null if the shard was not found for the guild id.</returns>
	public DiscordClient GetShard(ulong guildId)
	{
		var index = this._manuallySharding ? this.GetShardIdFromGuilds(guildId) : Utilities.GetShardId(guildId, this.ShardClients.Count);

		return index != -1 ? this._shards[index] : null;
	}

	/// <summary>
	/// Gets a shard from a guild.
	/// <para>
	///     If automatically sharding, this will use the <see cref="Utilities.GetShardId(ulong, int)"/> method.
	///     Otherwise if manually sharding, it will instead iterate through each shard's guild caches.
	/// </para>
	/// </summary>
	/// <param name="guild">The guild for the shard.</param>
	/// <returns>The found <see cref="DiscordClient"/> shard. Otherwise null if the shard was not found for the guild.</returns>
	public DiscordClient GetShard(DiscordGuild guild)
		=> this.GetShard(guild.Id);

	/// <summary>
	/// Updates the status on all shards.
	/// </summary>
	/// <param name="activity">The activity to set. Defaults to null.</param>
	/// <param name="userStatus">The optional status to set. Defaults to null.</param>
	/// <param name="idleSince">Since when is the client performing the specified activity. Defaults to null.</param>
	/// <returns>Asynchronous operation.</returns>
	public async Task UpdateStatusAsync(DiscordActivity activity = null, UserStatus? userStatus = null, DateTimeOffset? idleSince = null)
	{
		var tasks = new List<Task>();
		foreach (var client in this._shards.Values)
			tasks.Add(client.UpdateStatusAsync(activity, userStatus, idleSince));

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

#endregion

#region Internal Methods

	/// <summary>
	/// Initializes the shards.
	/// </summary>
	/// <returns>The count of initialized shards.</returns>
	internal async Task<int> InitializeShardsAsync()
	{
		if (!this._shards.IsEmpty)
			return this._shards.Count;

		this.GatewayInfo = await this.GetGatewayInfoAsync().ConfigureAwait(false);
		var shardCount = this._configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this._configuration.ShardCount;
		var lf = new ShardedLoggerFactory(this.Logger);
		for (var i = 0; i < shardCount; i++)
		{
			var cfg = new DiscordConfiguration(this._configuration)
			{
				ShardId = i,
				ShardCount = shardCount,
				LoggerFactory = lf
			};

			var client = new DiscordClient(cfg);
			if (!this._shards.TryAdd(i, client))
				throw new InvalidOperationException("Could not initialize shards.");
		}

		return shardCount;
	}

#endregion

#region Private Methods & Version Property

	/// <summary>
	/// Gets the gateway info.
	/// </summary>
	private async Task<GatewayInfo> GetGatewayInfoAsync()
	{
		var url = $"{Utilities.GetApiBaseUri(this._configuration)}{Endpoints.GATEWAY}{Endpoints.BOT}";
		var http = new HttpClient();

		http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
		http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Utilities.GetFormattedToken(this._configuration));
		http.DefaultRequestHeaders.TryAddWithoutValidation("x-discord-locale", this._configuration.Locale);
		if (!string.IsNullOrWhiteSpace(this._configuration.Timezone))
			http.DefaultRequestHeaders.TryAddWithoutValidation("x-discord-timezone", this._configuration.Timezone);
		if (this._configuration.Override != null)
			http.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", this._configuration.Override);

		this.Logger.LogDebug(LoggerEvents.ShardRest, $"Obtaining gateway information from GET {Endpoints.GATEWAY}{Endpoints.BOT}...");
		var resp = await http.GetAsync(url).ConfigureAwait(false);

		http.Dispose();

		if (!resp.IsSuccessStatusCode)
		{
			var ratelimited = await HandleHttpError(url, resp).ConfigureAwait(false);

			if (ratelimited)
				return await this.GetGatewayInfoAsync().ConfigureAwait(false);
		}

		var timer = new Stopwatch();
		timer.Start();

		var jo = JObject.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
		var info = jo.ToObject<GatewayInfo>();

		//There is a delay from parsing here.
		timer.Stop();

		info.SessionBucket.ResetAfterInternal -= (int)timer.ElapsedMilliseconds;
		info.SessionBucket.ResetAfter = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds(info.SessionBucket.ResetAfterInternal);

		return info;

		async Task<bool> HandleHttpError(string reqUrl, HttpResponseMessage msg)
		{
			var code = (int)msg.StatusCode;

			if (code == 401 || code == 403)
				throw new($"Authentication failed, check your token and try again: {code} {msg.ReasonPhrase}");
			else if (code == 429)
			{
				this.Logger.LogError(LoggerEvents.ShardClientError, $"Ratelimit hit, requeuing request to {reqUrl}");

				var hs = msg.Headers.ToDictionary(xh => xh.Key, xh => string.Join("\n", xh.Value), StringComparer.OrdinalIgnoreCase);
				var waitInterval = 0;

				if (hs.TryGetValue("Retry-After", out var retryAfterRaw))
					waitInterval = int.Parse(retryAfterRaw, CultureInfo.InvariantCulture);

				await Task.Delay(waitInterval).ConfigureAwait(false);
				return true;
			}
			else if (code >= 500)
				throw new($"Internal Server Error: {code} {msg.ReasonPhrase}");
			else
				throw new($"An unsuccessful HTTP status code was encountered: {code} {msg.ReasonPhrase}");
		}
	}

	/// <summary>
	/// Gets the version string.
	/// </summary>
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

	/// <summary>
	/// Gets the name of the used bot library.
	/// </summary>
	private readonly string _botLibrary = "DisCatSharp";

#endregion

#region Private Connection Methods

	/// <summary>
	/// Connects a shard.
	/// </summary>
	/// <param name="i">The shard id.</param>
	private async Task ConnectShardAsync(int i)
	{
		if (!this._shards.TryGetValue(i, out var client))
			throw new($"Could not initialize shard {i}.");

		client.IsShard = true;
		if (this.GatewayInfo != null)
		{
			client.GatewayInfo = this.GatewayInfo;
			client.GatewayUri = new(client.GatewayInfo.Url);
		}

		if (this.CurrentUser != null)
			client.CurrentUser = this.CurrentUser;

		if (this.CurrentApplication != null)
			client.CurrentApplication = this.CurrentApplication;

		if (this._internalVoiceRegions != null)
		{
			client.InternalVoiceRegions = this._internalVoiceRegions;
			client.VoiceRegionsLazy = new(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(client.InternalVoiceRegions));
		}

		this.HookEventHandlers(client);

		await client.ConnectAsync().ConfigureAwait(false);
		this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booted shard {0}.", i);

		this.GatewayInfo ??= client.GatewayInfo;

		if (this.CurrentUser == null)
			this.CurrentUser = client.CurrentUser;

		if (this.CurrentApplication == null)
			this.CurrentApplication = client.CurrentApplication;

		if (this._internalVoiceRegions == null)
		{
			this._internalVoiceRegions = client.InternalVoiceRegions;
			this._voiceRegionsLazy = new(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this._internalVoiceRegions));
		}
	}

	/// <summary>
	/// Stops all shards.
	/// </summary>
	/// <param name="enableLogger">Whether to enable the logger.</param>
	private Task InternalStopAsync(bool enableLogger = true)
	{
		if (!this._isStarted)
			throw new InvalidOperationException("This client has not been started.");

		if (enableLogger)
			this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {0} shards.", this._shards.Count);

		this._isStarted = false;
		this._voiceRegionsLazy = null;

		this.GatewayInfo = null;
		this.CurrentUser = null;
		this.CurrentApplication = null;

		for (var i = 0; i < this._shards.Count; i++)
			if (this._shards.TryGetValue(i, out var client))
			{
				this.UnhookEventHandlers(client);

				client.Dispose();

				if (enableLogger)
					this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {0}.", i);
			}

		this._shards.Clear();

		return Task.CompletedTask;
	}

#endregion

#region Event Handler Initialization/Registering

	/// <summary>
	/// Sets the shard client up internally..
	/// </summary>
	private void InternalSetup()
	{
		this._clientErrored = new("CLIENT_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
		this._socketErrored = new("SOCKET_ERRORED", DiscordClient.EventExecutionLimit, this.Goof);
		this._socketOpened = new("SOCKET_OPENED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._socketClosed = new("SOCKET_CLOSED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._ready = new("READY", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._resumed = new("RESUMED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._channelCreated = new("CHANNEL_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._channelUpdated = new("CHANNEL_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._channelDeleted = new("CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._dmChannelDeleted = new("DM_CHANNEL_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._channelPinsUpdated = new("CHANNEL_PINS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildCreated = new("GUILD_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildAvailable = new("GUILD_AVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildUpdated = new("GUILD_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildDeleted = new("GUILD_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildUnavailable = new("GUILD_UNAVAILABLE", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildDownloadCompleted = new("GUILD_DOWNLOAD_COMPLETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._inviteCreated = new("INVITE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._inviteDeleted = new("INVITE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageCreated = new("MESSAGE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._presenceUpdated = new("PRESENCE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildBanAdded = new("GUILD_BAN_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildBanRemoved = new("GUILD_BAN_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildEmojisUpdated = new("GUILD_EMOJI_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildStickersUpdated = new("GUILD_STICKER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationsUpdated = new("GUILD_INTEGRATIONS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberAdded = new("GUILD_MEMBER_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberRemoved = new("GUILD_MEMBER_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberUpdated = new("GUILD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleCreated = new("GUILD_ROLE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleUpdated = new("GUILD_ROLE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildRoleDeleted = new("GUILD_ROLE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageUpdated = new("MESSAGE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageDeleted = new("MESSAGE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageBulkDeleted = new("MESSAGE_BULK_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._interactionCreated = new("INTERACTION_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._componentInteractionCreated = new("COMPONENT_INTERACTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._contextMenuInteractionCreated = new("CONTEXT_MENU_INTERACTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._typingStarted = new("TYPING_STARTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._userSettingsUpdated = new("USER_SETTINGS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._userUpdated = new("USER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._voiceStateUpdated = new("VOICE_STATE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._voiceServerUpdated = new("VOICE_SERVER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMembersChunk = new("GUILD_MEMBERS_CHUNKED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._unknownEvent = new("UNKNOWN_EVENT", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionAdded = new("MESSAGE_REACTION_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionRemoved = new("MESSAGE_REACTION_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionsCleared = new("MESSAGE_REACTIONS_CLEARED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._messageReactionRemovedEmoji = new("MESSAGE_REACTION_REMOVED_EMOJI", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._webhooksUpdated = new("WEBHOOKS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._heartbeated = new("HEARTBEATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandCreated = new("APPLICATION_COMMAND_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandUpdated = new("APPLICATION_COMMAND_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandDeleted = new("APPLICATION_COMMAND_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildApplicationCommandCountUpdated = new("GUILD_APPLICATION_COMMAND_COUNTS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._applicationCommandPermissionsUpdated = new("APPLICATION_COMMAND_PERMISSIONS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationCreated = new("INTEGRATION_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationUpdated = new("INTEGRATION_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildIntegrationDeleted = new("INTEGRATION_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceCreated = new("STAGE_INSTANCE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceUpdated = new("STAGE_INSTANCE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._stageInstanceDeleted = new("STAGE_INSTANCE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadCreated = new("THREAD_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadUpdated = new("THREAD_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadDeleted = new("THREAD_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadListSynced = new("THREAD_LIST_SYNCED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadMemberUpdated = new("THREAD_MEMBER_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._threadMembersUpdated = new("THREAD_MEMBERS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._zombied = new("ZOMBIED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._payloadReceived = new("PAYLOAD_RECEIVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventCreated = new("GUILD_SCHEDULED_EVENT_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUpdated = new("GUILD_SCHEDULED_EVENT_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventDeleted = new("GUILD_SCHEDULED_EVENT_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUserAdded = new("GUILD_SCHEDULED_EVENT_USER_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildScheduledEventUserRemoved = new("GUILD_SCHEDULED_EVENT_USER_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._embeddedActivityUpdated = new("EMBEDDED_ACTIVITY_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutAdded = new("GUILD_MEMBER_TIMEOUT_ADDED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutChanged = new("GUILD_MEMBER_TIMEOUT_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildMemberTimeoutRemoved = new("GUILD_MEMBER_TIMEOUT_REMOVED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleCreated = new("AUTO_MODERATION_RULE_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleUpdated = new("AUTO_MODERATION_RULE_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._automodRuleDeleted = new("AUTO_MODERATION_RULE_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._automodActionExecuted = new("AUTO_MODERATION_ACTION_EXECUTED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._guildAuditLogEntryCreated = new("GUILD_AUDIT_LOG_ENTRY_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._voiceChannelStatusUpdated = new("CHANNEL_STATUS_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._entitlementCreated = new("ENTITLEMENT_CREATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._entitlementUpdated = new("ENTITLEMENT_UPDATED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
		this._entitlementDeleted = new("ENTITLEMENT_DELETED", DiscordClient.EventExecutionLimit, this.EventErrorHandler);
	}

	/// <summary>
	/// Hooks the event handlers.
	/// </summary>
	/// <param name="client">The client.</param>
	private void HookEventHandlers(DiscordClient client)
	{
		client.ClientErrored += this.Client_ClientError;
		client.SocketErrored += this.Client_SocketError;
		client.SocketOpened += this.Client_SocketOpened;
		client.SocketClosed += this.Client_SocketClosed;
		client.Ready += this.Client_Ready;
		client.Resumed += this.Client_Resumed;
		client.ChannelCreated += this.Client_ChannelCreated;
		client.ChannelUpdated += this.Client_ChannelUpdated;
		client.ChannelDeleted += this.Client_ChannelDeleted;
		client.DmChannelDeleted += this.Client_DMChannelDeleted;
		client.ChannelPinsUpdated += this.Client_ChannelPinsUpdated;
		client.GuildCreated += this.Client_GuildCreated;
		client.GuildAvailable += this.Client_GuildAvailable;
		client.GuildUpdated += this.Client_GuildUpdated;
		client.GuildDeleted += this.Client_GuildDeleted;
		client.GuildUnavailable += this.Client_GuildUnavailable;
		client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;
		client.InviteCreated += this.Client_InviteCreated;
		client.InviteDeleted += this.Client_InviteDeleted;
		client.MessageCreated += this.Client_MessageCreated;
		client.PresenceUpdated += this.Client_PresenceUpdate;
		client.GuildBanAdded += this.Client_GuildBanAdd;
		client.GuildBanRemoved += this.Client_GuildBanRemove;
		client.GuildEmojisUpdated += this.Client_GuildEmojisUpdate;
		client.GuildStickersUpdated += this.Client_GuildStickersUpdate;
		client.GuildIntegrationsUpdated += this.Client_GuildIntegrationsUpdate;
		client.GuildMemberAdded += this.Client_GuildMemberAdd;
		client.GuildMemberRemoved += this.Client_GuildMemberRemove;
		client.GuildMemberUpdated += this.Client_GuildMemberUpdate;
		client.GuildRoleCreated += this.Client_GuildRoleCreate;
		client.GuildRoleUpdated += this.Client_GuildRoleUpdate;
		client.GuildRoleDeleted += this.Client_GuildRoleDelete;
		client.MessageUpdated += this.Client_MessageUpdate;
		client.MessageDeleted += this.Client_MessageDelete;
		client.MessagesBulkDeleted += this.Client_MessageBulkDelete;
		client.InteractionCreated += this.Client_InteractionCreated;
		client.ComponentInteractionCreated += this.Client_ComponentInteractionCreate;
		client.ContextMenuInteractionCreated += this.Client_ContextMenuInteractionCreate;
		client.TypingStarted += this.Client_TypingStart;
		client.UserSettingsUpdated += this.Client_UserSettingsUpdate;
		client.UserUpdated += this.Client_UserUpdate;
		client.VoiceStateUpdated += this.Client_VoiceStateUpdate;
		client.VoiceServerUpdated += this.Client_VoiceServerUpdate;
		client.GuildMembersChunked += this.Client_GuildMembersChunk;
		client.UnknownEvent += this.Client_UnknownEvent;
		client.MessageReactionAdded += this.Client_MessageReactionAdd;
		client.MessageReactionRemoved += this.Client_MessageReactionRemove;
		client.MessageReactionsCleared += this.Client_MessageReactionRemoveAll;
		client.MessageReactionRemovedEmoji += this.Client_MessageReactionRemovedEmoji;
		client.WebhooksUpdated += this.Client_WebhooksUpdate;
		client.Heartbeated += this.Client_HeartBeated;
		client.ApplicationCommandCreated += this.Client_ApplicationCommandCreated;
		client.ApplicationCommandUpdated += this.Client_ApplicationCommandUpdated;
		client.ApplicationCommandDeleted += this.Client_ApplicationCommandDeleted;
		client.GuildApplicationCommandCountUpdated += this.Client_GuildApplicationCommandCountUpdated;
		client.ApplicationCommandPermissionsUpdated += this.Client_ApplicationCommandPermissionsUpdated;
		client.GuildIntegrationCreated += this.Client_GuildIntegrationCreated;
		client.GuildIntegrationUpdated += this.Client_GuildIntegrationUpdated;
		client.GuildIntegrationDeleted += this.Client_GuildIntegrationDeleted;
		client.StageInstanceCreated += this.Client_StageInstanceCreated;
		client.StageInstanceUpdated += this.Client_StageInstanceUpdated;
		client.StageInstanceDeleted += this.Client_StageInstanceDeleted;
		client.ThreadCreated += this.Client_ThreadCreated;
		client.ThreadUpdated += this.Client_ThreadUpdated;
		client.ThreadDeleted += this.Client_ThreadDeleted;
		client.ThreadListSynced += this.Client_ThreadListSynced;
		client.ThreadMemberUpdated += this.Client_ThreadMemberUpdated;
		client.ThreadMembersUpdated += this.Client_ThreadMembersUpdated;
		client.Zombied += this.Client_Zombied;
		client.PayloadReceived += this.Client_PayloadReceived;
		client.GuildScheduledEventCreated += this.Client_GuildScheduledEventCreated;
		client.GuildScheduledEventUpdated += this.Client_GuildScheduledEventUpdated;
		client.GuildScheduledEventDeleted += this.Client_GuildScheduledEventDeleted;
		client.GuildScheduledEventUserAdded += this.Client_GuildScheduledEventUserAdded;
		client.GuildScheduledEventUserRemoved += this.Client_GuildScheduledEventUserRemoved;
		client.EmbeddedActivityUpdated += this.Client_EmbeddedActivityUpdated;
		client.GuildMemberTimeoutAdded += this.Client_GuildMemberTimeoutAdded;
		client.GuildMemberTimeoutChanged += this.Client_GuildMemberTimeoutChanged;
		client.GuildMemberTimeoutRemoved += this.Client_GuildMemberTimeoutRemoved;
		client.AutomodRuleCreated += this.Client_AutomodRuleCreated;
		client.AutomodRuleUpdated += this.Client_AutomodRuleUpdated;
		client.AutomodRuleDeleted += this.Client_AutomodRuleDeleted;
		client.AutomodActionExecuted += this.Client_AutomodActionExecuted;
		client.GuildAuditLogEntryCreated += this.Client_GuildAuditLogEntryCreated;
		client.VoiceChannelStatusUpdated += this.Client_VoiceChannelStatusUpdated;
		client.EntitlementCreated += this.Client_EntitlementCreated;
		client.EntitlementUpdated += this.Client_EntitlementUpdated;
		client.EntitlementDeleted += this.Client_EntitlementDeleted;
	}

	/// <summary>
	/// Unhooks the event handlers.
	/// </summary>
	/// <param name="client">The client.</param>
	private void UnhookEventHandlers(DiscordClient client)
	{
		client.ClientErrored -= this.Client_ClientError;
		client.SocketErrored -= this.Client_SocketError;
		client.SocketOpened -= this.Client_SocketOpened;
		client.SocketClosed -= this.Client_SocketClosed;
		client.Ready -= this.Client_Ready;
		client.Resumed -= this.Client_Resumed;
		client.ChannelCreated -= this.Client_ChannelCreated;
		client.ChannelUpdated -= this.Client_ChannelUpdated;
		client.ChannelDeleted -= this.Client_ChannelDeleted;
		client.DmChannelDeleted -= this.Client_DMChannelDeleted;
		client.ChannelPinsUpdated -= this.Client_ChannelPinsUpdated;
		client.GuildCreated -= this.Client_GuildCreated;
		client.GuildAvailable -= this.Client_GuildAvailable;
		client.GuildUpdated -= this.Client_GuildUpdated;
		client.GuildDeleted -= this.Client_GuildDeleted;
		client.GuildUnavailable -= this.Client_GuildUnavailable;
		client.GuildDownloadCompleted -= this.Client_GuildDownloadCompleted;
		client.InviteCreated -= this.Client_InviteCreated;
		client.InviteDeleted -= this.Client_InviteDeleted;
		client.MessageCreated -= this.Client_MessageCreated;
		client.PresenceUpdated -= this.Client_PresenceUpdate;
		client.GuildBanAdded -= this.Client_GuildBanAdd;
		client.GuildBanRemoved -= this.Client_GuildBanRemove;
		client.GuildEmojisUpdated -= this.Client_GuildEmojisUpdate;
		client.GuildStickersUpdated -= this.Client_GuildStickersUpdate;
		client.GuildIntegrationsUpdated -= this.Client_GuildIntegrationsUpdate;
		client.GuildMemberAdded -= this.Client_GuildMemberAdd;
		client.GuildMemberRemoved -= this.Client_GuildMemberRemove;
		client.GuildMemberUpdated -= this.Client_GuildMemberUpdate;
		client.GuildRoleCreated -= this.Client_GuildRoleCreate;
		client.GuildRoleUpdated -= this.Client_GuildRoleUpdate;
		client.GuildRoleDeleted -= this.Client_GuildRoleDelete;
		client.MessageUpdated -= this.Client_MessageUpdate;
		client.MessageDeleted -= this.Client_MessageDelete;
		client.MessagesBulkDeleted -= this.Client_MessageBulkDelete;
		client.InteractionCreated -= this.Client_InteractionCreated;
		client.ComponentInteractionCreated -= this.Client_ComponentInteractionCreate;
		client.ContextMenuInteractionCreated -= this.Client_ContextMenuInteractionCreate;
		client.TypingStarted -= this.Client_TypingStart;
		client.UserSettingsUpdated -= this.Client_UserSettingsUpdate;
		client.UserUpdated -= this.Client_UserUpdate;
		client.VoiceStateUpdated -= this.Client_VoiceStateUpdate;
		client.VoiceServerUpdated -= this.Client_VoiceServerUpdate;
		client.GuildMembersChunked -= this.Client_GuildMembersChunk;
		client.UnknownEvent -= this.Client_UnknownEvent;
		client.MessageReactionAdded -= this.Client_MessageReactionAdd;
		client.MessageReactionRemoved -= this.Client_MessageReactionRemove;
		client.MessageReactionsCleared -= this.Client_MessageReactionRemoveAll;
		client.MessageReactionRemovedEmoji -= this.Client_MessageReactionRemovedEmoji;
		client.WebhooksUpdated -= this.Client_WebhooksUpdate;
		client.Heartbeated -= this.Client_HeartBeated;
		client.ApplicationCommandCreated -= this.Client_ApplicationCommandCreated;
		client.ApplicationCommandUpdated -= this.Client_ApplicationCommandUpdated;
		client.ApplicationCommandDeleted -= this.Client_ApplicationCommandDeleted;
		client.GuildApplicationCommandCountUpdated -= this.Client_GuildApplicationCommandCountUpdated;
		client.ApplicationCommandPermissionsUpdated -= this.Client_ApplicationCommandPermissionsUpdated;
		client.GuildIntegrationCreated -= this.Client_GuildIntegrationCreated;
		client.GuildIntegrationUpdated -= this.Client_GuildIntegrationUpdated;
		client.GuildIntegrationDeleted -= this.Client_GuildIntegrationDeleted;
		client.StageInstanceCreated -= this.Client_StageInstanceCreated;
		client.StageInstanceUpdated -= this.Client_StageInstanceUpdated;
		client.StageInstanceDeleted -= this.Client_StageInstanceDeleted;
		client.ThreadCreated -= this.Client_ThreadCreated;
		client.ThreadUpdated -= this.Client_ThreadUpdated;
		client.ThreadDeleted -= this.Client_ThreadDeleted;
		client.ThreadListSynced -= this.Client_ThreadListSynced;
		client.ThreadMemberUpdated -= this.Client_ThreadMemberUpdated;
		client.ThreadMembersUpdated -= this.Client_ThreadMembersUpdated;
		client.Zombied -= this.Client_Zombied;
		client.PayloadReceived -= this.Client_PayloadReceived;
		client.GuildScheduledEventCreated -= this.Client_GuildScheduledEventCreated;
		client.GuildScheduledEventUpdated -= this.Client_GuildScheduledEventUpdated;
		client.GuildScheduledEventDeleted -= this.Client_GuildScheduledEventDeleted;
		client.GuildScheduledEventUserAdded -= this.Client_GuildScheduledEventUserAdded;
		client.GuildScheduledEventUserRemoved -= this.Client_GuildScheduledEventUserRemoved;
		client.EmbeddedActivityUpdated -= this.Client_EmbeddedActivityUpdated;
		client.GuildMemberTimeoutAdded -= this.Client_GuildMemberTimeoutAdded;
		client.GuildMemberTimeoutChanged -= this.Client_GuildMemberTimeoutChanged;
		client.GuildMemberTimeoutRemoved -= this.Client_GuildMemberTimeoutRemoved;
		client.AutomodRuleCreated -= this.Client_AutomodRuleCreated;
		client.AutomodRuleUpdated -= this.Client_AutomodRuleUpdated;
		client.AutomodRuleDeleted -= this.Client_AutomodRuleDeleted;
		client.AutomodActionExecuted -= this.Client_AutomodActionExecuted;
		client.GuildAuditLogEntryCreated -= this.Client_GuildAuditLogEntryCreated;
		client.VoiceChannelStatusUpdated -= this.Client_VoiceChannelStatusUpdated;
		client.EntitlementCreated -= this.Client_EntitlementCreated;
		client.EntitlementUpdated -= this.Client_EntitlementUpdated;
		client.EntitlementDeleted -= this.Client_EntitlementDeleted;
	}

	/// <summary>
	/// Gets the shard id from guilds.
	/// </summary>
	/// <param name="id">The id.</param>
	/// <returns>An int.</returns>
	private int GetShardIdFromGuilds(ulong id)
	{
		foreach (var s in this._shards.Values)
			if (s.GuildsInternal.TryGetValue(id, out _))
				return s.ShardId;

		return -1;
	}

#endregion

#region Destructor

	~DiscordShardedClient()
	{
		this.InternalStopAsync(false).GetAwaiter().GetResult();
	}

#endregion
}
