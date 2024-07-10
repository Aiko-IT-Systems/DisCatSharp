using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a bulk ban response
/// </summary>
public sealed class DiscordBulkBanResponse : ObservableApiObject
{
	/// <summary>
	/// Gets the list of user ids, that were successfully banned.
	/// </summary>
	[JsonProperty("banned_users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> BannedUsers { get; internal set; } = [];

	/// <summary>
	/// <para>Gets the list of user ids, that were not banned.</para>
	/// <para>These users were either already banned or could not be banned due to various reasons.</para>
	/// </summary>
	[JsonProperty("failed_users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> FailedUsers { get; internal set; } = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordBulkBanResponse"/> class.
	/// </summary>
	internal DiscordBulkBanResponse()
	{ }
}
