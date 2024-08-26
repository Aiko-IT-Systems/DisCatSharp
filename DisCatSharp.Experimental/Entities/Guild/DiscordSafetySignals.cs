using Newtonsoft.Json;

namespace DisCatSharp.Experimental.Entities;

/// <summary>
///     Represents safety signal queries.
/// </summary>
public sealed class DiscordSafetySignals
{
	/// <summary>
	///     When the member's unusual DM activity flag will expire.
	///     Possible query types: <see cref="DiscordQuery.RangeQuery" />.
	/// </summary>
	[JsonProperty("unusual_dm_activity_until", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? UnusualDmActivityUntil { get; set; }

	/// <summary>
	///     When the member's timeout will expire.
	///     Possible query types: <see cref="DiscordQuery.RangeQuery" />.
	/// </summary>
	[JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordQuery? CommunicationDisabledUntil { get; set; }

	/// <summary>
	///     Whether unusual account activity is detected.
	/// </summary>
	[JsonProperty("unusual_account_activity", NullValueHandling = NullValueHandling.Ignore)]
	public bool? UnusualAccountActivity { get; set; }

	/// <summary>
	///     Whether the member has been indefinitely quarantined by an AutoMod Rule for their username, display name, or
	///     nickname.
	/// </summary>
	[JsonProperty("automod_quarantined_username", NullValueHandling = NullValueHandling.Ignore)]
	public bool? AutomodQuarantinedUsername { get; set; }
}
