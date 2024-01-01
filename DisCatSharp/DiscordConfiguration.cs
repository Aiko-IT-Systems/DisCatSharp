using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Udp;
using DisCatSharp.Net.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents configuration for <see cref="DiscordClient"/> and <see cref="DiscordShardedClient"/>.
/// </summary>
public sealed class DiscordConfiguration
{
	/// <summary>
	/// Sets the token used to identify the client.
	/// </summary>
	public string? Token
	{
		internal get => this._token;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentNullException(nameof(value), "Token cannot be null, empty, or all whitespace.");

			this._token = value.Trim();
		}
	}

	/// <summary>
	/// Sets the token used to identify the client (protected).
	/// </summary>
	private string? _token;

	/// <summary>
	/// <para>Sets the type of the token used to identify the client.</para>
	/// <para>Defaults to <see cref="TokenType.Bot"/>.</para>
	/// </summary>
	public TokenType TokenType { internal get; set; } = TokenType.Bot;

	/// <summary>
	/// <para>Sets the minimum logging level for messages.</para>
	/// <para>Defaults to <see cref="LogLevel.Information"/>.</para>
	/// </summary>
	public LogLevel MinimumLogLevel { internal get; set; } = LogLevel.Information;

	/// <summary>
	/// Overwrites the api version.
	/// Defaults to 10.
	/// </summary>
	public string ApiVersion { internal get; set; } = "10";

	/// <summary>
	/// <para>Sets whether to rely on Discord for NTP (Network Time Protocol) synchronization with the "X-Ratelimit-Reset-After" header.</para>
	/// <para>If the system clock is unsynced, setting this to true will ensure ratelimits are synced with Discord and reduce the risk of hitting one.</para>
	/// <para>This should only be set to false if the system clock is synced with NTP.</para>
	/// <para>Defaults to <see langword="true"/>.</para>
	/// </summary>
	public bool UseRelativeRatelimit { internal get; set; } = true;

	/// <summary>
	/// <para>Allows you to overwrite the time format used by the internal debug logger.</para>
	/// <para>Only applicable when <see cref="LoggerFactory"/> is set left at default value. Defaults to ISO 8601-like format.</para>
	/// </summary>
	public string LogTimestampFormat { internal get; set; } = "yyyy-MM-dd HH:mm:ss zzz";

	/// <summary>
	/// <para>Sets the member count threshold at which guilds are considered large.</para>
	/// <para>Defaults to 250.</para>
	/// </summary>
	public int LargeThreshold { internal get; set; } = 250;

	/// <summary>
	/// <para>Sets whether to automatically reconnect in case a connection is lost.</para>
	/// <para>Defaults to <see langword="true"/>.</para>
	/// </summary>
	public bool AutoReconnect { internal get; set; } = true;

	/// <summary>
	/// <para>Sets the ID of the shard to connect to.</para>
	/// <para>If not sharding, or sharding automatically, this value should be left with the default value of 0.</para>
	/// </summary>
	public int ShardId { internal get; set; } = 0;

	/// <summary>
	/// <para>Sets the total number of shards the bot is on. If not sharding, this value should be left with a default value of 1.</para>
	/// <para>If sharding automatically, this value will indicate how many shards to boot. If left default for automatic sharding, the client will determine the shard count automatically.</para>
	/// </summary>
	public int ShardCount { internal get; set; } = 1;

	/// <summary>
	/// <para>Sets the level of compression for WebSocket traffic.</para>
	/// <para>Disabling this option will increase the amount of traffic sent via WebSocket. Setting <see cref="GatewayCompressionLevel.Payload"/> will enable compression for READY and GUILD_CREATE payloads. Setting <see cref="GatewayCompressionLevel.Stream"/> will enable compression for the entire WebSocket stream, drastically reducing amount of traffic.</para>
	/// <para>Defaults to <see cref="GatewayCompressionLevel.Stream"/>.</para>
	/// </summary>
	public GatewayCompressionLevel GatewayCompressionLevel { internal get; set; } = GatewayCompressionLevel.Stream;

	/// <summary>
	/// <para>Sets the size of the global message cache.</para>
	/// <para>Setting this to 0 will disable message caching entirely.</para>
	/// <para>Defaults to 1024.</para>
	/// </summary>
	public int MessageCacheSize { internal get; set; } = 1024;

	/// <summary>
	/// <para>Sets the proxy to use for HTTP and WebSocket connections to Discord.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public IWebProxy? Proxy { internal get; set; } = null;

	/// <summary>
	/// <para>Sets the timeout for HTTP requests.</para>
	/// <para>Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts.</para>
	/// <para>Defaults to 20 seconds.</para>
	/// </summary>
	public TimeSpan HttpTimeout { internal get; set; } = TimeSpan.FromSeconds(20);

	/// <summary>
	/// <para>Defines that the client should attempt to reconnect indefinitely.</para>
	/// <para>This is typically a very bad idea to set to <c>true</c>, as it will swallow all connection errors.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool ReconnectIndefinitely { internal get; set; } = false;

	/// <summary>
	/// Sets whether the client should attempt to cache members if exclusively using unprivileged intents.
	/// <para>
	///     This will only take effect if there are no <see cref="DiscordIntents.GuildMembers"/> or <see cref="DiscordIntents.GuildPresences"/>
	///     intents specified. Otherwise, this will always be overwritten to true.
	/// </para>
	/// <para>Defaults to <see langword="true"/>.</para>
	/// </summary>
	public bool AlwaysCacheMembers { internal get; set; } = true;

	/// <summary>
	/// Sets whether a shard logger is attached.
	/// </summary>
	internal bool HasShardLogger { get; set; } = false;

	/// <summary>
	/// <para>Sets the gateway intents for this client.</para>
	/// <para>If set, the client will only receive events that they specify with intents.</para>
	/// <para>Defaults to <see cref="DiscordIntents.AllUnprivileged"/>.</para>
	/// </summary>
	public DiscordIntents Intents { internal get; set; } = DiscordIntents.AllUnprivileged;

	/// <summary>
	/// <para>Sets the factory method used to create instances of WebSocket clients.</para>
	/// <para>Use <see cref="WebSocketClient.CreateNew"/> and equivalents on other implementations to switch out client implementations.</para>
	/// <para>Defaults to <see cref="WebSocketClient.CreateNew"/>.</para>
	/// </summary>
	public WebSocketClientFactoryDelegate WebSocketClientFactory
	{
		internal get => this._webSocketClientFactory;
		set => this._webSocketClientFactory = value ?? throw new InvalidOperationException("You need to supply a valid WebSocket client factory method.");
	}

	/// <summary>
	/// Sets the factory method and creates a new instances of the <see cref="WebSocketClient"/>.
	/// </summary>
	private WebSocketClientFactoryDelegate _webSocketClientFactory = WebSocketClient.CreateNew;

	/// <summary>
	/// <para>Sets the factory method used to create instances of UDP clients.</para>
	/// <para>Use <see cref="DcsUdpClient.CreateNew"/> and equivalents on other implementations to switch out client implementations.</para>
	/// <para>Defaults to <see cref="DcsUdpClient.CreateNew"/>.</para>
	/// </summary>
	public UdpClientFactoryDelegate UdpClientFactory
	{
		internal get => this._udpClientFactory;
		set => this._udpClientFactory = value ?? throw new InvalidOperationException("You need to supply a valid UDP client factory method.");
	}

	/// <summary>
	/// Sets the factory method and creates a new instances of the <see cref="DcsUdpClient"/>.
	/// </summary>
	private UdpClientFactoryDelegate _udpClientFactory = DcsUdpClient.CreateNew;

	/// <summary>
	/// <para>Sets the logger implementation to use.</para>
	/// <para>To create your own logger, implement the <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> instance.</para>
	/// <para>Defaults to built-in implementation.</para>
	/// </summary>
	public ILoggerFactory LoggerFactory { internal get; set; } = null!;

	/// <summary>
	/// <para>Sets if the bot's status should show the mobile icon.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool MobileStatus { internal get; set; } = false;

	/// <summary>
	/// <para>Which api channel to use.</para>
	/// <para>Defaults to <see cref="ApiChannel.Stable"/>.</para>
	/// </summary>
	public ApiChannel ApiChannel { internal get; set; } = ApiChannel.Stable;

	/// <summary>
	/// <para>Refresh full guild channel cache.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool AutoRefreshChannelCache { internal get; set; } = false;

	/// <summary>
	/// <para>Do not use, this is meant for DisCatSharp Devs.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public string? Override { internal get; set; } = null;

	/// <summary>
	/// Sets your preferred API language. See <see cref="DiscordLocales" /> for valid locales.
	/// </summary>
	public string Locale { internal get; set; } = DiscordLocales.AMERICAN_ENGLISH;

	/// <summary>
	/// Sets your timezone.
	/// </summary>
	public string? Timezone { internal get; set; } = null;

	/// <summary>
	/// <para>Whether to report missing fields for discord object.</para>
	/// <para>Useful for library development.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool ReportMissingFields { internal get; set; } = false;

	/// <summary>
	/// <para>Sets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { internal get; init; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// <para>Whether to report missing fields for discord object.</para>
	/// <para>This helps us to track missing data and library bugs better.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool EnableSentry { internal get; set; } = false;

	/// <summary>
	/// <para>Whether to attach the bots username and id to sentry reports.</para>
	/// <para>This helps us to pinpoint problems.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool AttachUserInfo { internal get; set; } = false;

	/// <summary>
	/// <para>Your email address we can reach out when your bot encounters library bugs.</para>
	/// <para>Will only be transmitted if <see cref="AttachUserInfo"/> is <see langword="true"/>.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public string? FeedbackEmail { internal get; set; } = null;

	/// <summary>
	/// <para>Your discord user id we can reach out when your bot encounters library bugs.</para>
	/// <para>Will only be transmitted if <see cref="AttachUserInfo"/> is <see langword="true"/>.</para>
	/// <para>Defaults to <see langword="null"/>.</para>
	/// </summary>
	public ulong? DeveloperUserId { internal get; set; } = null;

	/// <summary>
	/// <para>Causes the <see cref="DiscordClient.PayloadReceived"/> event to be fired.</para>
	/// <para>Useful if you want to work with raw events.</para>
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool EnablePayloadReceivedEvent { internal get; set; } = false;

	/// <summary>
	/// <para>Sets which exceptions to track with sentry.</para>
	/// <para>Do not touch this unless you're developing the library.</para>
	/// </summary>
	/// <exception cref="InvalidOperationException">Thrown when the base type of all exceptions is not <see cref="DisCatSharpException"/>.</exception>
	internal List<Type> TrackExceptions
	{
		get => this._exceptions;
		set
		{
			if (!this.EnableLibraryDeveloperMode)
				throw new AccessViolationException("Cannot set this as non-library-dev");
			else if (value == null)
				this._exceptions.Clear();
			else
				this._exceptions = value.All(val => val.BaseType == typeof(DisCatSharpException))
					? value
					: throw new InvalidOperationException("Can only track exceptions who inherit from " + nameof(DisCatSharpException) + " and must be constructed with typeof(Type)");
		}
	}

	/// <summary>
	/// The exception we track with sentry.
	/// </summary>
	private List<Type> _exceptions = [typeof(ServerErrorException), typeof(BadRequestException)];

	/// <summary>
	/// <para>Whether to enable the library developer mode.</para>
	/// <para>Defaults <see langword="false"/>.</para>
	/// </summary>
	internal bool EnableLibraryDeveloperMode { get; set; } = false;

	/// <summary>
	/// Whether to turn sentry's debug mode on.
	/// </summary>
	internal bool SentryDebug { get; set; } = false;

	/// <summary>
	/// Whether to disable the exception filter.
	/// </summary>
	internal bool DisableExceptionFilter { get; set; } = false;

	/// <summary>
	/// Custom Sentry Dsn.
	/// </summary>
	internal string? CustomSentryDsn { get; set; } = null;

	/// <summary>
	/// Whether to autofetch the sku ids.
	/// <para>Mutually exclusive to <see cref="SkuId"/> and <see cref="TestSkuId"/>.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public bool AutoFetchSkuIds { internal get; set; } = false;

	/// <summary>
	/// The applications sku id for premium apps.
	/// <para>Mutually exclusive to <see cref="AutoFetchSkuIds"/>.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public ulong? SkuId { internal get; set; } = null;

	/// <summary>
	/// The applications test sku id for premium apps.
	/// <para>Mutually exclusive to <see cref="AutoFetchSkuIds"/>.</para>
	/// </summary>
	[RequiresFeature(Features.MonetizedApplication)]
	public ulong? TestSkuId { internal get; set; } = null;

	/// <summary>
	/// Whether to disable the update check.
	/// </summary>
	public bool DisableUpdateCheck { internal get; set; } = false;

	/// <summary>
	/// Against which channel to check for updates.
	/// <para>Defaults to <see cref="VersionCheckMode.NuGet"/>.</para>
	/// </summary>
	public VersionCheckMode UpdateCheckMode { internal get; set; } = VersionCheckMode.NuGet;

	/// <summary>
	/// Whether to include prerelease versions in the update check.
	/// </summary>
	public bool IncludePrereleaseInUpdateCheck { get; internal set; } = true;

	/// <summary>
	/// Sets the GitHub token to use for the update check.
	/// <para>Only useful if extensions are private and <see cref="UpdateCheckMode"/> is <see cref="VersionCheckMode.GitHub"/>.</para>
	/// </summary>
	public string? UpdateCheckGitHubToken { get; set; } = null;

	/// <summary>
	/// Whether to show release notes in the update check.
	/// <para>Defaults to <see langword="false"/>.</para>
	/// </summary>
	public bool ShowReleaseNotesInUpdateCheck { get; set; } = false;

	/// <summary>
	/// Creates a new configuration with default values.
	/// </summary>
	public DiscordConfiguration()
	{ }

	/// <summary>
	/// Utilized via dependency injection pipeline.
	/// </summary>
	/// <param name="provider">The service provider.</param>
	[ActivatorUtilitiesConstructor]
	public DiscordConfiguration(IServiceProvider provider)
	{
		this.ServiceProvider = provider;
	}

	/// <summary>
	/// Creates a clone of another discord configuration.
	/// </summary>
	/// <param name="other">Client configuration to clone.</param>
	public DiscordConfiguration(DiscordConfiguration other)
	{
		this.Token = other.Token;
		this.TokenType = other.TokenType;
		this.MinimumLogLevel = other.MinimumLogLevel;
		this.UseRelativeRatelimit = other.UseRelativeRatelimit;
		this.LogTimestampFormat = other.LogTimestampFormat;
		this.LargeThreshold = other.LargeThreshold;
		this.AutoReconnect = other.AutoReconnect;
		this.ShardId = other.ShardId;
		this.ShardCount = other.ShardCount;
		this.GatewayCompressionLevel = other.GatewayCompressionLevel;
		this.MessageCacheSize = other.MessageCacheSize;
		this.WebSocketClientFactory = other.WebSocketClientFactory;
		this.UdpClientFactory = other.UdpClientFactory;
		this.Proxy = other.Proxy;
		this.HttpTimeout = other.HttpTimeout;
		this.ReconnectIndefinitely = other.ReconnectIndefinitely;
		this.Intents = other.Intents;
		this.LoggerFactory = other.LoggerFactory;
		this.MobileStatus = other.MobileStatus;
		this.AutoRefreshChannelCache = other.AutoRefreshChannelCache;
		this.ApiVersion = other.ApiVersion;
		this.ServiceProvider = other.ServiceProvider;
		this.Override = other.Override;
		this.Locale = other.Locale;
		this.Timezone = other.Timezone;
		this.ReportMissingFields = other.ReportMissingFields;
		this.EnableSentry = other.EnableSentry;
		this.AttachUserInfo = other.AttachUserInfo;
		this.FeedbackEmail = other.FeedbackEmail;
		this.DeveloperUserId = other.DeveloperUserId;
		this.HasShardLogger = other.HasShardLogger;
		this._exceptions = other._exceptions;
		this.EnableLibraryDeveloperMode = other.EnableLibraryDeveloperMode;
		this.SentryDebug = other.SentryDebug;
		this.DisableExceptionFilter = other.DisableExceptionFilter;
		this.CustomSentryDsn = other.CustomSentryDsn;
		this.EnablePayloadReceivedEvent = other.EnablePayloadReceivedEvent;
		this.AutoFetchSkuIds = other.AutoFetchSkuIds;
		this.SkuId = other.SkuId;
		this.TestSkuId = other.TestSkuId;
		this.DisableUpdateCheck = other.DisableUpdateCheck;
		this.UpdateCheckMode = other.UpdateCheckMode;
		this.IncludePrereleaseInUpdateCheck = other.IncludePrereleaseInUpdateCheck;
		this.UpdateCheckGitHubToken = other.UpdateCheckGitHubToken;
		this.ShowReleaseNotesInUpdateCheck = other.ShowReleaseNotesInUpdateCheck;
	}
}
