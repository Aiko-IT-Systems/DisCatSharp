using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.Entities.OAuth2;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

/// <summary>
///     Represents the outcome of processing a Discord OAuth authorization-code callback.
/// </summary>
public sealed class DiscordOAuthCallbackResult
{
	private static readonly IReadOnlyDictionary<string, string?> EmptyStringProperties =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));

	private static readonly IReadOnlyDictionary<string, object?> EmptyObjectProperties =
		new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal));

	private DiscordOAuthCallbackResult(
		DiscordOAuthCallbackStatus status,
		string? state,
		string? code,
		string? detail,
		DiscordIngressPendingState? pendingState,
		Uri? redirectUri,
		Uri? callbackUri,
		DiscordAccessToken? accessToken,
		string? oauthError,
		string? oauthErrorDescription,
		Exception? exception,
		IReadOnlyDictionary<string, string?>? authorizationParameters,
		IReadOnlyDictionary<string, string?>? callbackParameters,
		IReadOnlyDictionary<string, object?>? tokenAdditionalProperties
	)
	{
		this.Status = status;
		this.State = state;
		this.Code = code;
		this.Detail = detail;
		this.PendingState = pendingState;
		this.RedirectUri = redirectUri;
		this.CallbackUri = callbackUri;
		this.AccessToken = accessToken;
		this.OAuthError = oauthError;
		this.OAuthErrorDescription = oauthErrorDescription;
		this.Exception = exception;
		this.AuthorizationParameters = CreateStringProperties(authorizationParameters);
		this.CallbackParameters = CreateStringProperties(callbackParameters);
		this.TokenAdditionalProperties = CreateObjectProperties(tokenAdditionalProperties);
	}

	/// <summary>
	///     Gets the callback processing status.
	/// </summary>
	public DiscordOAuthCallbackStatus Status { get; }

	/// <summary>
	///     Gets a value indicating whether the callback completed successfully.
	/// </summary>
	public bool IsSuccess => this.Status == DiscordOAuthCallbackStatus.Success;

	/// <summary>
	///     Gets the callback state received from Discord.
	/// </summary>
	public string? State { get; }

	/// <summary>
	///     Gets the authorization code received from Discord.
	/// </summary>
	public string? Code { get; }

	/// <summary>
	///     Gets the human-readable callback detail for non-success outcomes.
	/// </summary>
	public string? Detail { get; }

	/// <summary>
	///     Gets the consumed pending state entry, when one was available.
	/// </summary>
	public DiscordIngressPendingState? PendingState { get; }

	/// <summary>
	///     Gets the redirect URI used for the OAuth code exchange.
	/// </summary>
	public Uri? RedirectUri { get; }

	/// <summary>
	///     Gets the callback URI that was received by ASP.NET Core.
	/// </summary>
	public Uri? CallbackUri { get; }

	/// <summary>
	///     Gets the exchanged access token when the callback completed successfully.
	/// </summary>
	public DiscordAccessToken? AccessToken { get; }

	/// <summary>
	///     Gets the OAuth error returned by Discord, when available.
	/// </summary>
	public string? OAuthError { get; }

	/// <summary>
	///     Gets the OAuth error description returned by Discord, when available.
	/// </summary>
	public string? OAuthErrorDescription { get; }

	/// <summary>
	///     Gets the underlying exception, when one was captured.
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	///     Gets the normalized authorization request parameters derived from the stored pending request URI.
	/// </summary>
	public IReadOnlyDictionary<string, string?> AuthorizationParameters { get; }

	/// <summary>
	///     Gets the normalized callback query parameters received by ASP.NET Core.
	/// </summary>
	public IReadOnlyDictionary<string, string?> CallbackParameters { get; }

	/// <summary>
	///     Gets any additional token response properties preserved from the Discord OAuth token payload.
	/// </summary>
	public IReadOnlyDictionary<string, object?> TokenAdditionalProperties { get; }

	/// <summary>
	///     Gets the originally requested OAuth scope, when it was captured.
	/// </summary>
	public string? RequestedScope => GetProperty(this.AuthorizationParameters, "scope");

	/// <summary>
	///     Gets the scope granted by the exchanged Discord access token, when available.
	/// </summary>
	public string? GrantedScope => this.AccessToken?.Scope;

	/// <summary>
	///     Gets the Discord integration type associated with the callback, when available.
	/// </summary>
	public string? IntegrationType => GetProperty(this.AuthorizationParameters, "integration_type") ?? GetProperty(this.CallbackParameters, "integration_type");

	/// <summary>
	///     Gets a value indicating whether the token response preserved an incoming webhook payload.
	/// </summary>
	public bool HasIncomingWebhookPayload => this.TokenAdditionalProperties.ContainsKey("webhook");

	/// <summary>
	///     Gets the preserved incoming webhook payload from the token response, when available.
	/// </summary>
	public object? IncomingWebhookPayload => this.TokenAdditionalProperties.TryGetValue("webhook", out var payload) ? payload : null;

	/// <summary>
	///     Creates a successful callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult Success(
		string state,
		string code,
		DiscordIngressPendingState pendingState,
		Uri? redirectUri,
		Uri? callbackUri,
		DiscordAccessToken accessToken,
		IReadOnlyDictionary<string, string?>? authorizationParameters = null,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		IReadOnlyDictionary<string, object?>? tokenAdditionalProperties = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(state);
		ArgumentException.ThrowIfNullOrWhiteSpace(code);
		ArgumentNullException.ThrowIfNull(pendingState);
		ArgumentNullException.ThrowIfNull(accessToken);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.Success,
			state,
			code,
			null,
			pendingState,
			redirectUri,
			callbackUri,
			accessToken,
			null,
			null,
			null,
			authorizationParameters,
			callbackParameters,
			tokenAdditionalProperties);
	}

	/// <summary>
	///     Creates an invalid-request callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult InvalidRequest(
		string? state,
		string? code,
		string detail,
		string? oauthError = null,
		string? oauthErrorDescription = null,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		Uri? callbackUri = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(detail);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.InvalidRequest,
			state,
			code,
			detail,
			null,
			null,
			callbackUri,
			null,
			oauthError,
			oauthErrorDescription,
			null,
			null,
			callbackParameters,
			null);
	}

	/// <summary>
	///     Creates an invalid-state callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult InvalidState(
		string? state,
		string? code,
		string detail,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		Uri? callbackUri = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(detail);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.InvalidState,
			state,
			code,
			detail,
			null,
			null,
			callbackUri,
			null,
			null,
			null,
			null,
			null,
			callbackParameters,
			null);
	}

	/// <summary>
	///     Creates a security-failure callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult SecurityFailure(
		string? state,
		string? code,
		DiscordIngressPendingState? pendingState,
		Uri? redirectUri,
		Uri? callbackUri,
		string detail,
		string? oauthError = null,
		string? oauthErrorDescription = null,
		IReadOnlyDictionary<string, string?>? authorizationParameters = null,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		Exception? exception = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(detail);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.SecurityFailure,
			state,
			code,
			detail,
			pendingState,
			redirectUri,
			callbackUri,
			null,
			oauthError,
			oauthErrorDescription,
			exception,
			authorizationParameters,
			callbackParameters,
			null);
	}

	/// <summary>
	///     Creates a configuration-failure callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult ConfigurationFailure(
		string? state,
		string? code,
		string detail,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		Uri? callbackUri = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(detail);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.ConfigurationFailure,
			state,
			code,
			detail,
			null,
			null,
			callbackUri,
			null,
			null,
			null,
			null,
			null,
			callbackParameters,
			null);
	}

	/// <summary>
	///     Creates a code-exchange failure callback result.
	/// </summary>
	public static DiscordOAuthCallbackResult ExchangeFailure(
		string? state,
		string? code,
		DiscordIngressPendingState? pendingState,
		Uri? redirectUri,
		Uri? callbackUri,
		string detail,
		IReadOnlyDictionary<string, string?>? authorizationParameters = null,
		IReadOnlyDictionary<string, string?>? callbackParameters = null,
		Exception? exception = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(detail);

		return new DiscordOAuthCallbackResult(
			DiscordOAuthCallbackStatus.ExchangeFailure,
			state,
			code,
			detail,
			pendingState,
			redirectUri,
			callbackUri,
			null,
			null,
			null,
			exception,
			authorizationParameters,
			callbackParameters,
			null);
	}

	private static IReadOnlyDictionary<string, string?> CreateStringProperties(IReadOnlyDictionary<string, string?>? properties)
	{
		if (properties is null || properties.Count == 0)
			return EmptyStringProperties;

		Dictionary<string, string?> normalizedProperties = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in properties)
			normalizedProperties[key] = value;

		return new ReadOnlyDictionary<string, string?>(normalizedProperties);
	}

	private static IReadOnlyDictionary<string, object?> CreateObjectProperties(IReadOnlyDictionary<string, object?>? properties)
	{
		if (properties is null || properties.Count == 0)
			return EmptyObjectProperties;

		Dictionary<string, object?> normalizedProperties = new(StringComparer.Ordinal);
		foreach (var (key, value) in properties)
			normalizedProperties[key] = value;

		return new ReadOnlyDictionary<string, object?>(normalizedProperties);
	}

	private static string? GetProperty(IReadOnlyDictionary<string, string?> properties, string key)
		=> properties.TryGetValue(key, out var value) ? value : null;
}
