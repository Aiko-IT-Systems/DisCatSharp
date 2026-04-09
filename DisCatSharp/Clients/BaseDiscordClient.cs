using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Entities.Core;
using DisCatSharp.Enums;
using DisCatSharp.Enums.Core;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using DisCatSharp.Telemetry;

namespace DisCatSharp;

/// <summary>
///     Represents a common base for various Discord Client implementations.
/// </summary>
public abstract class BaseDiscordClient : IDisposable, IAsyncDisposable
{
	/// <summary>
	///     Gets the lazy voice regions.
	/// </summary>
	internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> VoiceRegionsLazy;

	public event EventHandler<GlobalExceptionEventArgs> GlobalExceptionOccurred;

	public void InitGlobalExceptionTracking()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			GlobalExceptionOccurred?.Invoke(this, new GlobalExceptionEventArgs((Exception)e.ExceptionObject, this.ServiceProvider));
		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			GlobalExceptionOccurred?.Invoke(this, new GlobalExceptionEventArgs(e.Exception, this.ServiceProvider));
			e.SetObserved();
		};
	}

	/// <summary>
	///     Initializes this Discord API client.
	/// </summary>
	/// <param name="config">Configuration for this client.</param>
	protected BaseDiscordClient(DiscordConfiguration config)
	{
		this.Configuration = new(config);
		this.ServiceProvider = config.ServiceProvider;
		if (this.Configuration.Telemetry.CustomSentryDsn != null)
			SentryDsn = this.Configuration.Telemetry.CustomSentryDsn;
		if (this.ServiceProvider is not null)
		{
			this.Configuration.Logging.LoggerFactory ??= config.ServiceProvider.GetService<ILoggerFactory>()!;
			this.Logger = config.ServiceProvider.GetService<ILogger<BaseDiscordClient>>()!;
		}

		if (this.Configuration.Logging.LoggerFactory is null)
		{
			this.Configuration.Logging.LoggerFactory = new DefaultLoggerFactory();
			this.Configuration.Logging.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
		}

		this.DiagnosticsSink = !this.Configuration.Logging.HasShardLogger ? TelemetryBootstrap.CreateSink(this.Configuration) : NoOpDiagnosticsSink.Instance;

		this.Logger ??= this.Configuration.Logging.LoggerFactory!.CreateLogger<BaseDiscordClient>();

		this.ApiClient = new(this);
		this.UserCache = new();
		this.InternalVoiceRegions = new();
		this.VoiceRegionsLazy = new(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));

		var httphandler = new HttpClientHandler
		{
			UseCookies = false,
			AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
			UseProxy = this.Configuration.Proxy != null,
			Proxy = this.Configuration.Proxy
		};
		this.RestClient = new(httphandler)
		{
			Timeout = this.Configuration.Rest.RequestTimeout
		};

		var a = typeof(DiscordClient).GetTypeInfo().Assembly;

		var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (iv != null)
			this.VersionString = iv.InformationalVersion;
		else
		{
			var v = a.GetName().Version;
			var vs = v.ToString(3);

			if (v.Revision > 0)
				this.VersionString = $"{vs}, CI build {v.Revision}";
		}
		this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation(CommonHeaders.USER_AGENT, Utilities.GetUserAgent());

		this.InitGlobalExceptionTracking();
		_ = Task.Run(() => DisCatSharpBadDomainChecker.LoadAndInitBadDomainHashesAsync(this))
			.ContinueWith(t => this.Logger.LogWarning(t.Exception, "Bad domain checker failed to load"), TaskContinuationOptions.OnlyOnFaulted);
	}

	/// <summary>
	///     Gets the api client.
	/// </summary>
	protected internal DiscordApiClient ApiClient { get; }

	/// <summary>
	///     Gets the REST diagnostics provider, exposing runtime metrics for bucket workers, queue depths, and circuit breaker state.
	///     Returns a safe wrapper — the underlying <see cref="RestClient" /> cannot be accessed through this property.
	/// </summary>
	public IRestDiagnostics RestDiagnostics
		=> new RestDiagnosticsWrapper(this.ApiClient.Rest);

	/// <summary>
	///     Cancels all pending REST requests across every bucket worker queue.
	///     Each cancelled request is faulted with <see cref="OperationCanceledException" />.
	///     Use this as an emergency stop when stuck queues or cascading failures need immediate clearing.
	/// </summary>
	/// <param name="reason">An optional reason included in the cancellation exception message.</param>
	public void CancelAllPendingRequests(string? reason = null)
		=> this.ApiClient.Rest.CancelAllPendingRequests(reason);

	/// <summary>
	///     Gets the library diagnostics sink for telemetry reporting.
	/// </summary>
	internal ILibraryDiagnosticsSink DiagnosticsSink { get; set; }

	/// <summary>
	///     Gets the current api channel.
	/// </summary>
	public ApiChannel ApiChannel
		=> this.Configuration.Api.Channel;

	/// <summary>
	///     Gets the current api version.
	/// </summary>
	public string ApiVersion
		=> $"v{this.Configuration.Api.Version}";

	/// <summary>
	///     Gets the sentry dsn.
	/// </summary>
	internal static string SentryDsn { get; set; } = "https://1da216e26a2741b99e8ccfccea1b7ac8@o1113828.ingest.sentry.io/4504901362515968";

	/// <summary>
	///     Gets the configuration.
	/// </summary>
	protected internal DiscordConfiguration Configuration { get; }

	/// <summary>
	///     Gets the instance of the logger for this client.
	/// </summary>
	public ILogger<BaseDiscordClient> Logger { get; internal set; }

	/// <summary>
	///     Gets the string representing the version of bot lib.
	/// </summary>
	public string VersionString { get; }

	/// <summary>
	///     Gets the bot library name.
	/// </summary>
	public string BotLibrary
		=> "DisCatSharp";

	/// <summary>
	///     Gets the current user.
	/// </summary>
	public DiscordUser CurrentUser { get; internal set; }

	/// <summary>
	///     Gets the current application.
	/// </summary>
	public DiscordApplication CurrentApplication { get; internal set; }

	/// <summary>
	///     Exposes a <see cref="HttpClient">Http Client</see> for custom operations.
	/// </summary>
	public HttpClient RestClient { get; internal set; }

	/// <summary>
	///     Gets the cached guilds for this client.
	/// </summary>
	public abstract IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

	/// <summary>
	///     Gets the statistics for this client.
	/// </summary>
	public IReadOnlyDictionary<DisCatSharpStatisticType, int> Statistics
	{
		get
		{
			return new Dictionary<DisCatSharpStatisticType, int>
			{
#pragma warning disable IDE0004
				[DisCatSharpStatisticType.Guilds] = this.Guilds.Count,
				[DisCatSharpStatisticType.Users] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.MemberCount ?? 0)),
				[DisCatSharpStatisticType.Channels] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.Channels.Count)),
				[DisCatSharpStatisticType.Threads] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.Threads.Count)),
				[DisCatSharpStatisticType.Roles] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.Roles.Count)),
				[DisCatSharpStatisticType.Emojis] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.Emojis.Count)),
				[DisCatSharpStatisticType.Stickers] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.Stickers.Count)),
				[DisCatSharpStatisticType.SoundboardSounds] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.SoundboardSounds.Count)),
				[DisCatSharpStatisticType.StageInstances] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.StageInstances.Count)),
				[DisCatSharpStatisticType.ScheduledEvents] = this.Guilds.Values.Sum((Func<DiscordGuild, int>)(guild => guild.ScheduledEvents.Count))
