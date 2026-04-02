using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
///     Configuration for Discord API protocol settings.
/// </summary>
public sealed class ApiConfiguration
{
	/// <summary>
	///     Creates a new API configuration with default values.
	/// </summary>
	public ApiConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another API configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public ApiConfiguration(ApiConfiguration other)
	{
		this.Version = other.Version;
		this.Channel = other.Channel;
		this.Override = other.Override;
		this.Locale = other.Locale;
		this.Timezone = other.Timezone;
	}

	/// <summary>
	///     <para>Overwrites the API version.</para>
	///     <para>Defaults to <c>10</c>.</para>
	/// </summary>
	public string Version { internal get; set; } = "10";

	/// <summary>
	///     <para>Which API channel to use.</para>
	///     <para>Defaults to <see cref="ApiChannel.Stable" />.</para>
	/// </summary>
	public ApiChannel Channel { internal get; set; } = ApiChannel.Stable;

	/// <summary>
	///     <para>Do not use, this is meant for DisCatSharp Devs.</para>
	///     <para>Defaults to <see langword="null" />.</para>
	/// </summary>
	public string? Override { internal get; set; } = null;

	/// <summary>
	///     Sets your preferred API language. See <see cref="DiscordLocales" /> for valid locales.
	/// </summary>
	public string Locale { internal get; set; } = DiscordLocales.AMERICAN_ENGLISH;

	/// <summary>
	///     Sets your timezone.
	/// </summary>
	public string? Timezone { internal get; set; } = null;
}
