using System.Collections.Generic;
using System.Collections.ObjectModel;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents user counts for a scheduled event and its exceptions.
/// </summary>
public sealed class DiscordScheduledEventUserCounts : ObservableApiObject
{
	/// <summary>
	///     Gets the user count for the scheduled event itself.
	/// </summary>
	[JsonProperty("guild_scheduled_event_count")]
	public int ScheduledEventCount { get; internal set; }

	/// <summary>
	///     Gets the user counts for the requested scheduled event exceptions.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, int> ExceptionCounts
		=> new ReadOnlyDictionary<ulong, int>(this.ExceptionCountsInternal);

	/// <summary>
	///     Gets or sets the raw exception counts returned by the API.
	/// </summary>
	[JsonProperty("guild_scheduled_event_exception_counts", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<ulong, int> ExceptionCountsInternal { get; set; } = [];
}
