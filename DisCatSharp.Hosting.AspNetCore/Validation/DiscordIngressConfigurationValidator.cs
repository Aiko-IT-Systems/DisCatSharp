using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Hosting.AspNetCore.Ingress;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Validates local ingress options against computed public URLs and Discord application metadata.
/// </summary>
public static class DiscordIngressConfigurationValidator
{
	/// <summary>
	///     Validates a local ASP.NET Core ingress configuration snapshot.
	/// </summary>
	/// <param name="context">The configuration snapshot to validate.</param>
	/// <returns>A report containing computed public URLs and any discovered issues.</returns>
	public static DiscordIngressValidationReport Validate(DiscordIngressValidationContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var aspNetCoreOptions = context.AspNetCoreOptions ?? new DiscordAspNetCoreIngressOptions();
		var webOptions = context.WebOptions ?? new DiscordWebIngressOptions();
		var oauthOptions = context.OAuthOptions ?? new DiscordOAuthIngressOptions();
		var linkedRolesOptions = context.LinkedRolesOptions;
		List<DiscordIngressValidationIssue> issues = [];

		DiscordIngressPublicUrls? publicUrls = null;
		if (context.PublicBaseUrl is not null)
		{
			if (!context.PublicBaseUrl.IsAbsoluteUri)
				issues.Add(new("public-base-url-invalid", DiscordIngressValidationSeverity.Error, "The supplied public base URL must be absolute."));
			else if (!string.IsNullOrEmpty(context.PublicBaseUrl.Query) || !string.IsNullOrEmpty(context.PublicBaseUrl.Fragment))
				issues.Add(new("public-base-url-components", DiscordIngressValidationSeverity.Error, "The supplied public base URL cannot include query string or fragment components."));
			else
				publicUrls = DiscordIngressPublicUrls.Create(context.PublicBaseUrl, aspNetCoreOptions, linkedRolesOptions);
		}

		ValidateOAuthOptions(oauthOptions, publicUrls, issues);
		ValidateOAuthClient(context.OAuthClient, oauthOptions, issues);
		ValidateApplication(context.Application, publicUrls, webOptions, oauthOptions, context.ExpectedRoleConnectionsVerificationUrl ?? publicUrls?.RoleConnectionsVerificationUrl, issues);

		return new(publicUrls, issues);
	}

	private static void ValidateOAuthOptions(
		DiscordOAuthIngressOptions oauthOptions,
		DiscordIngressPublicUrls? publicUrls,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		if (string.IsNullOrWhiteSpace(oauthOptions.RedirectUri))
			return;

		if (!Uri.TryCreate(oauthOptions.RedirectUri, UriKind.Absolute, out var redirectUri))
		{
			issues.Add(new("oauth-redirect-uri-invalid", DiscordIngressValidationSeverity.Error, "The configured OAuth redirect URI must be an absolute URI."));
			return;
		}

		if (publicUrls is not null && !UriEquals(redirectUri, publicUrls.OAuthCallbackUrl))
			issues.Add(new("oauth-redirect-uri-mismatch", DiscordIngressValidationSeverity.Error, $"The configured OAuth redirect URI does not match the computed callback URL '{publicUrls.OAuthCallbackUrl}'."));
	}

	private static void ValidateOAuthClient(
		DiscordOAuth2Client? oauthClient,
		DiscordOAuthIngressOptions oauthOptions,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		if (oauthClient is null)
			return;

		if (oauthOptions.ClientId != 0 && oauthOptions.ClientId != oauthClient.ClientId)
			issues.Add(new("oauth-client-id-mismatch", DiscordIngressValidationSeverity.Error, "The configured OAuth client ID does not match the active DiscordOAuth2Client instance."));

		if (!string.IsNullOrWhiteSpace(oauthOptions.ClientSecret) && !string.Equals(oauthOptions.ClientSecret, oauthClient.ClientSecret, StringComparison.Ordinal))
			issues.Add(new("oauth-client-secret-mismatch", DiscordIngressValidationSeverity.Error, "The configured OAuth client secret does not match the active DiscordOAuth2Client instance."));

		if (!string.IsNullOrWhiteSpace(oauthOptions.RedirectUri)
			&& Uri.TryCreate(oauthOptions.RedirectUri, UriKind.Absolute, out var redirectUri)
			&& !UriEquals(redirectUri, oauthClient.RedirectUri))
			issues.Add(new("oauth-client-redirect-mismatch", DiscordIngressValidationSeverity.Error, "The configured OAuth redirect URI does not match the active DiscordOAuth2Client instance."));
	}

	private static void ValidateApplication(
		DiscordApplication? application,
		DiscordIngressPublicUrls? publicUrls,
		DiscordWebIngressOptions webOptions,
		DiscordOAuthIngressOptions oauthOptions,
		Uri? expectedRoleConnectionsVerificationUrl,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		if (application is null)
			return;

		if (oauthOptions.ClientId != 0 && application.Id != oauthOptions.ClientId)
			issues.Add(new("application-client-id-mismatch", DiscordIngressValidationSeverity.Error, "The configured OAuth client ID does not match the current Discord application."));

		ValidateVerifyKey(application, webOptions, issues);
		ValidateRedirectUris(application, oauthOptions, publicUrls, issues);
		ValidateInteractionsUrl(application, publicUrls, issues);
		ValidateRoleConnectionsUrl(application, expectedRoleConnectionsVerificationUrl, issues);
	}