#pragma warning restore IDE0004
			};
		}
	}

	/// <summary>
	///     Lock that serialises all mutations of and snapshots from <see cref="_readyGuildIds" />.
	/// </summary>
	private readonly Lock _readyGuildIdsLock = new();

	/// <summary>
	///     Backing store for <see cref="ReadyGuildIds" />.
	/// </summary>
	private readonly List<ulong> _readyGuildIds = [];

	/// <summary>
	///     Gets a point-in-time snapshot of the guild ids received in the last READY payload for this shard.
	///     Callers receive their own copy — mutations to the returned list do not affect the internal state.
	/// </summary>
	internal List<ulong> ReadyGuildIds
	{
		get
		{
			lock (this._readyGuildIdsLock)
				return [..this._readyGuildIds];
		}
	}

	/// <summary>
	///     Atomically replaces the ready guild id set.  All callers that were previously reading a snapshot
	///     are unaffected; future calls to <see cref="ReadyGuildIds" /> will reflect the new set.
	/// </summary>
	/// <param name="ids">The new collection of guild ids.</param>
	internal void SetReadyGuildIds(IEnumerable<ulong> ids)
	{
		lock (this._readyGuildIdsLock)
		{
			this._readyGuildIds.Clear();
			this._readyGuildIds.AddRange(ids);
		}
	}

	/// <summary>
	///     Gets the cached users for this client.
	/// </summary>
	public ConcurrentDictionary<ulong, DiscordUser> UserCache { get; internal set; }

	/// <summary>
	///     <para>Gets the service provider.</para>
	///     <para>This allows passing data around without resorting to static members.</para>
	///     <para>Defaults to null.</para>
	/// </summary>
	internal IServiceProvider ServiceProvider { get; set; }

	/// <summary>
	///     Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
	/// </summary>
	public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
		=> this.VoiceRegionsLazy.Value;

	/// <summary>
	///     Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions" />.
	/// </summary>
	protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }

	/// <summary>
	///     Gets the cached application emojis for this client.
	/// </summary>
	public abstract IReadOnlyDictionary<ulong, DiscordApplicationEmoji> Emojis { get; }

	/// <summary>
	///     Disposes this client.
	/// </summary>
	public abstract void Dispose();

	/// <summary>
	///     Asynchronously disposes this client.
	/// </summary>
	public abstract ValueTask DisposeAsync();

	/// <summary>
	///     Gets the current API application.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public async Task<DiscordApplication> GetCurrentApplicationAsync(CancellationToken cancellationToken = default)
	{
		var tapp = await this.ApiClient.GetCurrentApplicationOauth2InfoAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
		return new(tapp);
	}

	/// <summary>
	///     Updates the current API application.
	/// </summary>
	/// <param name="description">The new description.</param>
	/// <param name="interactionsEndpointUrl">The new interactions endpoint url.</param>
	/// <param name="roleConnectionsVerificationUrl">The new role connections verification url.</param>
	/// <param name="customInstallUrl">The new custom install url.</param>
	/// <param name="tags">The new tags.</param>
	/// <param name="icon">The new application icon.</param>
	/// <param name="coverImage">The new application cover image.</param>
	/// <param name="flags">The new application flags. Can be only limited gateway intents.</param>
	/// <param name="installParams">The new install params.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The updated application.</returns>
	[DiscordDeprecated("Install params is gonna be replaced by integration types config")]
	public async Task<DiscordApplication> UpdateCurrentApplicationInfoAsync(
		Optional<string?> description,
		Optional<string?> interactionsEndpointUrl,
		Optional<string?> roleConnectionsVerificationUrl,
		Optional<string?> customInstallUrl,
		Optional<List<string>?> tags,
		Optional<Stream?> icon,
		Optional<Stream?> coverImage,
		Optional<ApplicationFlags> flags,
		[DiscordDeprecated("Replaced by Optional<DiscordIntegrationTypesConfig?>")]
		Optional<DiscordApplicationInstallParams?> installParams
	, CancellationToken cancellationToken = default)
	{
		var iconb64 = MediaTool.Base64FromStream(icon);
		var coverImageb64 = MediaTool.Base64FromStream(coverImage);
		if (tags is { HasValue: true, Value: not null })
			if (tags.Value.Any(x => x.Length > 20))
				throw new InvalidOperationException("Tags can not exceed 20 chars.");

		DiscordApplication app = new(await this.ApiClient.ModifyCurrentApplicationInfoAsync(description, interactionsEndpointUrl, roleConnectionsVerificationUrl, customInstallUrl, tags, iconb64, coverImageb64, flags, installParams, Optional.None, cancellationToken: cancellationToken).ConfigureAwait(false));
		this.CurrentApplication = app;
		return app;
	}

	/// <summary>
	///     Enables user app functionality.
	/// </summary>
	/// <returns>The updated application.</returns>
	public async Task<DiscordApplication> EnableUserAppsAsync()
	{
		var currentApplication = await this.GetCurrentApplicationAsync().ConfigureAwait(false);
		var installParams = currentApplication.InstallParams;

		DiscordIntegrationTypesConfig integrationTypesConfig = new()
		{
			UserInstall = new(),
			GuildInstall = new()
			{
				OAuth2InstallParams = installParams is { Scopes: not null, Permissions: not null }
					? new()
					{
						Scopes = installParams?.Scopes,
						Permissions = installParams?.Permissions
					}
					: null
			}
		};

		var app = await this.UpdateCurrentApplicationInfoAsync(Optional.None, Optional.None, Optional.None, Optional.None, Optional.None, Optional.None, Optional.None, Optional.None, integrationTypesConfig).ConfigureAwait(false);
		return app;
	}

	/// <summary>
	///     Updates the current API application.
	/// </summary>
	/// <param name="description">The new description.</param>
	/// <param name="interactionsEndpointUrl">The new interactions endpoint url.</param>
	/// <param name="roleConnectionsVerificationUrl">The new role connections verification url.</param>
	/// <param name="customInstallUrl">The new custom install url.</param>
	/// <param name="tags">The new tags.</param>
	/// <param name="icon">The new application icon.</param>
	/// <param name="coverImage">The new application cover image.</param>
	/// <param name="flags">The new application flags. Can be only limited gateway intents.</param>
	/// <param name="integrationTypesConfig">The new integration types configuration.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>The updated application.</returns>
	public async Task<DiscordApplication> UpdateCurrentApplicationInfoAsync(
		Optional<string?> description,
		Optional<string?> interactionsEndpointUrl,
		Optional<string?> roleConnectionsVerificationUrl,
		Optional<string?> customInstallUrl,
		Optional<List<string>?> tags,
		Optional<Stream?> icon,
		Optional<Stream?> coverImage,
		Optional<ApplicationFlags> flags,
		Optional<DiscordIntegrationTypesConfig?> integrationTypesConfig
	, CancellationToken cancellationToken = default)
	{
		var iconb64 = MediaTool.Base64FromStream(icon);
		var coverImageb64 = MediaTool.Base64FromStream(coverImage);
		if (tags is { HasValue: true, Value: not null })
			if (tags.Value.Any(x => x.Length > 20))
				throw new InvalidOperationException("Tags can not exceed 20 chars.");

		DiscordApplication app = new(await this.ApiClient.ModifyCurrentApplicationInfoAsync(description, interactionsEndpointUrl, roleConnectionsVerificationUrl, customInstallUrl, tags, iconb64, coverImageb64, flags, Optional.None, integrationTypesConfig, cancellationToken: cancellationToken).ConfigureAwait(false));
		this.CurrentApplication = app;
		return app;
	}

	/// <summary>
	///     Gets a list of voice regions.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync(CancellationToken cancellationToken = default)
		=> this.ApiClient.ListVoiceRegionsAsync(cancellationToken: cancellationToken);

	/// <summary>
	///     Initializes this client. This method fetches information about current user, application, and voice regions.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		if (this.CurrentUser is null)
		{
			this.CurrentUser = await this.ApiClient.GetCurrentUserAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
			this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
		}

		if (this.Configuration.TokenType is TokenType.Bot && this.CurrentApplication is null)
			this.CurrentApplication = await this.GetCurrentApplicationAsync(cancellationToken).ConfigureAwait(false);

		if (this.Configuration.TokenType is not TokenType.Bearer && this.InternalVoiceRegions.IsEmpty)
		{
			var vrs = await this.ListVoiceRegionsAsync(cancellationToken).ConfigureAwait(false);
			foreach (var xvr in vrs)
				this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
		}
	}

	/// <summary>
	///     Gets the current gateway info for the provided token.
	///     <para>If no value is provided, the configuration value will be used instead.</para>
	/// </summary>
	/// <param name="token">The new token to use for the request.</param>
	/// <param name="cancellationToken">A token to cancel the request.</param>
	/// <returns>A gateway info object.</returns>
	public async Task<GatewayInfo> GetGatewayInfoAsync(string? token = null, CancellationToken cancellationToken = default)
	{
		if (this.Configuration.TokenType is not TokenType.Bot)
			throw new InvalidOperationException("Only bot tokens can access this info.");

		if (!string.IsNullOrEmpty(this.Configuration.Token))
			return await this.ApiClient.GetGatewayInfoAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

		if (string.IsNullOrEmpty(token))
			throw new InvalidOperationException("Could not locate a valid token.");

		this.Configuration.Token = token;

		var res = await this.ApiClient.GetGatewayInfoAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
		this.Configuration.Token = null;
		return res;
	}

	/// <summary>
	///     Gets a cached user.
	/// </summary>
	/// <param name="userId">The user id.</param>
	internal DiscordUser GetCachedOrEmptyUserInternal(ulong userId)
	{
		if (!this.TryGetCachedUserInternal(userId, out var user))
			user = new()
			{
				Id = userId,
				Discord = this
			};

		return user;
	}

	/// <summary>
	///     Tries the get a cached user.
	/// </summary>
	/// <param name="userId">The user id.</param>
	/// <param name="user">The user.</param>
	internal bool TryGetCachedUserInternal(ulong userId, [MaybeNullWhen(false)] out DiscordUser user)
		=> this.UserCache.TryGetValue(userId, out user);

	/// <summary>
	///     Updates the cached application emojis.
	/// </summary>
	/// <param name="rawEmojis">The raw emojis.</param>
	internal abstract IReadOnlyDictionary<ulong, DiscordApplicationEmoji> UpdateCachedApplicationEmojis(JArray? rawEmojis);

	/// <summary>
	///     Updates a cached application emoji.
	/// </summary>
	/// <param name="emoji">The emoji.</param>
	internal abstract DiscordApplicationEmoji UpdateCachedApplicationEmoji(DiscordApplicationEmoji emoji);
}
