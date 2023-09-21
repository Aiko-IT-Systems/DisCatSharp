using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents the oauth2 application role connection payload.
/// </summary>
internal sealed class RestOAuth2ApplicationRoleConnectionPayload : ObservableApiObject
{
	/// <summary>
	/// Sets the role connections new platform name.
	/// </summary>
	[JsonProperty("platform_name")]
	public Optional<string> PlatformName { internal get; set; }

	/// <summary>
	/// Sets the role connections new platform username.
	/// </summary>
	[JsonProperty("platform_username")]
	public Optional<string> PlatformUsername { internal get; set; }

	/// <summary>
	/// Sets the role connections new metadata.
	/// </summary>
	[JsonProperty("metadata")]
	public Optional<Dictionary<string, string>> Metadata { internal get; set; }
}
