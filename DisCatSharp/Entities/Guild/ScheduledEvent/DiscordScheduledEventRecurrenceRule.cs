using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the recurrence rule for a scheduled event.
/// </summary>
public sealed class DiscordScheduledEventRecurrenceRule
{
	/// <summary>
	///     Gets or sets the start time of the recurrence interval.
	/// </summary>
	[JsonProperty("start")]
	public DateTimeOffset Start { get; set; }

	/// <summary>
	///     Gets the end time of the recurrence interval.
	/// </summary>
	[JsonProperty("end")]
	public DateTimeOffset? End { get; internal set; }

	/// <summary>
	///     Gets or sets the frequency of the recurrence.
	/// </summary>
	[JsonProperty("frequency")]
	public RecurrenceRuleFrequency Frequency { get; set; }

	/// <summary>
	///     Gets or sets the interval between events.
	/// </summary>
	[JsonProperty("interval")]
	public int Interval { get; set; }

	/// <summary>
	///     Gets or sets specific days within a week for the event to recur on.
	/// </summary>
	[JsonProperty("by_weekday", NullValueHandling = NullValueHandling.Include)]
	public List<RecurrenceRuleWeekday>? ByWeekday { get; set; }

	/// <summary>
	///     Gets or sets specific days within a specific week (1-5) to recur on.
	/// </summary>
	[JsonProperty("by_n_weekday", NullValueHandling = NullValueHandling.Include)]
	public List<DiscordRecurrenceRuleNWeekday>? ByNWeekday { get; set; }

	/// <summary>
	///     Gets or sets specific months to recur on.
	/// </summary>
	[JsonProperty("by_month", NullValueHandling = NullValueHandling.Include)]
	public List<int>? ByMonth { get; set; }

	/// <summary>
	///     Gets or sets specific dates within a month to recur on.
	/// </summary>
	[JsonProperty("by_month_day", NullValueHandling = NullValueHandling.Include)]
	public List<int>? ByMonthDay { get; set; }

	/// <summary>
	///     Gets specific dates within a year to recur on.
	/// </summary>
	[JsonProperty("by_year_day", NullValueHandling = NullValueHandling.Include)]
	public List<int>? ByYearDay { get; internal set; }

	/// <summary>
	///     Gets the total amount of times that the event is allowed to recur before stopping.
	/// </summary>
	[JsonProperty("count", NullValueHandling = NullValueHandling.Include)]
	public int? Count { get; internal set; }
}
