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

namespace DisCatSharp;

/// <summary>
/// Represents a common base for various Discord Client implementations.
/// </summary>
public abstract class BaseDiscordClient : IDisposable
{
	/// <summary>
	/// Gets the api client.
	/// </summary>
	internal protected DiscordApiClient ApiClient { get; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	internal protected DiscordConfiguration Configuration { get; }

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
	public string BotLibrary { get; }

	[Obsolete("Use GetLibraryDeveloperTeamAsync")]
	public DisCatSharpTeam LibraryDeveloperTeamAsync
		=> this.GetLibraryDevelopmentTeamAsync().Result;

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
	internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> VoiceRegionsLazy;

	/// <summary>
	/// Initializes this Discord API client.
	/// </summary>
	/// <param name="config">Configuration for this client.</param>
	protected BaseDiscordClient(DiscordConfiguration config)
	{
		this.Configuration = new DiscordConfiguration(config);
		this.ServiceProvider = config.ServiceProvider;

		if (this.ServiceProvider != null)
		{
			this.Configuration.LoggerFactory ??= config.ServiceProvider.GetService<ILoggerFactory>();
			this.Logger = config.ServiceProvider.GetService<ILogger<BaseDiscordClient>>();
		}

		if (this.Configuration.LoggerFactory == null)
		{
			this.Configuration.LoggerFactory = new DefaultLoggerFactory();
			this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
			if (this.Configuration.EnableSentry)
				this.Configuration.LoggerFactory.AddSentry(x => x.DiagnosticLevel = Sentry.SentryLevel.Error);
		}
		this.Logger ??= this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();

		this.ApiClient = new DiscordApiClient(this);
		this.UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
		this.InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
		this.VoiceRegionsLazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));

		this.RestClient = new();
		this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Utilities.GetUserAgent());
		this.RestClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Discord-Locale", this.Configuration.Locale);

		var a = typeof(DiscordClient).GetTypeInfo().Assembly;

		var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (iv != null)
		{
			this.VersionString = iv.InformationalVersion;
		}
		else
		{
			var v = a.GetName().Version;
			var vs = v.ToString(3);

			if (v.Revision > 0)
				this.VersionString = $"{vs}, CI build {v.Revision}";
		}

		this.BotLibrary = "DisCatSharp";
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
			RoleConnectionsVerificationUrl = tapp.RoleConnectionsVerificationUrl,
			Tags = (tapp.Tags ?? Enumerable.Empty<string>()).ToArray()
		};

		if (tapp.Team == null)
		{
			app.Owners = new List<DiscordUser>(new[] { new DiscordUser(tapp.Owner) });
			app.Team = null;
			app.TeamName = null;
		}
		else
		{
			app.Team = new DiscordTeam(tapp.Team);

			var members = tapp.Team.Members
				.Select(x => new DiscordTeamMember(x) { TeamId = app.Team.Id, TeamName = app.Team.Name, User = new DiscordUser(x.User) })
				.ToArray();

			var owners = members
				.Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
				.Select(x => x.User)
				.ToArray();

			app.Owners = new List<DiscordUser>(owners);
			app.Team.Owner = owners.FirstOrDefault(x => x.Id == tapp.Team.OwnerId);
			app.Team.Members = new List<DiscordTeamMember>(members);
			app.TeamName = app.Team.Name;
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
	/// <param name="tags">The new tags.</param>
	/// <param name="icon">The new application icon.</param>
	/// <returns>The updated application.</returns>
	public async Task<DiscordApplication> UpdateCurrentApplicationInfoAsync(Optional<string> description, Optional<string> interactionsEndpointUrl, Optional<string> roleConnectionsVerificationUrl, Optional<List<string>?> tags, Optional<Stream> icon)
	{
		var iconb64 = ImageTool.Base64FromStream(icon);
		if (tags != null && tags.HasValue && tags.Value != null)
			if (tags.Value.Any(x => x.Length > 20))
				throw new InvalidOperationException("Tags can not exceed 20 chars.");
		_ = await this.ApiClient.ModifyCurrentApplicationInfoAsync(description, interactionsEndpointUrl, roleConnectionsVerificationUrl, tags, iconb64);
		// We use GetCurrentApplicationAsync because modify returns internal data not meant for developers.
		var app = await this.GetCurrentApplicationAsync();
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
	}

	/// <summary>
	/// Gets the current gateway info for the provided token.
	/// <para>If no value is provided, the configuration value will be used instead.</para>
	/// </summary>
	/// <returns>A gateway info object.</returns>
	public async Task<GatewayInfo> GetGatewayInfoAsync(string token = null)
	{
		if (this.Configuration.TokenType != TokenType.Bot)
			throw new InvalidOperationException("Only bot tokens can access this info.");

		if (string.IsNullOrEmpty(this.Configuration.Token))
		{
			if (string.IsNullOrEmpty(token))
				throw new InvalidOperationException("Could not locate a valid token.");

			this.Configuration.Token = token;

			var res = await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
			this.Configuration.Token = null;
			return res;
		}

		return await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Gets some information about the development team behind DisCatSharp.
	/// Can be used for crediting etc.
	/// <para>Note: This call contacts servers managed by the DCS team, no information is collected.</para>
	/// <returns>The team, or null with errors being logged on failure.</returns>
	/// </summary>
	[Obsolete("Don't use this right now, inactive")]
	public async Task<DisCatSharpTeam> GetLibraryDevelopmentTeamAsync()
		=> await DisCatSharpTeam.Get(this.RestClient, this.Logger, this.ApiClient).ConfigureAwait(false);

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

		user = new DiscordUser { Id = userId, Discord = this };
		return false;
	}

	/// <summary>
	/// Disposes this client.
	/// </summary>
	public abstract void Dispose();
}
