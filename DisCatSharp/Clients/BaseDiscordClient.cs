using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using Sentry;

namespace DisCatSharp;

/// <summary>
/// Represents a common base for various Discord Client implementations.
/// </summary>
public abstract class BaseDiscordClient : IDisposable
{
	/// <summary>
	/// Gets the api client.
	/// </summary>
	protected internal DiscordApiClient ApiClient { get; }

	/// <summary>
	/// Gets the sentry client.
	/// </summary>
	internal SentryClient Sentry { get; set; }

	/// <summary>
	/// Gets the sentry dsn.
	/// </summary>
	internal static string SentryDsn { get; set; } = "https://1da216e26a2741b99e8ccfccea1b7ac8@o1113828.ingest.sentry.io/4504901362515968";

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	protected internal DiscordConfiguration Configuration { get; }

	/// <summary>
	/// Gets the instance of the logger for this client.
	/// </summary>
	public ILogger<BaseDiscordClient> Logger { get; internal set; }

	/// <summary>
	/// Gets the string representing the version of bot lib.
	/// </summary>
	public string VersionString { get; }

	/// <summary>
	/// Gets the bot library name.
	/// </summary>
	public string BotLibrary
		=> "DisCatSharp";

	/// <summary>
	/// Gets the current user.
	/// </summary>
	public DiscordUser CurrentUser { get; internal set; }

	/// <summary>
	/// Gets the current application.
	/// </summary>
	public DiscordApplication CurrentApplication { get; internal set; }

	/// <summary>
	/// Exposes a <see cref="HttpClient">Http Client</see> for custom operations.
	/// </summary>
	public HttpClient RestClient { get; internal set; }

	/// <summary>
	/// Gets the cached guilds for this client.
	/// </summary>
	public abstract IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

	/// <summary>
	/// Gets the cached users for this client.
	/// </summary>
	public ConcurrentDictionary<ulong, DiscordUser> UserCache { get; internal set; }

	/// <summary>
	/// <para>Gets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to null.</para>
	/// </summary>
	internal IServiceProvider ServiceProvider { get; set; }

	/// <summary>
	/// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
	/// </summary>
	public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
		=> this.VoiceRegionsLazy.Value;

