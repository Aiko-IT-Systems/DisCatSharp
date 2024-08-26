using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the application integration type configuration.
/// </summary>
public sealed class DiscordIntegrationTypesConfig
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIntegrationTypesConfig" /> class.
	/// </summary>
	internal DiscordIntegrationTypesConfig()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordIntegrationTypesConfig" /> class.
	/// </summary>
	/// <param name="guildInstall">The guild install configuration. Defaults to <see langword="null" />.</param>
	/// <param name="userInstall">The user install configuration. Defaults to <see langword="null" />.</param>
	public DiscordIntegrationTypesConfig(DiscordApplicationIntegrationTypeConfiguration? guildInstall = null, DiscordApplicationIntegrationTypeConfiguration? userInstall = null)
	{
		this.GuildInstall = guildInstall;
		this.UserInstall = userInstall;
	}

	/// <summary>
	///     <para>Gets or sets the guild install configuration.</para>
	///     <para>Disabled when <see langword="null" />.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	[JsonProperty("0", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordApplicationIntegrationTypeConfiguration? GuildInstall { get; internal set; } = null;

	/// <summary>
	///     <para>Gets or sets the user install configuration.</para>
	///     <para>Disabled when <see langword="null" />.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	[JsonProperty("1", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordApplicationIntegrationTypeConfiguration? UserInstall { get; internal set; } = null;
}
