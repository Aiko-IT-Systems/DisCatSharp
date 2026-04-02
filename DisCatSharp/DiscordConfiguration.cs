using System;
using System.Collections.Generic;
using System.Net;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Net.Udp;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
///     Represents configuration for <see cref="DiscordClient" /> and <see cref="DiscordShardedClient" />.
/// </summary>
public sealed class DiscordConfiguration
{
	/// <summary>
	///     Creates a new configuration with default values.
	/// </summary>
	public DiscordConfiguration()
	{ }

	/// <summary>
	///     Utilized via dependency injection pipeline.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public DiscordConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	///     Creates a clone of another discord configuration.
	/// </summary>
	/// <param name="other">Client configuration to clone.</param>
	public DiscordConfiguration(DiscordConfiguration other)
	{
		this.Token = other.Token;
		this.TokenType = other.TokenType;
		this.ServiceProvider = other.ServiceProvider;
		this.Intents = other.Intents;
		this.HasActivitiesEnabled = other.HasActivitiesEnabled;
#pragma warning disable DCS0002
		this.ActivityHandlerType = other.ActivityHandlerType;
#pragma warning restore DCS0002
		this.EnableBadDomainCheckerSupport = other.EnableBadDomainCheckerSupport;
		this.Proxy = other.Proxy;
		this.EnableLibraryDeveloperMode = other.EnableLibraryDeveloperMode;
		this.Api = new(other.Api ?? new());
		this.Gateway = new(other.Gateway ?? new());
		this.Rest = new(other.Rest ?? new());
		this.Cache = new(other.Cache ?? new());
		this.Logging = new(other.Logging ?? new());
		this.Diagnostics = new(other.Diagnostics ?? new());
		this.Telemetry = new(other.Telemetry ?? new());
	}

	#region Root-level configuration (kept)

