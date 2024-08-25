using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a specific day within a specific week to recur on.
/// </summary>
public sealed class DiscordRecurrenceRuleNWeekday
{
	/// <summary>
	/// Gets or sets the week number (1-5) for recurrence.
	/// </summary>
	[JsonProperty("n")]
	public int WeekNumber { get; set; }

	/// <summary>
	/// Gets or sets the day of the week for recurrence.
	/// </summary>
	[JsonProperty("day")]
	public RecurrenceRuleWeekday Day { get; set; }
}
