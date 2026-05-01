using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.Net.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordOAuthCallbackResponseFactory : IDiscordOAuthCallbackResponseFactory
{
	private static readonly IReadOnlyDictionary<string, StringValues> NoStoreHeaders =
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
		=> new(statusCode, DiscordIngressPayload.FromString(DiscordJson.SerializeObject(payload)), "application/json; charset=utf-8", NoStoreHeaders);

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

	private sealed class SuccessPayload
	{
		public SuccessPayload(DiscordOAuthCallbackResult result)
		{
			this.State = result.State;
			this.Flow = result.PendingState?.Flow;
			this.CallbackUri = result.CallbackUri?.AbsoluteUri;
			this.RequestUri = result.PendingState?.RequestUri?.AbsoluteUri;
			this.RedirectUri = result.RedirectUri?.AbsoluteUri;
			this.RequestedScope = result.RequestedScope;
			this.GrantedScope = result.GrantedScope;
			this.TokenType = result.AccessToken?.TokenType;
			this.ExpiresIn = result.AccessToken?.ExpiresIn;
			this.HasRefreshToken = !string.IsNullOrWhiteSpace(result.AccessToken?.RefreshToken);
			this.IntegrationType = result.IntegrationType;
			this.IncomingWebhookAvailable = result.HasIncomingWebhookPayload;
			this.StateCreatedAt = result.PendingState?.CreatedAt;
			this.StateExpiresAt = result.PendingState?.ExpiresAt;
			this.AuthorizationParameters = result.AuthorizationParameters;
			this.CallbackParameters = FilterCallbackParameters(result.CallbackParameters);
		}

		[JsonProperty("status")]
		public string Status { get; } = "success";

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string? State { get; }

		[JsonProperty("flow", NullValueHandling = NullValueHandling.Ignore)]
		public string? Flow { get; }

		[JsonProperty("callback_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? CallbackUri { get; }

		[JsonProperty("request_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestUri { get; }

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RedirectUri { get; }

		[JsonProperty("requested_scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestedScope { get; }

		[JsonProperty("granted_scope", NullValueHandling = NullValueHandling.Ignore)]
		public string? GrantedScope { get; }

		[JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? TokenType { get; }

		[JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
		public int? ExpiresIn { get; }

		[JsonProperty("has_refresh_token")]
		public bool HasRefreshToken { get; }

		[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? IntegrationType { get; }

		[JsonProperty("incoming_webhook_available")]
		public bool IncomingWebhookAvailable { get; }

		[JsonProperty("state_created_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StateCreatedAt { get; }

		[JsonProperty("state_expires_at", NullValueHandling = NullValueHandling.Ignore)]
		public DateTimeOffset? StateExpiresAt { get; }

		[JsonProperty("authorization_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> AuthorizationParameters { get; }

		[JsonProperty("callback_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> CallbackParameters { get; }
	}

	private sealed class FailurePayload
	{
		public FailurePayload(string status, DiscordOAuthCallbackResult result)
		{
			this.Status = status;
			this.State = result.State;
			this.Detail = result.Detail;
			this.OAuthError = result.OAuthError;
			this.OAuthErrorDescription = result.OAuthErrorDescription;
			this.IntegrationType = result.IntegrationType;
			this.CallbackUri = result.CallbackUri?.AbsoluteUri;
			this.RequestUri = result.PendingState?.RequestUri?.AbsoluteUri;
			this.RedirectUri = result.RedirectUri?.AbsoluteUri;
			this.CallbackParameters = FilterCallbackParameters(result.CallbackParameters);
		}

		[JsonProperty("status")]
		public string Status { get; }

		[JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
		public string? State { get; }

		[JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
		public string? Detail { get; }

		[JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
		public string? OAuthError { get; }

		[JsonProperty("error_description", NullValueHandling = NullValueHandling.Ignore)]
		public string? OAuthErrorDescription { get; }

		[JsonProperty("integration_type", NullValueHandling = NullValueHandling.Ignore)]
		public string? IntegrationType { get; }

		[JsonProperty("callback_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? CallbackUri { get; }

		[JsonProperty("request_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RequestUri { get; }

		[JsonProperty("redirect_uri", NullValueHandling = NullValueHandling.Ignore)]
		public string? RedirectUri { get; }

		[JsonProperty("callback_parameters", NullValueHandling = NullValueHandling.Ignore)]
		public IReadOnlyDictionary<string, string?> CallbackParameters { get; }
	}
}
