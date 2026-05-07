namespace DisCatSharp.Hosting.AspNetCore.Ingress.OAuth;

/// <summary>
///     Configures the Discord OAuth callback flow exposed by the ASP.NET Core ingress package.
/// </summary>
public sealed class DiscordOAuthIngressOptions
{
	/// <summary>
	///     The default logical flow name used when storing pending OAuth state.
	/// </summary>
	public const string DefaultPendingStateFlow = "oauth";

	/// <summary>
	///     Gets or sets the Discord application client identifier used for token exchange.
	/// </summary>
	public ulong ClientId { get; set; }

	/// <summary>
	///     Gets or sets the Discord application client secret used for token exchange.
	/// </summary>
	public string? ClientSecret { get; set; }

	/// <summary>
	///     Gets or sets the redirect URI that Discord should use for the authorization-code callback.
	/// </summary>
	public string? RedirectUri { get; set; }

	/// <summary>
	///     Gets or sets the expected logical flow name for pending OAuth state entries.
	/// </summary>
	public string PendingStateFlow { get; set; } = DefaultPendingStateFlow;

	/// <summary>
	///     Gets a value indicating whether the OAuth callback flow has enough configuration to exchange codes.
	/// </summary>
	public bool IsConfigured
		=> this.ClientId != 0 && !string.IsNullOrWhiteSpace(this.ClientSecret) && !string.IsNullOrWhiteSpace(this.RedirectUri);
}
