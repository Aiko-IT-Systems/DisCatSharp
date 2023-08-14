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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Entities.OAuth2;
using DisCatSharp.Enums;
using DisCatSharp.Net;

using Microsoft.Extensions.Logging;

namespace DisCatSharp;

/// <summary>
/// Represents a <see cref="DiscordOAuth2Client"/>.
/// </summary>
public class DiscordOAuth2Client
{
	/// <summary>
	/// Gets the logger for this client.
	/// </summary>
	public ILogger<DiscordOAuth2Client> Logger { get; }

	/// <summary>
	/// Gets the api client.
	/// </summary>
	internal readonly DiscordApiClient ApiClient;

	/// <summary>
	/// Gets the minimal log level.
	/// </summary>
	internal readonly LogLevel MinimumLogLevel;

	/// <summary>
	/// Gets the log timestamp format.
	/// </summary>
	internal readonly string LogTimestampFormat;

	/// <summary>
	/// Gets the string representing the version header of the bot lib.
	/// </summary>
	public readonly string VersionHeader;

	/// <summary>
	/// Gets the bot library name.
	/// </summary>
	public string BotLibrary
		=> "DisCatSharp";

	/// <summary>
	/// Gets the client id.
	/// </summary>
	public readonly ulong ClientId;

	/// <summary>
	/// Gets the client secret.
	/// </summary>
	public readonly string ClientSecret;

	/// <summary>
	/// Gets the redirect uri.
	/// </summary>
	public readonly Uri RedirectUri;

	/// <summary>
	/// Creates a new oauth client.
	/// </summary>
	/// <param name="clientId">The client id.</param>
	/// <param name="clientSecret">The client secret.</param>
	/// <param name="redirectUri">The redirect uri.</param>
	/// <param name="proxy">The proxy to use for HTTP connections. Defaults to null.</param>
	/// <param name="timeout">The optional timeout to use for HTTP requests. Set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable timeouts. Defaults to null.</param>
	/// <param name="useRelativeRateLimit">Whether to use the system clock for computing rate limit resets. See <see cref="DiscordConfiguration.UseRelativeRatelimit"/> for more details. Defaults to true.</param>
	/// <param name="loggerFactory">The optional logging factory to use for this client. Defaults to null.</param>
	/// <param name="minimumLogLevel">The minimum logging level for messages. Defaults to information.</param>
	/// <param name="logTimestampFormat">The timestamp format to use for the logger.</param>
	public DiscordOAuth2Client(ulong clientId, string clientSecret, string redirectUri, IWebProxy proxy = null!, TimeSpan? timeout = null, bool useRelativeRateLimit = true,
		ILoggerFactory loggerFactory = null!, LogLevel minimumLogLevel = LogLevel.Information, string logTimestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
	{
		this.MinimumLogLevel = minimumLogLevel;
		this.LogTimestampFormat = logTimestampFormat;

		if (loggerFactory == null!)
		{
			loggerFactory = new DefaultLoggerFactory();
			loggerFactory.AddProvider(new DefaultLoggerProvider(this));
		}

		this.Logger = loggerFactory.CreateLogger<DiscordOAuth2Client>();
		this.ClientId = clientId;
		this.ClientSecret = clientSecret;
		this.RedirectUri = new(redirectUri);

		var parsedTimeout = timeout ?? TimeSpan.FromSeconds(10);

		this.ApiClient = new(this, proxy!, parsedTimeout, useRelativeRateLimit, this.Logger);


		this.ApiClient.Rest.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("client_id", this.ClientId.ToString());
		this.ApiClient.Rest.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", this.ClientSecret);

		var a = typeof(DiscordOAuth2Client).GetTypeInfo().Assembly;
		var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

		string vs;
		if (iv != null)
			vs = iv.InformationalVersion;
		else
		{
			var v = a.GetName().Version;
			vs = v?.ToString(3);
		}

		this.VersionHeader = $"DiscordBot (https://github.com/Aiko-IT-Systems/DisCatSharp, v{vs})";
		this.ApiClient.Rest.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", this.VersionHeader);
	}


	/// <summary>
	/// Gets the OAuth2 url.
	/// </summary>
	/// <param name="scopes">The space seperated scopes to request.</param>
	/// <param name="state">The state to use for security reasons. Use <see cref="GenerateState"/>.</param>.
	/// <param name="suppressPrompt">Whether to suppress the prompt. Works only if previously authorized with same scopes.</param>
	/// <returns>The OAuth2 url</returns>
	public Uri GetOAuth2Url(string scopes, string state, bool suppressPrompt = false) =>
		new(new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", this.ClientId.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", scopes)
			.AddParameter("state", state)
			.AddParameter("redirect_uri", this.RedirectUri.AbsoluteUri)
			.AddParameter("response_type", "code")
			.AddParameter("prompt", suppressPrompt ? "none" : "consent")
			.ToString());

	/// <summary>
	/// Generates a state for oauth2 authorization.
	/// </summary>
	/// <returns></returns>
	public string GenerateState()
		=> $"{DateTimeOffset.UtcNow.UtcTicks}::{this.ClientId.GetHashCode()}::{Guid.NewGuid()}";

	/// <summary>
	/// Exchanges a code for an access token.
	/// </summary>
	/// <param name="code">The exchange code.</param>
	public Task<DiscordAccessToken> ExchangeAccessTokenAsync(string code)
		=> this.ApiClient.ExchangeOAuth2AccessTokenAsync(code);

	/// <summary>
	/// Exchanges a refresh token for a new access token.
	/// </summary>
	/// <param name="accessToken">The current access token.</param>
	public Task<DiscordAccessToken> RefreshAccessTokenAsync(DiscordAccessToken accessToken)
		=> this.ApiClient.RefreshOAuth2AccessTokenAsync(accessToken.RefreshToken);

	/// <summary>
	/// Gets the current authorization information.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task<DiscordAuthorizationInformation> GetCurrentAuthorizationInformationAsync(DiscordAccessToken accessToken)
		=> this.ApiClient.GetCurrentOAuth2AuthorizationInformationAsync(accessToken.AccessToken);

	/// <summary>
	/// Gets the current user.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task<IReadOnlyList<DiscordUser>> GetCurrentUserAsync(DiscordAccessToken accessToken)
		=> throw new NotImplementedException();

	/// <summary>
	/// Gets the current user's connections.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task<IReadOnlyList<DiscordConnection>> GetCurrentUserConnectionsAsync(DiscordAccessToken accessToken)
		=> throw new NotImplementedException();

	/// <summary>
	/// Gets the current user's guilds.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task<IReadOnlyDictionary<ulong, DiscordGuild>> GetCurrentUserGuildsAsync(DiscordAccessToken accessToken)
		=> throw new NotImplementedException();

	/// <summary>
	/// Gets the current user's guild member for given <paramref name="guildId"/>.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	/// <param name="guildId">The guild id to get the member for.</param>
	public Task<DiscordMember> GetCurrentUserGuildMemberAsync(DiscordAccessToken accessToken, ulong guildId)
		=> throw new NotImplementedException();

	/// <summary>
	/// Gets the current user's application role connection.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task<DiscordApplicationRoleConnection> GetCurrentUserApplicationRoleConnectionAsync(DiscordAccessToken accessToken)
		=> throw new NotImplementedException();

	/// <summary>
	/// Updates the current user's application role connection.
	/// </summary>
	/// <param name="accessToken">The access token.</param>
	public Task UpdateCurrentUserApplicationRoleConnectionAsync(DiscordAccessToken accessToken, string? platformName, string? platformUsername, ApplicationRoleConnectionMetadata metadata)
		=> throw new NotImplementedException();
}
