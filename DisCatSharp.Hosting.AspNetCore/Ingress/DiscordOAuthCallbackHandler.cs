using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace DisCatSharp.Hosting.AspNetCore.Ingress;

internal sealed class DiscordOAuthCallbackHandler : IDiscordOAuthCallbackHandler
{
	private static readonly IReadOnlyDictionary<string, string?> EmptyStringProperties =
		new ReadOnlyDictionary<string, string?>(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase));

	private static readonly IReadOnlyDictionary<string, object?> EmptyObjectProperties =
		new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal));

	private readonly IOptions<DiscordOAuthIngressOptions> _options;
	private readonly IDiscordIngressPendingStateStore _pendingStateStore;
	private readonly IDiscordOAuthTokenExchangeService _tokenExchangeService;

	public DiscordOAuthCallbackHandler(
		IOptions<DiscordOAuthIngressOptions> options,
		IDiscordIngressPendingStateStore pendingStateStore,
		IDiscordOAuthTokenExchangeService tokenExchangeService
	)
	{
		this._options = options ?? throw new ArgumentNullException(nameof(options));
		this._pendingStateStore = pendingStateStore ?? throw new ArgumentNullException(nameof(pendingStateStore));
		this._tokenExchangeService = tokenExchangeService ?? throw new ArgumentNullException(nameof(tokenExchangeService));
	}

	public async Task<DiscordOAuthCallbackResult> HandleAsync(DiscordOAuthCallbackRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		cancellationToken.ThrowIfCancellationRequested();

		if (!string.IsNullOrWhiteSpace(request.Error))
			return string.Equals(request.Error, "access_denied", StringComparison.OrdinalIgnoreCase)
				? DiscordOAuthCallbackResult.SecurityFailure(
					request.State,
					request.Code,
					null,
					null,
					request.CallbackUri,
					request.ErrorDescription ?? "Discord denied the OAuth authorization request.",
					request.Error,
					request.ErrorDescription,
					callbackParameters: request.QueryParameters)
				: DiscordOAuthCallbackResult.InvalidRequest(
					request.State,
					request.Code,
					request.ErrorDescription ?? "Discord returned an OAuth error response.",
					request.Error,
					request.ErrorDescription,
					request.QueryParameters,
					request.CallbackUri);

		if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.State))
			return DiscordOAuthCallbackResult.InvalidRequest(
				request.State,
				request.Code,
				"The OAuth callback must include both code and state query parameters.",
				callbackParameters: request.QueryParameters,
				callbackUri: request.CallbackUri);

		var options = this._options.Value;
		if (!options.IsConfigured)
			return DiscordOAuthCallbackResult.ConfigurationFailure(
				request.State,
				request.Code,
				"Discord OAuth ingress is not configured. Configure client credentials and a redirect URI before accepting callbacks.",
				request.QueryParameters,
				request.CallbackUri);

		var pendingState = await this._pendingStateStore.ConsumeAsync(request.State, cancellationToken).ConfigureAwait(false);
		if (pendingState is null)
			return DiscordOAuthCallbackResult.InvalidState(
				request.State,
				request.Code,
				"The OAuth state is invalid, expired, or has already been consumed.",
				request.QueryParameters,
				request.CallbackUri);

		var redirectUri = TryCreateAbsoluteUri(options.RedirectUri);
		var authorizationParameters = ExtractQueryParameters(pendingState.RequestUri);

		if (!string.Equals(pendingState.Flow, options.PendingStateFlow, StringComparison.Ordinal))
			return DiscordOAuthCallbackResult.SecurityFailure(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				"The pending OAuth state did not belong to the configured callback flow.",
				authorizationParameters: authorizationParameters,
				callbackParameters: request.QueryParameters);

		if (authorizationParameters.TryGetValue("state", out var storedState) && !string.IsNullOrWhiteSpace(storedState) && !string.Equals(storedState, request.State, StringComparison.Ordinal))
			return DiscordOAuthCallbackResult.SecurityFailure(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				"The stored OAuth state no longer matches the callback state.",
				authorizationParameters: authorizationParameters,
				callbackParameters: request.QueryParameters);

		if (authorizationParameters.TryGetValue("redirect_uri", out var storedRedirectUri) && !string.IsNullOrWhiteSpace(storedRedirectUri) && !UriEquals(storedRedirectUri, options.RedirectUri))
			return DiscordOAuthCallbackResult.SecurityFailure(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				"The stored OAuth redirect URI does not match the configured redirect URI.",
				authorizationParameters: authorizationParameters,
				callbackParameters: request.QueryParameters);

		try
		{
			var accessToken = await this._tokenExchangeService.ExchangeAccessTokenAsync(request.Code, cancellationToken).ConfigureAwait(false);
			return DiscordOAuthCallbackResult.Success(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				accessToken,
				authorizationParameters,
				request.QueryParameters,
				CopyAdditionalProperties(accessToken.AdditionalProperties));
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (SecurityException exception)
		{
			return DiscordOAuthCallbackResult.SecurityFailure(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				"Discord rejected the OAuth callback for security reasons.",
				authorizationParameters: authorizationParameters,
				callbackParameters: request.QueryParameters,
				exception: exception);
		}
		catch (Exception exception)
		{
			return DiscordOAuthCallbackResult.ExchangeFailure(
				request.State,
				request.Code,
				pendingState,
				redirectUri,
				request.CallbackUri,
				"The Discord OAuth code exchange failed.",
				authorizationParameters,
				request.QueryParameters,
				exception);
		}
	}

	private static IReadOnlyDictionary<string, string?> ExtractQueryParameters(Uri? requestUri)
	{
		if (requestUri is null || string.IsNullOrWhiteSpace(requestUri.Query))
			return EmptyStringProperties;

		Dictionary<string, string?> queryParameters = new(StringComparer.OrdinalIgnoreCase);
		foreach (var (key, value) in QueryHelpers.ParseQuery(requestUri.Query))
			queryParameters[key] = Normalize(value.ToString());

		return queryParameters.Count == 0
			? EmptyStringProperties
			: new ReadOnlyDictionary<string, string?>(queryParameters);
	}

	private static IReadOnlyDictionary<string, object?> CopyAdditionalProperties(IDictionary<string, object>? additionalProperties)
	{
		if (additionalProperties is null || additionalProperties.Count == 0)
			return EmptyObjectProperties;

		Dictionary<string, object?> copiedProperties = new(StringComparer.Ordinal);
		foreach (var (key, value) in additionalProperties)
			copiedProperties[key] = value;

		return new ReadOnlyDictionary<string, object?>(copiedProperties);
	}

	private static Uri? TryCreateAbsoluteUri(string? value)
		=> Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;

	private static bool UriEquals(string left, string? right)
	{
		if (!Uri.TryCreate(left, UriKind.Absolute, out var leftUri) || !Uri.TryCreate(right, UriKind.Absolute, out var rightUri))
			return false;

		return Uri.Compare(leftUri, rightUri, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal) == 0;
	}

	private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