	/// <summary>
	///     Sets the token used to identify the client.
	/// </summary>
	public string? Token
	{
		internal get;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentNullException(nameof(value), "Token cannot be null, empty, or all whitespace.");

			field = value.Trim();
		}
	}

	/// <summary>
	///     <para>Sets the type of the token used to identify the client.</para>
	///     <para>Defaults to <see cref="Enums.TokenType.Bot" />.</para>
	/// </summary>
	public TokenType TokenType { internal get; set; } = TokenType.Bot;

	/// <summary>
	///     <para>Sets the gateway intents for this client.</para>
	///     <para>If set, the client will only receive events that they specify with intents.</para>
	///     <para>Defaults to <see cref="DiscordIntents.AllUnprivileged" />.</para>
	/// </summary>
	public DiscordIntents Intents { internal get; set; } = DiscordIntents.AllUnprivileged;

	/// <summary>
	///     <para>Sets the service provider.</para>
	///     <para>This allows passing data around without resorting to static members.</para>
	///     <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; init; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	///     Whether this app uses activities.
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool HasActivitiesEnabled { internal get; set; } = false;

	/// <summary>
	///     <para>Let's you overwrite the activity handler type set by Discord.</para>
	///     <para>Defaults to <see cref="ApplicationCommandHandlerType.DiscordLaunchActivity" />.</para>
	///     <para>Takes no affect if you use the <c>ApplicationCommandExtension.RegisterEntryPointCommand</c> extension method to register your entry point command.</para>
	/// </summary>
	[Deprecated("ActivityHandlerType is a deprecation candidate. Prefer explicit handler registration via ApplicationCommandsExtension.")]
	public ApplicationCommandHandlerType ActivityHandlerType { internal get; set; } = ApplicationCommandHandlerType.DiscordLaunchActivity;

	/// <summary>
	///     Whether to enable bad domain checker support.
	/// </summary>
	/// <remarks>
	///     Enabling this will make the library load bad domain hashes on startup and check URLs against them when needed.
	///     This may slightly increase startup time and memory usage.
	///     When disabled, the bad domain checker will always return false on checks.
	///     We use https://github.com/nager/Nager.PublicSuffix with a cached HTTP rule provider to parse domain names; the public suffix list is cached on disk in the temp directory using the library defaults.
	/// </remarks>
	public bool EnableBadDomainCheckerSupport { get; internal set; } = false;

	/// <summary>
	///     <para>Sets the proxy to use for HTTP and WebSocket connections to Discord.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public IWebProxy? Proxy { internal get; set; } = null;

	/// <summary>
	///     <para>Whether to enable the library developer mode.</para>
	///     <para>Defaults <see langword="false" />.</para>
	/// </summary>
	internal bool EnableLibraryDeveloperMode { get; set; } = false;

	#endregion

	#region Nested configuration

	/// <summary>
	///     <para>API protocol settings (version, channel, locale, timezone).</para>
	/// </summary>
	public ApiConfiguration Api { get; set; } = new();

	/// <summary>
	///     <para>Gateway connection settings (sharding, reconnection, compression, factories).</para>
	/// </summary>
	public GatewayConfiguration Gateway { get; set; } = new();

	/// <summary>
	///     <para>REST client settings (request timeout, rate-limit strategy, proxy).</para>
	/// </summary>
	public RestConfiguration Rest { get; set; } = new();

	/// <summary>
	///     <para>Caching behavior (message cache, presence cache, member caching, auto-fetch).</para>
	/// </summary>
	public CacheConfiguration Cache { get; set; } = new();

	/// <summary>
	///     <para>Logging behavior (log level, timestamp format, logger factory).</para>
	/// </summary>
	public LoggingConfiguration Logging { get; set; } = new();

	/// <summary>
	///     <para>Diagnostics and debugging (payload events, update checks).</para>
	/// </summary>
	public DiagnosticsConfiguration Diagnostics { get; set; } = new();

	/// <summary>
	///     <para>Telemetry and Sentry error reporting configuration.</para>
	/// </summary>
	public TelemetryConfiguration Telemetry { get; set; } = new();

	#endregion

	#region Forwarding properties (deprecated — use nested config instead)

	/// <inheritdoc cref="ApiConfiguration.Version" />
	[Deprecated("Use Api.Version instead. This property will be removed in a future version.")]
	public string ApiVersion
	{
		internal get => this.Api.Version;
		set => this.Api.Version = value;
	}

	/// <inheritdoc cref="ApiConfiguration.Channel" />
	[Deprecated("Use Api.Channel instead. This property will be removed in a future version.")]
	public ApiChannel ApiChannel
	{
		internal get => this.Api.Channel;
		set => this.Api.Channel = value;
	}

	/// <inheritdoc cref="ApiConfiguration.Override" />
	[Deprecated("Use Api.Override instead. This property will be removed in a future version.")]
	public string? Override
	{
		internal get => this.Api.Override;
		set => this.Api.Override = value;
	}

	/// <inheritdoc cref="ApiConfiguration.Locale" />
	[Deprecated("Use Api.Locale instead. This property will be removed in a future version.")]
	public string Locale
	{
		internal get => this.Api.Locale;
		set => this.Api.Locale = value;
	}

	/// <inheritdoc cref="ApiConfiguration.Timezone" />
	[Deprecated("Use Api.Timezone instead. This property will be removed in a future version.")]
	public string? Timezone
	{
		internal get => this.Api.Timezone;
		set => this.Api.Timezone = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.AutoReconnect" />
	[Deprecated("Use Gateway.AutoReconnect instead. This property will be removed in a future version.")]
	public bool AutoReconnect
	{
		internal get => this.Gateway.AutoReconnect;
		set => this.Gateway.AutoReconnect = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.ReconnectIndefinitely" />
	[Deprecated("Use Gateway.ReconnectIndefinitely instead. This property will be removed in a future version.")]
	public bool ReconnectIndefinitely
	{
		internal get => this.Gateway.ReconnectIndefinitely;
		set => this.Gateway.ReconnectIndefinitely = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.ShardId" />
	[Deprecated("Use Gateway.ShardId instead. This property will be removed in a future version.")]
	public int ShardId
	{
		internal get => this.Gateway.ShardId;
		set => this.Gateway.ShardId = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.ShardCount" />
	[Deprecated("Use Gateway.ShardCount instead. This property will be removed in a future version.")]
	public int ShardCount
	{
		internal get => this.Gateway.ShardCount;
		set => this.Gateway.ShardCount = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.CompressionLevel" />
	[Deprecated("Use Gateway.CompressionLevel instead. This property will be removed in a future version.")]
	public GatewayCompressionLevel GatewayCompressionLevel
	{
		internal get => this.Gateway.CompressionLevel;
		set => this.Gateway.CompressionLevel = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.LargeThreshold" />
	[Deprecated("Use Gateway.LargeThreshold instead. This property will be removed in a future version.")]
	public int LargeThreshold
	{
		internal get => this.Gateway.LargeThreshold;
		set => this.Gateway.LargeThreshold = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.Capabilities" />
	[Deprecated("Use Gateway.Capabilities instead. This property will be removed in a future version.")]
	public GatewayCapabilities Capabilities
	{
		internal get => this.Gateway.Capabilities;
		set => this.Gateway.Capabilities = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.MobileStatus" />
	[Deprecated("Use Gateway.MobileStatus instead. This property will be removed in a future version.")]
	public bool MobileStatus
	{
		internal get => this.Gateway.MobileStatus;
		set => this.Gateway.MobileStatus = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.WebSocketClientFactory" />
	[Deprecated("Use Gateway.WebSocketClientFactory instead. This property will be removed in a future version.")]
	public WebSocketClientFactoryDelegate WebSocketClientFactory
	{
		internal get => this.Gateway.WebSocketClientFactory;
		set => this.Gateway.WebSocketClientFactory = value;
	}

	/// <inheritdoc cref="GatewayConfiguration.UdpClientFactory" />
	[Deprecated("Use Gateway.UdpClientFactory instead. This property will be removed in a future version.")]
	public UdpClientFactoryDelegate UdpClientFactory
	{
		internal get => this.Gateway.UdpClientFactory;
		set => this.Gateway.UdpClientFactory = value;
	}

	/// <inheritdoc cref="RestConfiguration.RequestTimeout" />
	[Deprecated("Use Rest.RequestTimeout instead. This property will be removed in a future version.")]
	public TimeSpan HttpTimeout
	{
		internal get => this.Rest.RequestTimeout;
		set => this.Rest.RequestTimeout = value;
	}

	/// <inheritdoc cref="RestConfiguration.UseRelativeRatelimit" />
	[Deprecated("Use Rest.UseRelativeRatelimit instead. This property will be removed in a future version.")]
	public bool UseRelativeRatelimit
	{
		internal get => this.Rest.UseRelativeRatelimit;
		set => this.Rest.UseRelativeRatelimit = value;
	}

	/// <inheritdoc cref="CacheConfiguration.MessageCacheSize" />
	[Deprecated("Use Cache.MessageCacheSize instead. This property will be removed in a future version.")]
	public int MessageCacheSize
	{
		internal get => this.Cache.MessageCacheSize;
		set => this.Cache.MessageCacheSize = value;
	}

	/// <inheritdoc cref="CacheConfiguration.PresenceCacheSize" />
	[Deprecated("Use Cache.PresenceCacheSize instead. This property will be removed in a future version.")]
	public int PresenceCacheSize
	{
		internal get => this.Cache.PresenceCacheSize;
		set => this.Cache.PresenceCacheSize = value;
	}

	/// <inheritdoc cref="CacheConfiguration.AlwaysCacheMembers" />
	[Deprecated("Use Cache.AlwaysCacheMembers instead. This property will be removed in a future version.")]
	public bool AlwaysCacheMembers
	{
		internal get => this.Cache.AlwaysCacheMembers;
		set => this.Cache.AlwaysCacheMembers = value;
	}

	/// <inheritdoc cref="CacheConfiguration.AutoRefreshChannelCache" />
	[Deprecated("Use Cache.AutoRefreshChannelCache instead. This property will be removed in a future version.")]
	public bool AutoRefreshChannelCache
	{
		internal get => this.Cache.AutoRefreshChannelCache;
		set => this.Cache.AutoRefreshChannelCache = value;
	}

	/// <inheritdoc cref="CacheConfiguration.AutoFetchApplicationEmojis" />
	[Deprecated("Use Cache.AutoFetchApplicationEmojis instead. This property will be removed in a future version.")]
	public bool AutoFetchApplicationEmojis
	{
		internal get => this.Cache.AutoFetchApplicationEmojis;
		set => this.Cache.AutoFetchApplicationEmojis = value;
	}

	/// <inheritdoc cref="CacheConfiguration.AutoFetchSkuIds" />
	[Deprecated("Use Cache.AutoFetchSkuIds instead. This property will be removed in a future version.")]
	[RequiresFeature(Features.MonetizedApplication)]
	public bool AutoFetchSkuIds
	{
		internal get => this.Cache.AutoFetchSkuIds;
		set => this.Cache.AutoFetchSkuIds = value;
	}

	/// <inheritdoc cref="CacheConfiguration.SkuId" />
	[Deprecated("Use Cache.SkuId instead. This property will be removed in a future version.")]
	[RequiresFeature(Features.MonetizedApplication)]
	public ulong? SkuId
	{
		internal get => this.Cache.SkuId;
		set => this.Cache.SkuId = value;
	}

	/// <inheritdoc cref="LoggingConfiguration.MinimumLogLevel" />
	[Deprecated("Use Logging.MinimumLogLevel instead. This property will be removed in a future version.")]
	public LogLevel MinimumLogLevel
	{
		internal get => this.Logging.MinimumLogLevel;
		set => this.Logging.MinimumLogLevel = value;
	}

	/// <inheritdoc cref="LoggingConfiguration.LogTimestampFormat" />
	[Deprecated("Use Logging.LogTimestampFormat instead. This property will be removed in a future version.")]
	public string LogTimestampFormat
	{
		internal get => this.Logging.LogTimestampFormat;
		set => this.Logging.LogTimestampFormat = value;
	}

	/// <inheritdoc cref="LoggingConfiguration.LoggerFactory" />
	[Deprecated("Use Logging.LoggerFactory instead. This property will be removed in a future version.")]
	public ILoggerFactory LoggerFactory
	{
		internal get => this.Logging.LoggerFactory;
		set => this.Logging.LoggerFactory = value;
	}

	/// <inheritdoc cref="LoggingConfiguration.HasShardLogger" />
	internal bool HasShardLogger
	{
		get => this.Logging.HasShardLogger;
		set => this.Logging.HasShardLogger = value;
	}

	/// <inheritdoc cref="DiagnosticsConfiguration.EnablePayloadReceivedEvent" />
	[Deprecated("Use Diagnostics.EnablePayloadReceivedEvent instead. This property will be removed in a future version.")]
	public bool EnablePayloadReceivedEvent
	{
		internal get => this.Diagnostics.EnablePayloadReceivedEvent;
		set => this.Diagnostics.EnablePayloadReceivedEvent = value;
	}

	/// <inheritdoc cref="UpdateCheckConfiguration.Disabled" />
	[Deprecated("Use Diagnostics.UpdateChecks.Disabled instead. This property will be removed in a future version.")]
	public bool DisableUpdateCheck
	{
		internal get => this.Diagnostics.UpdateChecks.Disabled;
		set => this.Diagnostics.UpdateChecks.Disabled = value;
	}

	/// <inheritdoc cref="UpdateCheckConfiguration.Mode" />
	[Deprecated("Use Diagnostics.UpdateChecks.Mode instead. This property will be removed in a future version.")]
	public VersionCheckMode UpdateCheckMode
	{
		internal get => this.Diagnostics.UpdateChecks.Mode;
		set => this.Diagnostics.UpdateChecks.Mode = value;
	}

	/// <inheritdoc cref="UpdateCheckConfiguration.IncludePrerelease" />
	[Deprecated("Use Diagnostics.UpdateChecks.IncludePrerelease instead. This property will be removed in a future version.")]
	public bool IncludePrereleaseInUpdateCheck
	{
		internal get => this.Diagnostics.UpdateChecks.IncludePrerelease;
		set => this.Diagnostics.UpdateChecks.IncludePrerelease = value;
	}

	/// <inheritdoc cref="UpdateCheckConfiguration.GitHubToken" />
	[Deprecated("Use Diagnostics.UpdateChecks.GitHubToken instead. This property will be removed in a future version.")]
	public string? UpdateCheckGitHubToken
	{
		internal get => this.Diagnostics.UpdateChecks.GitHubToken;
		set => this.Diagnostics.UpdateChecks.GitHubToken = value;
	}

	/// <inheritdoc cref="UpdateCheckConfiguration.ShowReleaseNotes" />
	[Deprecated("Use Diagnostics.UpdateChecks.ShowReleaseNotes instead. This property will be removed in a future version.")]
	public bool ShowReleaseNotesInUpdateCheck
	{
		internal get => this.Diagnostics.UpdateChecks.ShowReleaseNotes;
		set => this.Diagnostics.UpdateChecks.ShowReleaseNotes = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.EnableSentry" />
	[Deprecated("Use Telemetry.EnableSentry instead. This property will be removed in a future version.")]
	public bool EnableSentry
	{
		internal get => this.Telemetry.EnableSentry;
		set => this.Telemetry.EnableSentry = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.AttachRecentLogEntries" />
	[Deprecated("Use Telemetry.AttachRecentLogEntries instead. This property will be removed in a future version.")]
	public bool AttachRecentLogEntries
	{
		internal get => this.Telemetry.AttachRecentLogEntries;
		set => this.Telemetry.AttachRecentLogEntries = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.AttachUserInfo" />
	[Deprecated("Use Telemetry.AttachUserInfo instead. This property will be removed in a future version.")]
	public bool AttachUserInfo
	{
		internal get => this.Telemetry.AttachUserInfo;
		set => this.Telemetry.AttachUserInfo = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.FeedbackEmail" />
	[Deprecated("Use Telemetry.FeedbackEmail instead. This property will be removed in a future version.")]
	public string? FeedbackEmail
	{
		internal get => this.Telemetry.FeedbackEmail;
		set => this.Telemetry.FeedbackEmail = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.DeveloperUserId" />
	[Deprecated("Use Telemetry.DeveloperUserId instead. This property will be removed in a future version.")]
	public ulong? DeveloperUserId
	{
		internal get => this.Telemetry.DeveloperUserId;
		set => this.Telemetry.DeveloperUserId = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.EnableDiscordIdScrubber" />
	[Deprecated("Use Telemetry.EnableDiscordIdScrubber instead. This property will be removed in a future version.")]
	public bool EnableDiscordIdScrubber
	{
		internal get => this.Telemetry.EnableDiscordIdScrubber;
		set => this.Telemetry.EnableDiscordIdScrubber = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.TrackExceptions" />
	internal List<Type> TrackExceptions
	{
		get => this.Telemetry.TrackExceptions;
		set
		{
			if (!this.EnableLibraryDeveloperMode)
				throw new AccessViolationException("Cannot set this as non-library-dev");

			this.Telemetry.TrackExceptions = value;
		}
	}

	/// <inheritdoc cref="TelemetryConfiguration.DisableScrubber" />
	internal bool DisableScrubber
	{
		get => this.Telemetry.DisableScrubber;
		set => this.Telemetry.DisableScrubber = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.SentryDebug" />
	internal bool SentryDebug
	{
		get => this.Telemetry.SentryDebug;
		set => this.Telemetry.SentryDebug = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.DisableExceptionFilter" />
	internal bool DisableExceptionFilter
	{
		get => this.Telemetry.DisableExceptionFilter;
		set => this.Telemetry.DisableExceptionFilter = value;
	}

	/// <inheritdoc cref="TelemetryConfiguration.CustomSentryDsn" />
	internal string? CustomSentryDsn
	{
		get => this.Telemetry.CustomSentryDsn;
		set => this.Telemetry.CustomSentryDsn = value;
	}

	#endregion

	/// <summary>
	///     Validates cross-property constraints that cannot be checked in individual property setters.
	/// </summary>
	internal void Validate()
		=> this.Gateway.Validate();
}

