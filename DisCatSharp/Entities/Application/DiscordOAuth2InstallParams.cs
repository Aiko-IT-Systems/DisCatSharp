using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents Discord OAuth2 install parameters. Is the same model as <see cref="DiscordApplicationInstallParams" />,
///     which is obsolete in future.
/// </summary>
public sealed class DiscordOAuth2InstallParams
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordOAuth2InstallParams" /> class.
	/// </summary>
	internal DiscordOAuth2InstallParams()
	{ }

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordOAuth2InstallParams" /> class.
	/// </summary>
	/// <param name="scopes">The scopes.</param>
	/// <param name="permissions">The permissions.</param>
	public DiscordOAuth2InstallParams(List<string>? scopes = null, Permissions? permissions = null)
	{
		this.Scopes = scopes;
		this.Permissions = permissions;
	}

	/// <summary>
	///     Gets the scopes.
	/// </summary>
	[JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Scopes { get; internal set; }

	/// <summary>
	///     Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? Permissions { get; internal set; }
}
