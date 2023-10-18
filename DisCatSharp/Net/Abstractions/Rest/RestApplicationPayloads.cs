using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

internal sealed class RestApplicationModifyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> Description { get; set; }

	/// <summary>
	/// Gets or sets the interactions endpoint url.
	/// </summary>
	[JsonProperty("interactions_endpoint_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> InteractionsEndpointUrl { get; set; }

	/// <summary>
	/// Gets or sets the role connections verification url.
	/// </summary>
	[JsonProperty("role_connections_verification_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> RoleConnectionsVerificationUrl { get; set; }

	/// <summary>
	/// Gets or sets the custom install url.
	/// </summary>
	[JsonProperty("custom_install_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> CustomInstallUrl { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	[JsonProperty("tags", NullValueHandling = NullValueHandling.Include)]
	public Optional<List<string>?> Tags { get; set; }

	/// <summary>
	/// Gets or sets the icon base64.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> IconBase64 { get; set; }

	/// <summary>
	/// Gets or sets the cover image base64.
	/// </summary>
	[JsonProperty("cover_image", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> ConverImageBase64 { get; set; }

	/// <summary>
	/// Gets or sets the application flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ApplicationFlags> Flags { get; set; }

	/// <summary>
	/// Gets or sets the install params.
	/// </summary>
	[JsonProperty("install_params", NullValueHandling = NullValueHandling.Include)]
	public Optional<DiscordApplicationInstallParams?> InstallParams { get; set; }
}
