using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
/// Represents a <see cref="DiscordApplicationRoleConnection"/>.
/// </summary>
public sealed class DiscordApplicationRoleConnection : ObservableApiObject
{
	/// <summary>
	/// Gets the platform name.
	/// </summary>
	[JsonProperty("platform_name", NullValueHandling = NullValueHandling.Include)]
	public string? PlatformName { get; internal set; }

	/// <summary>
	/// Gets the platform username.
	/// </summary>
	[JsonProperty("platform_username", NullValueHandling = NullValueHandling.Include)]
	public string? PlatformUsername { get; internal set; }

	/// <summary>
	/// Gets the raw role connection metadata.
	/// </summary>
	[JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string> RawMetadata { get; set; }

	/// <summary>
	/// Gets the role connection metadata.
	/// </summary>
	[JsonIgnore]
	public ApplicationRoleConnectionMetadata Metadata
		=> new(this.RawMetadata);
}

/// <summary>
/// Represents a <see cref="ApplicationRoleConnectionMetadata"/>.
/// </summary>
public sealed class ApplicationRoleConnectionMetadata
{
	/// <summary>
	/// <para>Gets the metadata.</para>
	/// <para>The <c>key</c> is the metadata key.</para>
	/// <para>The <c>value</c> is the string-ified value of the metadata.</para>
	/// </summary>
	public Dictionary<string, string> Metadata { get; internal set; } = [];

	/// <summary>
	/// Initializes a new instance of <see cref="ApplicationRoleConnectionMetadata"/>.
	/// </summary>
	internal ApplicationRoleConnectionMetadata()
	{ }

	/// <summary>
	/// Initializes a new instance of <see cref="ApplicationRoleConnectionMetadata"/>.
	/// </summary>
	/// <param name="metadata">The metadata.</param>
	internal ApplicationRoleConnectionMetadata(Dictionary<string, string> metadata)
	{
		this.Metadata = metadata;
	}

	/// <summary>
	/// Adds a metadata.
	/// </summary>
	/// <param name="key">The key to add.</param>
	/// <param name="value">The value to add.</param>
	public ApplicationRoleConnectionMetadata AddMetadata(string key, string value)
	{
		this.Metadata.Add(key, value);
		return this;
	}

	/// <summary>
	/// Removes a metadata by its key.
	/// </summary>
	/// <param name="key">The key to remove.</param>
	public ApplicationRoleConnectionMetadata RemoveMetadata(string key)
	{
		this.Metadata.Remove(key);
		return this;
	}

	/// <summary>
	/// Gets the KVPs.
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, string> GetKeyValuePairs()
		=> this.Metadata;
}
