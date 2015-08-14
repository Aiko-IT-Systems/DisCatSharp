using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities.OAuth2;

/// <summary>
/// Represents a <see cref="DiscordAuthorizationInformation"/>.
/// </summary>
public sealed class DiscordAuthorizationInformation : ObservableApiObject
{
	/// <summary>
	/// Gets the current application.
	/// </summary>
	[JsonProperty("application")]
	public DiscordApplication Application { get; internal set; }

	/// <summary>
	/// Gets the scopes the user has authorized the application for.
	/// </summary>
	[JsonProperty("scopes")]
	public List<string> Scopes { get; internal set; } = [];

	/// <summary>
	/// Gets when the access token expires as raw string.
	/// </summary>
	[JsonProperty("expires")]
	internal string ExpiresRaw { get; set; }

	/// <summary>
	/// Gets when the access token expires.
	/// </summary>
	[JsonIgnore]
	internal DateTimeOffset Expires
		=> DateTimeOffset.TryParse(this.ExpiresRaw, out var expires)
			? expires
			: throw new InvalidCastException("Something went wrong");

	/// <summary>
	/// Gets the user who has authorized, if the user has authorized with the <c>identify</c> scope.
	/// </summary>
	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser? User { get; internal set; }
}
