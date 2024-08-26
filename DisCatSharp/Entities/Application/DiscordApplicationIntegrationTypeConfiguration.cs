using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represets the application integration type configuration.
/// </summary>
public sealed class DiscordApplicationIntegrationTypeConfiguration : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApplicationIntegrationTypeConfiguration" /> class.
	/// </summary>
	internal DiscordApplicationIntegrationTypeConfiguration()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordApplicationIntegrationTypeConfiguration" /> class.
	/// </summary>
	/// <param name="oAuth2InstallParams">The oauth2 install params.</param>
	public DiscordApplicationIntegrationTypeConfiguration(DiscordOAuth2InstallParams? oAuth2InstallParams = null)
	{
		this.OAuth2InstallParams = oAuth2InstallParams;
	}

	/// <summary>
	///     Gets or sets the oauth2 install params.
	/// </summary>
	[JsonProperty("oauth2_install_params", NullValueHandling = NullValueHandling.Include)]
	public DiscordOAuth2InstallParams? OAuth2InstallParams { get; internal set; } = null;
}