	/// <summary>
	/// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
	/// </summary>
	protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }

	/// <summary>
	/// Gets the lazy voice regions.
	/// </summary>
	internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> VoiceRegionsLazy;

	/// <summary>
	/// Initializes this Discord API client.
	/// </summary>
	/// <param name="config">Configuration for this client.</param>
	protected BaseDiscordClient(DiscordConfiguration config)
	{
		this.Configuration = new(config);
		this.ServiceProvider = config.ServiceProvider;
		if (this.Configuration.CustomSentryDsn != null)
			SentryDsn = this.Configuration.CustomSentryDsn;
		if (this.ServiceProvider != null)
		{
			this.Configuration.LoggerFactory ??= config.ServiceProvider.GetService<ILoggerFactory>()!;
			this.Logger = config.ServiceProvider.GetService<ILogger<BaseDiscordClient>>()!;
		}

		if (this.Configuration.LoggerFactory == null && !this.Configuration.EnableSentry)
		{
			this.Configuration.LoggerFactory = new DefaultLoggerFactory();
			this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
		}
		else if (this.Configuration.LoggerFactory == null && this.Configuration.EnableSentry)
		{
			var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>(string.Empty, x =>
			{
#pragma warning disable CS0618 // Type or member is obsolete
				x.TimestampFormat = this.Configuration.LogTimestampFormat;
#pragma warning restore CS0618 // Type or member is obsolete
				x.LogToStandardErrorThreshold = this.Configuration.MinimumLogLevel;
			});
			var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
			var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());
			/*
			var configureFormatterOptions = new ConfigureNamedOptions<ConsoleFormatterOptions>(string.Empty, x => { x.TimestampFormat = this.Configuration.LogTimestampFormat; });
			var formatterFactory = new OptionsFactory<ConsoleFormatterOptions>(new[] { configureFormatterOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleFormatterOptions>>());
			var formatterMonitor = new OptionsMonitor<ConsoleFormatterOptions>(formatterFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleFormatterOptions>>(), new OptionsCache<ConsoleFormatterOptions>());
			*/

			var l = new ConsoleLoggerProvider(optionsMonitor);
			this.Configuration.LoggerFactory = new LoggerFactory();
			this.Configuration.LoggerFactory.AddProvider(l);
		}

		var ass = typeof(DiscordClient).GetTypeInfo().Assembly;
		var vrs = "";
		var ivr = ass.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (ivr != null)
			vrs = ivr.InformationalVersion;
		else
		{
			var v = ass.GetName().Version;
			vrs = v?.ToString(3);
		}

		if (!this.Configuration.HasShardLogger)
			if (this.Configuration is { LoggerFactory: not null, EnableSentry: true })
				this.Configuration.LoggerFactory.AddSentry(o =>
				{
					o.InitializeSdk = true;
					o.Dsn = SentryDsn;
					o.DetectStartupTime = StartupTimeDetectionMode.Fast;
					o.DiagnosticLevel = SentryLevel.Debug;
					o.Environment = "dev";
					o.IsGlobalModeEnabled = false;
					o.TracesSampleRate = 1.0;
					o.ReportAssembliesMode = ReportAssembliesMode.InformationalVersion;
					o.AddInAppInclude("DisCatSharp");
					o.AttachStacktrace = true;
					o.StackTraceMode = StackTraceMode.Enhanced;
					o.Release = $"{this.BotLibrary}@{vrs}";
					o.SendClientReports = true;
					o.IsEnvironmentUser = false;
					o.UseAsyncFileIO = true;
					o.EnableScopeSync = true;
					if (!this.Configuration.DisableExceptionFilter)
						o.AddExceptionFilter(new DisCatSharpExceptionFilter(this.Configuration));
					o.Debug = this.Configuration.SentryDebug;
					o.SetBeforeSend((e, _) =>
					{
						if (!this.Configuration.DisableExceptionFilter)
						{
							if (e.Exception != null)
							{
								if (!this.Configuration.TrackExceptions.Contains(e.Exception.GetType()))
									return null;
							}
							else if (e.Extra.Count == 0 || !e.Extra.ContainsKey("Found Fields"))
								return null;
						}

						if (!e.HasUser())
							if (this.Configuration.AttachUserInfo && this.CurrentUser! != null!)
								e.User = new()
								{
									Id = this.CurrentUser.Id.ToString(),
									Username = this.CurrentUser.UsernameWithDiscriminator,
									Other = new Dictionary<string, string>()
									{
										{ "developer", this.Configuration.DeveloperUserId?.ToString() ?? "not_given" },
										{ "email", this.Configuration.FeedbackEmail ?? "not_given" }
									}
								};
						return e;
					});
				});

		if (this.Configuration.EnableSentry)
		{
			SentryOptions options = new()
			{
				DetectStartupTime = StartupTimeDetectionMode.Fast,
				DiagnosticLevel = SentryLevel.Debug,
				Environment = "dev",
				IsGlobalModeEnabled = false,
				TracesSampleRate = 1.0,
				ReportAssembliesMode = ReportAssembliesMode.InformationalVersion,
				Dsn = SentryDsn,
				AttachStacktrace = true,
				StackTraceMode = StackTraceMode.Enhanced,
				SendClientReports = true,
				Release = $"{this.BotLibrary}@{vrs}",
				IsEnvironmentUser = false,
				UseAsyncFileIO = true,
				EnableScopeSync = true,
				Debug = this.Configuration.SentryDebug
			};
			options.SetBeforeSend((e, _) =>
			{
				if (!this.Configuration.DisableExceptionFilter)
				{
					if (e.Exception != null)
					{
						if (!this.Configuration.TrackExceptions.Contains(e.Exception.GetType()))
							return null;
					}
					else if (e.Extra.Count == 0 || !e.Extra.ContainsKey("Found Fields"))
						return null;
				}

				if (!e.HasUser())
					if (this.Configuration.AttachUserInfo && this.CurrentUser! != null!)
						e.User = new()
						{
							Id = this.CurrentUser.Id.ToString(),
							Username = this.CurrentUser.UsernameWithDiscriminator,
							Other = new Dictionary<string, string>()
							{
								{ "developer", this.Configuration.DeveloperUserId?.ToString() ?? "not_given" },
								{ "email", this.Configuration.FeedbackEmail ?? "not_given" }
							}
						};

				if (!e.Extra.ContainsKey("Found Fields"))
					e.SetFingerprint(GenerateSentryFingerPrint(e));
				return e;
			});
			this.Sentry = new(options);
		}

		this.Logger ??= this.Configuration.LoggerFactory!.CreateLogger<BaseDiscordClient>();

		this.ApiClient = new(this);
		this.UserCache = new();
		this.InternalVoiceRegions = new();
		this.VoiceRegionsLazy = new(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));

		this.RestClient = new();
		this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
		this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("x-discord-locale", this.Configuration.Locale);
		if (!string.IsNullOrWhiteSpace(this.Configuration.Timezone))
			this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("x-discord-timezone", this.Configuration.Timezone);
		if (this.Configuration.Override != null)
			this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("x-super-properties", this.Configuration.Override);

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
	}

	/// <summary>
	/// Gets the current API application.
	/// </summary>
	public async Task<DiscordApplication> GetCurrentApplicationAsync()
	{
		var tapp = await this.ApiClient.GetCurrentApplicationOauth2InfoAsync().ConfigureAwait(false);
		var app = new DiscordApplication
		{
			Discord = this,
			Id = tapp.Id,
			Name = tapp.Name,
			Description = tapp.Description,
			Summary = tapp.Summary,
			IconHash = tapp.IconHash,
			RpcOrigins = tapp.RpcOrigins != null ? new ReadOnlyCollection<string>(tapp.RpcOrigins) : null,
			Flags = tapp.Flags,
			IsHook = tapp.IsHook,
			Type = tapp.Type,
			PrivacyPolicyUrl = tapp.PrivacyPolicyUrl,
			TermsOfServiceUrl = tapp.TermsOfServiceUrl,
			CustomInstallUrl = tapp.CustomInstallUrl,
			InstallParams = tapp.InstallParams,
			RoleConnectionsVerificationUrl = tapp.RoleConnectionsVerificationUrl.ValueOrDefault(),
			InteractionsEndpointUrl = tapp.InteractionsEndpointUrl.ValueOrDefault(),
			CoverImageHash = tapp.CoverImageHash.ValueOrDefault(),
			Tags = (tapp.Tags ?? Enumerable.Empty<string>()).ToArray()
		};

		if (tapp.Team == null)
		{
			app.Members = [..new[] { new DiscordUser(tapp.Owner) }];
			app.Team = null;
			app.TeamName = null;
			app.Owner = new(tapp.Owner);
		}
		else
		{
			app.Team = new(tapp.Team);

			var members = tapp.Team.Members
				.Select(x => new DiscordTeamMember(x)
				{
					TeamId = app.Team.Id,
					TeamName = app.Team.Name,
					User = new(x.User)
				})
				.ToArray();

			foreach (var member in members)
				if (member.User.Id == tapp.Team.OwnerId)
					member.Role = "owner";

			var users = members
				.Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
				.Select(x => x.User)
				.ToArray();

			app.Members = [..users];
			app.Team.Owner = members.First(x => x.Role == "owner").User;
			app.Team.Members = new List<DiscordTeamMember>(members);
			app.TeamName = app.Team.Name;
			app.Owner = new(tapp.Owner);
		}

		app.GuildId = tapp.GuildId.ValueOrDefault();
		app.Slug = tapp.Slug.ValueOrDefault();
		app.PrimarySkuId = tapp.PrimarySkuId.ValueOrDefault();
		app.VerifyKey = tapp.VerifyKey.ValueOrDefault();
		app.CoverImageHash = tapp.CoverImageHash.ValueOrDefault();
		app.Guild = tapp.Guild.ValueOrDefault();
		app.ApproximateGuildCount = tapp.ApproximateGuildCount.ValueOrDefault();
		app.RequiresCodeGrant = tapp.BotRequiresCodeGrant.ValueOrDefault();
		app.IsPublic = tapp.IsPublicBot.ValueOrDefault();
		app.RedirectUris = tapp.RedirectUris.ValueOrDefault();
		app.InteractionsEndpointUrl = tapp.InteractionsEndpointUrl.ValueOrDefault();

		return app;
	}

	/// <summary>
	/// Updates the current API application.
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
		Optional<DiscordApplicationInstallParams?> installParams
	)
	{
		var iconb64 = ImageTool.Base64FromStream(icon);
		var coverImageb64 = ImageTool.Base64FromStream(coverImage);
		if (tags != null && tags is { HasValue: true, Value: not null })
			if (tags.Value.Any(x => x.Length > 20))
				throw new InvalidOperationException("Tags can not exceed 20 chars.");

		_ = await this.ApiClient.ModifyCurrentApplicationInfoAsync(description, interactionsEndpointUrl, roleConnectionsVerificationUrl, customInstallUrl, tags, iconb64, coverImageb64, flags, installParams).ConfigureAwait(false);
		// We use GetCurrentApplicationAsync because modify returns internal data not meant for developers.
		var app = await this.GetCurrentApplicationAsync().ConfigureAwait(false);
		this.CurrentApplication = app;
		return app;
	}

	/// <summary>
	/// Gets a list of voice regions.
	/// </summary>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
		=> this.ApiClient.ListVoiceRegionsAsync();

	/// <summary>
	/// Initializes this client. This method fetches information about current user, application, and voice regions.
	/// </summary>
	public virtual async Task InitializeAsync()
	{
		if (this.CurrentUser == null)
		{
			this.CurrentUser = await this.ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
			this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
		}

		if (this.Configuration.TokenType == TokenType.Bot && this.CurrentApplication == null)
			this.CurrentApplication = await this.GetCurrentApplicationAsync().ConfigureAwait(false);

		if (this.Configuration.TokenType != TokenType.Bearer && this.InternalVoiceRegions.IsEmpty)
		{
			var vrs = await this.ListVoiceRegionsAsync().ConfigureAwait(false);
			foreach (var xvr in vrs)
				this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
		}

		if (this.Configuration is { EnableSentry: true, AttachUserInfo: true })
			SentrySdk.ConfigureScope(x => x.User = new()
			{
				Id = this.CurrentUser.Id.ToString(),
				Username = this.CurrentUser.UsernameWithDiscriminator,
				Other = new Dictionary<string, string>()
				{
					{ "developer", this.Configuration.DeveloperUserId?.ToString() ?? "not_given" },
					{ "email", this.Configuration.FeedbackEmail ?? "not_given" }
				}
			});
	}

	/// <summary>
	/// Gets the current gateway info for the provided token.
	/// <para>If no value is provided, the configuration value will be used instead.</para>
	/// </summary>
	/// <returns>A gateway info object.</returns>
	public async Task<GatewayInfo> GetGatewayInfoAsync(string? token = null)
	{
		if (this.Configuration.TokenType is not TokenType.Bot)
			throw new InvalidOperationException("Only bot tokens can access this info.");

		if (!string.IsNullOrEmpty(this.Configuration.Token))
			return await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);

		if (string.IsNullOrEmpty(token))
			throw new InvalidOperationException("Could not locate a valid token.");

		this.Configuration.Token = token;

		var res = await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
		this.Configuration.Token = null;
		return res;
	}

	/// <summary>
	/// Gets a cached user.
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
	/// Tries the get a cached user.
	/// </summary>
	/// <param name="userId">The user id.</param>
	/// <param name="user">The user.</param>
	internal bool TryGetCachedUserInternal(ulong userId, [MaybeNullWhen(false)] out DiscordUser user)
		=> this.UserCache.TryGetValue(userId, out user);

	/// <summary>
	/// Disposes this client.
	/// </summary>
	public abstract void Dispose();

	/// <summary>
	/// Generates a fingerprint for sentry.
	/// </summary>
	/// <param name="ev">The sentry event.</param>
	/// <param name="additional">The optional additional fingerprint value, if any.</param>
	internal static IEnumerable<string> GenerateSentryFingerPrint(SentryEvent ev, string? additional = null)
	{
		var fingerPrint = new List<string>
		{
			ev.Level.ToString(),
			ev.Logger
		};

		if (ev.Message?.Message is not null)
			fingerPrint.Add(ev.Message.Message);

		if (additional is not null)
			fingerPrint.Add(additional);

		var ex = ev.Exception;

		if (ex is null)
			return fingerPrint;

		fingerPrint.Add(ex.GetType().FullName);
		if (!string.IsNullOrEmpty(ex.Message))
			fingerPrint.Add(ex.Message);

		if (ex.TargetSite is not null)
			fingerPrint.Add(ex.TargetSite.ToString());

		if (ex.InnerException is null)
			return fingerPrint;

		fingerPrint.Add(ex.InnerException.GetType().FullName);
		if (!string.IsNullOrEmpty(ex.InnerException.Message))
			fingerPrint.Add(ex.InnerException.Message);

		return fingerPrint;
	}
}
