using System;

using DisCatSharp;
using DisCatSharp.Entities;

namespace DisCatSharp.Hosting.AspNetCore;

/// <summary>
///     Describes the local ingress configuration and optional Discord application state to validate.
/// </summary>
public sealed class DiscordIngressValidationContext
{
	/// <summary>
	///     Gets or sets the externally visible base URL for the ASP.NET Core app.
	/// </summary>
	public Uri? PublicBaseUrl { get; set; }

	/// <summary>
	///     Gets or sets the expected public role-connections verification URL when the app exposes one outside the package.
	/// </summary>
	public Uri? ExpectedRoleConnectionsVerificationUrl { get; set; }

	/// <summary>
	///     Gets or sets the local ASP.NET Core ingress route options.
	/// </summary>
	public DiscordAspNetCoreIngressOptions? AspNetCoreOptions { get; set; }

	/// <summary>
	///     Gets or sets the local transport-agnostic ingress options.
	/// </summary>
	public Ingress.DiscordWebIngressOptions? WebOptions { get; set; }

	/// <summary>
	///     Gets or sets the local OAuth callback flow options.
	/// </summary>
	public Ingress.DiscordOAuthIngressOptions? OAuthOptions { get; set; }

	/// <summary>
	///     Gets or sets the local linked-roles helper options.
	/// </summary>
	public DiscordLinkedRolesOptions? LinkedRolesOptions { get; set; }

	/// <summary>
	///     Gets or sets the current Discord application snapshot from the developer portal or API.
	/// </summary>
	public DiscordApplication? Application { get; set; }

	/// <summary>
	///     Gets or sets the active OAuth2 client used by the host application.
	/// </summary>
	public DiscordOAuth2Client? OAuthClient { get; set; }
}
