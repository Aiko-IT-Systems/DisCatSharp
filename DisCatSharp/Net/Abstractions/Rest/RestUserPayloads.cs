using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a user dm create payload.
/// </summary>
internal sealed class RestUserDmCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the recipient.
	/// </summary>
	[JsonProperty("recipient_id")]
	public ulong Recipient { get; set; }
}

/// <summary>
/// Represents a user group dm create payload.
/// </summary>
internal sealed class RestUserGroupDmCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the access tokens.
	/// </summary>
	[JsonProperty("access_tokens")]
	public IEnumerable<string> AccessTokens { get; set; }

	/// <summary>
	/// Gets or sets the nicknames.
	/// </summary>
	[JsonProperty("nicks")]
	public IDictionary<ulong, string> Nicknames { get; set; }
}

/// <summary>
/// Represents a user update current payload.
/// </summary>
internal sealed class RestUserUpdateCurrentPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the username.
	/// </summary>
	[JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
	public string Username { get; set; }

	/// <summary>
	/// Gets or sets the avatar base64.
	/// </summary>
	[JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
	public Optional<string?> AvatarBase64 { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether avatar set.
	/// </summary>
	[JsonIgnore]
	public bool AvatarSet { get; set; }

	/// <summary>
	/// Gets whether the avatar should be serialized.
	/// </summary>
	public bool ShouldSerializeAvatarBase64()
		=> this.AvatarSet;
}

/// <summary>
/// Represents a user guild.
/// </summary>
internal sealed class RestUserGuild : ObservableApiObject
{
	/// <summary>
	/// Gets the id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Id { get; set; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	/// Gets the icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string IconHash { get; set; }

	/// <summary>
	/// Gets a value indicating whether is owner.
	/// </summary>
	[JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsOwner { get; set; }

	/// <summary>
	/// Gets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Permissions { get; set; }

	/// <summary>
	/// Gets the guild features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Features { get; set; }
}

/// <summary>
/// Represents a user guild list payload.
/// </summary>
internal sealed class RestUserGuildListPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the limit.
	/// </summary>
	[JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
	public int Limit { get; set; }

	/// <summary>
	/// Gets or sets the before.
	/// </summary>
	[JsonProperty("before", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? Before { get; set; }

	/// <summary>
	/// Gets or sets the after.
	/// </summary>
	[JsonProperty("after", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? After { get; set; }
}
