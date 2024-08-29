using System;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     Represents information about the game state's timestamps.
/// </summary>
public class TransportDiscordGameTimestamps
{
	[JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
	internal long? EndInternal;

	[JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
	internal long? StartInternal;

	/// <summary>
	///     Gets the time the game has started.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? Start
		=> this.StartInternal != null ? Utilities.GetDateTimeOffsetFromMilliseconds(this.StartInternal.Value, false) : null;

	/// <summary>
	///     Gets the time the game is going to end.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? End
		=> this.EndInternal != null ? Utilities.GetDateTimeOffsetFromMilliseconds(this.EndInternal.Value, false) : null;
}
