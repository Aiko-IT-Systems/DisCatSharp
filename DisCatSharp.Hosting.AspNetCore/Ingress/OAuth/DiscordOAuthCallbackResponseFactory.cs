using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.Net.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

internal sealed class DiscordOAuthCallbackResponseFactory : IDiscordOAuthCallbackResponseFactory
{
	private static readonly IReadOnlyDictionary<string, StringValues> s_noStoreHeaders =
		new ReadOnlyDictionary<string, StringValues>(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase)
		{
			["Cache-Control"] = "no-store, no-cache",
			["Pragma"] = "no-cache",
			["X-Content-Type-Options"] = "nosniff"
		});

	public DiscordIngressResponse CreateResponse(DiscordOAuthCallbackResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		return result.Status switch
		{
			DiscordOAuthCallbackStatus.Success => CreateJsonResponse(StatusCodes.Status200OK, new SuccessPayload(result)),
			DiscordOAuthCallbackStatus.InvalidRequest => CreateJsonResponse(StatusCodes.Status400BadRequest, new FailurePayload("invalid_request", result)),
			DiscordOAuthCallbackStatus.InvalidState => CreateJsonResponse(StatusCodes.Status400BadRequest, new FailurePayload("invalid_state", result)),
			DiscordOAuthCallbackStatus.SecurityFailure => CreateJsonResponse(StatusCodes.Status403Forbidden, new FailurePayload("security_failure", result)),
			DiscordOAuthCallbackStatus.ConfigurationFailure => CreateJsonResponse(StatusCodes.Status500InternalServerError, new FailurePayload("configuration_failure", result)),
			DiscordOAuthCallbackStatus.ExchangeFailure => CreateJsonResponse(StatusCodes.Status502BadGateway, new FailurePayload("exchange_failure", result)),
			_ => CreateJsonResponse(StatusCodes.Status500InternalServerError, new FailurePayload("failure", result))
		};
	}

	private static DiscordIngressResponse CreateJsonResponse(int statusCode, object payload)
		=> new(statusCode, DiscordIngressPayload.FromString(DiscordJson.SerializeObject(payload)), "application/json; charset=utf-8", s_noStoreHeaders);

	private static IReadOnlyDictionary<string, string?> FilterCallbackParameters(IReadOnlyDictionary<string, string?> queryParameters)
	{
		if (queryParameters.Count == 0)
			return queryParameters;

		Dictionary<string, string?> filteredParameters = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in queryParameters)
		{
			if (string.Equals(key, "code", StringComparison.OrdinalIgnoreCase))
				continue;

			filteredParameters[key] = value;
		}

		return new ReadOnlyDictionary<string, string?>(filteredParameters);
	}

	private sealed class SuccessPayload(DiscordOAuthCallbackResult result)
	{
		[JsonProperty("status")]
		public string Status { get; } = "success";

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string? State { get; } = result.State;

		[JsonProperty("flow", NullValueHandling = NullValueHandling.Ignore)]
		public string? Flow { get; } = result.PendingState?.Flow;

		[JsonProperty("callback_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? CallbackUri { get; } = result.CallbackUri?.AbsoluteUri;

		[JsonProperty("request_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestUri { get; } = result.PendingState?.RequestUri?.AbsoluteUri;

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RedirectUri { get; } = result.RedirectUri?.AbsoluteUri;

		[JsonProperty("requested_scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestedScope { get; } = result.RequestedScope;

		[JsonProperty("granted_scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? GrantedScope { get; } = result.GrantedScope;

		[JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? TokenType { get; } = result.AccessToken?.TokenType;

		[JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
		public int? ExpiresIn { get; } = result.AccessToken?.ExpiresIn;

		[JsonProperty("has_refresh_token")]
		public bool HasRefreshToken { get; } = !string.IsNullOrWhiteSpace(result.AccessToken?.RefreshToken);

		[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? IntegrationType { get; } = result.IntegrationType;

		[JsonProperty("incoming_webhook_available")]
		public bool IncomingWebhookAvailable { get; } = result.HasIncomingWebhookPayload;

		[JsonProperty("state_created_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StateCreatedAt { get; } = result.PendingState?.CreatedAt;

		[JsonProperty("state_expires_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StateExpiresAt { get; } = result.PendingState?.ExpiresAt;

		[JsonProperty("authorization_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> AuthorizationParameters { get; } = result.AuthorizationParameters;

		[JsonProperty("callback_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> CallbackParameters { get; } = FilterCallbackParameters(result.CallbackParameters);
	}

	private sealed class FailurePayload(string status, DiscordOAuthCallbackResult result)
	{
		[JsonProperty("status")]
		public string Status { get; } = status;

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string? State { get; } = result.State;

		[JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
		public string? Detail { get; } = result.Detail;

		[JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
		public string? OAuthError { get; } = result.OAuthError;

		[JsonProperty("error_description", NullValueHandling = NullValueHandling.Ignore)]
		public string? OAuthErrorDescription { get; } = result.OAuthErrorDescription;

		[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? IntegrationType { get; } = result.IntegrationType;

		[JsonProperty("callback_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? CallbackUri { get; } = result.CallbackUri?.AbsoluteUri;

		[JsonProperty("request_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestUri { get; } = result.PendingState?.RequestUri?.AbsoluteUri;

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RedirectUri { get; } = result.RedirectUri?.AbsoluteUri;

		[JsonProperty("callback_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> CallbackParameters { get; } = FilterCallbackParameters(result.CallbackParameters);
	}
}
