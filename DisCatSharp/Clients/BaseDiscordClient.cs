// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
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
	// ReSharper disable once MemberCanBeProtected.Global
	// ReSharper disable once MemberCanBeMadeStatic.Global
	public string BotLibrary
		=> "DisCatSharp";

	/// <summary>
	/// Gets the current user.
	/// </summary>
	public DiscordUser? CurrentUser { get; internal set; }

	/// <summary>
	/// Gets the current application.
	/// </summary>
	public DiscordApplication? CurrentApplication { get; internal set; }

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
	/// Gets the dictionary of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
	/// </summary>
	protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }

	/// <summary>
	/// Gets the lazy dictionary of available voice regions.
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
			this.Configuration.LoggerFactory ??= config.ServiceProvider.GetService<ILoggerFactory>();
			this.Logger = config.ServiceProvider.GetService<ILogger<BaseDiscordClient>>()!;
		}

		switch (this.Configuration.LoggerFactory)
		{
			case null when !this.Configuration.EnableSentry:
				this.Configuration.LoggerFactory = new DefaultLoggerFactory();
				this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
				break;
			case null when this.Configuration.EnableSentry:
			{
				var configureNamedOptions = new ConfigureNamedOptions<ConsoleLoggerOptions>(string.Empty, x =>
				{
					x.Format = ConsoleLoggerFormat.Default;
					x.TimestampFormat = this.Configuration.LogTimestampFormat;
					x.LogToStandardErrorThreshold = this.Configuration.MinimumLogLevel;
				});
				var optionsFactory = new OptionsFactory<ConsoleLoggerOptions>(new[] { configureNamedOptions }, Enumerable.Empty<IPostConfigureOptions<ConsoleLoggerOptions>>());
				var optionsMonitor = new OptionsMonitor<ConsoleLoggerOptions>(optionsFactory, Enumerable.Empty<IOptionsChangeTokenSource<ConsoleLoggerOptions>>(), new OptionsCache<ConsoleLoggerOptions>());

				var l = new ConsoleLoggerProvider(optionsMonitor);
				this.Configuration.LoggerFactory = new LoggerFactory();
				this.Configuration.LoggerFactory.AddProvider(l);
				break;
			}
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
			if (this.Configuration.LoggerFactory != null && this.Configuration.EnableSentry)
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
					o.BeforeSend = e =>
					{
						if (!this.Configuration.DisableExceptionFilter)
						{
							if (e.Exception != null)
							{
								if (!this.Configuration.TrackExceptions?.Contains(e.Exception.GetType()) ?? true)
									return null;
							}
							else if (e.Extra.Count == 0 || !e.Extra.ContainsKey("Found Fields"))
								return null;
						}

						if (e.HasUser())
							return e;

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
					};
				});

		if (this.Configuration.EnableSentry)
			this.Sentry = new(new()
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
				Debug = this.Configuration.SentryDebug,
				BeforeSend = e =>
				{
					if (!this.Configuration.DisableExceptionFilter)
					{
						if (e.Exception != null)
						{
							if (!this.Configuration.TrackExceptions?.Contains(e.Exception.GetType()) ?? true)
								return null;
						}
						else if (e.Extra.Count == 0 || !e.Extra.ContainsKey("Found Fields"))
							return null;
					}

					if (e.HasUser())
						return e;

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
				}
			});

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
		var transportApplication = await this.ApiClient.GetCurrentApplicationOauth2InfoAsync().ConfigureAwait(false);
		var app = new DiscordApplication
		{
			Discord = this,
			Id = transportApplication.Id,
			Name = transportApplication.Name,
			Description = transportApplication.Description,
			Summary = transportApplication.Summary,
			IconHash = transportApplication.IconHash,
			RpcOrigins = transportApplication.RpcOrigins != null ? new ReadOnlyCollection<string>(transportApplication.RpcOrigins) : null,
			Flags = transportApplication.Flags,
			IsHook = transportApplication.IsHook,
			Type = transportApplication.Type,
			PrivacyPolicyUrl = transportApplication.PrivacyPolicyUrl,
			TermsOfServiceUrl = transportApplication.TermsOfServiceUrl,
			CustomInstallUrl = transportApplication.CustomInstallUrl,
			InstallParams = transportApplication.InstallParams,
			RoleConnectionsVerificationUrl = transportApplication.RoleConnectionsVerificationUrl.ValueOrDefault(),
			InteractionsEndpointUrl = transportApplication.InteractionsEndpointUrl.ValueOrDefault(),
			CoverImageHash = transportApplication.CoverImageHash.ValueOrDefault(),
			Tags = (transportApplication.Tags ?? Enumerable.Empty<string>()).ToArray()
		};

		if (transportApplication.Team == null)
		{
			app.Members = new(new[] { new DiscordUser(transportApplication.Owner) });
			app.Team = null;
			app.TeamName = null;
			app.Owner = new(transportApplication.Owner);
		}
		else
		{
			app.Team = new(transportApplication.Team);

			var members = transportApplication.Team.Members
				.Select(x => new DiscordTeamMember(x) { TeamId = app.Team.Id, TeamName = app.Team.Name, User = new(x.User) })
				.ToArray();

			foreach (var member in members)
				if (member.User.Id == transportApplication.Team.OwnerId)
					member.Role = "owner";

			var users = members
				.Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
				.Select(x => x.User)
				.ToArray();

			app.Members = new(users);
			app.Team.Owner = members.First(x => x.Role == "owner").User;
			app.Team.Members = new List<DiscordTeamMember>(members);
			app.TeamName = app.Team.Name;
			app.Owner = new(transportApplication.Owner);
		}

		app.GuildId = transportApplication.GuildId.ValueOrDefault();
		app.Slug = transportApplication.Slug.ValueOrDefault();
		app.PrimarySkuId = transportApplication.PrimarySkuId.ValueOrDefault();
		app.VerifyKey = transportApplication.VerifyKey.ValueOrDefault();
		app.CoverImageHash = transportApplication.CoverImageHash.ValueOrDefault();
		app.Guild = transportApplication.Guild.ValueOrDefault();
		app.ApproximateGuildCount = transportApplication.ApproximateGuildCount.ValueOrDefault();
		app.RequiresCodeGrant = transportApplication.BotRequiresCodeGrant.ValueOrDefault();
		app.IsPublic = transportApplication.IsPublicBot.ValueOrDefault();
		app.RedirectUris = transportApplication.RedirectUris.ValueOrDefault();
		app.InteractionsEndpointUrl = transportApplication.InteractionsEndpointUrl.ValueOrDefault();

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
		Optional<string?> interactionsEndpointUrl, Optional<string?> roleConnectionsVerificationUrl, Optional<string?> customInstallUrl,
		Optional<List<string>?> tags, Optional<Stream?> icon, Optional<Stream?> coverImage,
		Optional<ApplicationFlags> flags, Optional<DiscordApplicationInstallParams?> installParams)
	{
		var iconBase64 = ImageTool.Base64FromStream(icon);
		var coverImageBase64 = ImageTool.Base64FromStream(coverImage);
		if (tags != null && tags is { HasValue: true, Value: not null })
			if (tags.Value.Any(x => x.Length > 20))
				throw new InvalidOperationException("Tags can not exceed 20 chars.");

		_ = await this.ApiClient.ModifyCurrentApplicationInfoAsync(description, interactionsEndpointUrl, roleConnectionsVerificationUrl, customInstallUrl, tags, iconBase64, coverImageBase64, flags, installParams).ConfigureAwait(false);
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

		if (this.Configuration.EnableSentry && this.Configuration.AttachUserInfo)
			SentrySdk.ConfigureScope(x => x.User = new()
			{
				Id = this.CurrentUser?.Id.ToString(),
				Username = this.CurrentUser?.UsernameWithDiscriminator,
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
		if (this.Configuration.TokenType != TokenType.Bot)
			throw new InvalidOperationException("Only bot tokens can access this info.");

		if (!string.IsNullOrEmpty(this.Configuration.Token))
			return await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);

		if (string.IsNullOrEmpty(token))
			throw new InvalidOperationException("Could not locate a valid token.");

		this.Configuration.Token = token;

		var res = await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
		return res;
	}

	/// <summary>
	/// Gets some information about the development team behind DisCatSharp.
	/// Can be used for crediting etc.
	/// <para>Note: This call contacts servers managed by the DCS team, no information is collected.</para>
	/// <returns>The team, or null with errors being logged on failure.</returns>
	/// </summary>
	[Deprecated("Don't use this right now, inactive")]
	public Task<DisCatSharpTeam> GetLibraryDevelopmentTeamAsync()
		=> DisCatSharpTeam.Get(this.RestClient, this.Logger, this.ApiClient);

	/// <summary>
	/// Gets a cached user.
	/// </summary>
	/// <param name="userId">The user id.</param>
	internal DiscordUser GetCachedOrEmptyUserInternal(ulong userId)
	{
		this.TryGetCachedUserInternal(userId, out var user);
		return user;
	}

	/// <summary>
	/// Tries the get a cached user.
	/// </summary>
	/// <param name="userId">The user id.</param>
	/// <param name="user">The user.</param>
	internal bool TryGetCachedUserInternal(ulong userId, out DiscordUser user)
	{
		if (this.UserCache.TryGetValue(userId, out user))
			return true;

		user = new() { Id = userId, Discord = this };
		return false;
	}

	/// <summary>
	/// Disposes this client.
	/// </summary>
	public abstract void Dispose();
}