	private static void ValidateVerifyKey(
		DiscordApplication application,
		DiscordWebIngressOptions webOptions,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		var applicationVerifyKey = NormalizeHex(application.VerifyKey);
		var configuredVerifyKey = NormalizeHex(webOptions.ApplicationVerifyKey);

		if (string.IsNullOrWhiteSpace(applicationVerifyKey))
			return;

		if (string.IsNullOrWhiteSpace(configuredVerifyKey))
		{
			issues.Add(new("verify-key-missing", DiscordIngressValidationSeverity.Warning, "Discord exposes an application verify key, but DiscordWebIngressOptions.ApplicationVerifyKey is not configured."));
			return;
		}

		if (!string.Equals(applicationVerifyKey, configuredVerifyKey, StringComparison.Ordinal))
			issues.Add(new("verify-key-mismatch", DiscordIngressValidationSeverity.Error, "The configured ingress verify key does not match the Discord application verify key."));
	}

	private static void ValidateRedirectUris(
		DiscordApplication application,
		DiscordOAuthIngressOptions oauthOptions,
		DiscordIngressPublicUrls? publicUrls,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		Uri? configuredRedirectUri = null;
		if (!string.IsNullOrWhiteSpace(oauthOptions.RedirectUri))
		{
			if (Uri.TryCreate(oauthOptions.RedirectUri, UriKind.Absolute, out var redirectUri))
				configuredRedirectUri = redirectUri;
		}
		else if (publicUrls is not null)
		{
			configuredRedirectUri = publicUrls.OAuthCallbackUrl;
		}

		if (configuredRedirectUri is null)
			return;

		if (application.RedirectUris.Count is 0)
		{
			issues.Add(new("redirect-uri-not-registered", DiscordIngressValidationSeverity.Warning, "The Discord application does not expose any registered redirect URIs to validate against."));
			return;
		}

		var registeredUris = application.RedirectUris
			.Select(static value => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null)
			.Where(static uri => uri is not null)
			.Cast<Uri>()
			.ToArray();

		if (!registeredUris.Any(candidate => UriEquals(candidate, configuredRedirectUri)))
			issues.Add(new("redirect-uri-mismatch", DiscordIngressValidationSeverity.Error, $"The configured OAuth redirect URI '{configuredRedirectUri}' is not registered on the Discord application."));
	}

	private static void ValidateInteractionsUrl(
		DiscordApplication application,
		DiscordIngressPublicUrls? publicUrls,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		if (publicUrls is null)
			return;

		if (string.IsNullOrWhiteSpace(application.InteractionsEndpointUrl))
		{
			issues.Add(new("interactions-endpoint-missing", DiscordIngressValidationSeverity.Warning, "The Discord application does not have an interactions endpoint URL configured."));
			return;
		}

		if (!Uri.TryCreate(application.InteractionsEndpointUrl, UriKind.Absolute, out var interactionsEndpoint))
		{
			issues.Add(new("interactions-endpoint-invalid", DiscordIngressValidationSeverity.Warning, "The Discord application interactions endpoint URL is not a valid absolute URI."));
			return;
		}

		if (!UriEquals(interactionsEndpoint, publicUrls.InteractionsUrl))
			issues.Add(new("interactions-endpoint-mismatch", DiscordIngressValidationSeverity.Error, $"The Discord application interactions endpoint URL does not match the computed public URL '{publicUrls.InteractionsUrl}'."));
	}

	private static void ValidateRoleConnectionsUrl(
		DiscordApplication application,
		Uri? expectedRoleConnectionsVerificationUrl,
		ICollection<DiscordIngressValidationIssue> issues)
	{
		if (expectedRoleConnectionsVerificationUrl is null)
		{
			if (!string.IsNullOrWhiteSpace(application.RoleConnectionsVerificationUrl))
				issues.Add(new("role-connections-url-unvalidated", DiscordIngressValidationSeverity.Info, "The Discord application exposes a role-connections verification URL, but no expected local URL was supplied for validation."));

			return;
		}

		if (!expectedRoleConnectionsVerificationUrl.IsAbsoluteUri)
		{
			issues.Add(new("role-connections-url-invalid", DiscordIngressValidationSeverity.Error, "The expected role-connections verification URL must be absolute."));
			return;
		}

		if (string.IsNullOrWhiteSpace(application.RoleConnectionsVerificationUrl))
		{
			issues.Add(new("role-connections-url-missing", DiscordIngressValidationSeverity.Warning, "The Discord application does not have a role-connections verification URL configured."));
			return;
		}

		if (!Uri.TryCreate(application.RoleConnectionsVerificationUrl, UriKind.Absolute, out var configuredRoleConnectionsVerificationUrl))
		{
			issues.Add(new("role-connections-url-portal-invalid", DiscordIngressValidationSeverity.Warning, "The Discord application role-connections verification URL is not a valid absolute URI."));
			return;
		}

		if (!UriEquals(configuredRoleConnectionsVerificationUrl, expectedRoleConnectionsVerificationUrl))
			issues.Add(new("role-connections-url-mismatch", DiscordIngressValidationSeverity.Error, "The Discord application role-connections verification URL does not match the supplied expected public URL."));
	}

	private static string? NormalizeHex(string? value)
		=> string.IsNullOrWhiteSpace(value)
			? null
			: value.Trim().ToUpperInvariant();

	private static bool UriEquals(Uri left, Uri right)
		=> string.Equals(NormalizeUri(left), NormalizeUri(right), StringComparison.Ordinal);

	private static string NormalizeUri(Uri uri)
	{
		var normalizedPath = uri.AbsolutePath.Length > 1
			? uri.AbsolutePath.TrimEnd('/')
			: uri.AbsolutePath;

		UriBuilder builder = new(uri)
		{
			Scheme = uri.Scheme.ToLowerInvariant(),
			Host = uri.Host.ToLowerInvariant(),
			Path = string.IsNullOrEmpty(normalizedPath) ? "/" : normalizedPath,
			Query = string.Empty,
			Fragment = string.Empty
		};

		return builder.Uri.AbsoluteUri;
	}
}
