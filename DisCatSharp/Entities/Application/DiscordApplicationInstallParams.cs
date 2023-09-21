using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// The application install params.
/// </summary>
public sealed class DiscordApplicationInstallParams : ObservableApiObject
{
	/// <summary>
	/// Gets the scopes.
	/// </summary>
	[JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Scopes { get; internal set; }

	/// <summary>
	/// Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? Permissions { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordApplicationInstallParams"/> class.
	/// </summary>
	internal DiscordApplicationInstallParams()
	{ }

	public DiscordApplicationInstallParams(List<string>? scopes = null, Permissions? permissions = null)
	{
		this.Scopes = scopes;
		this.Permissions = permissions;
	}
}
