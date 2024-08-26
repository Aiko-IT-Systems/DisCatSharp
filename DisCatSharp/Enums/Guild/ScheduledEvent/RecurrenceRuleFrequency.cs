namespace DisCatSharp.Enums;

/// <summary>
///     Represents the frequency of a scheduled event's recurrence.
/// </summary>
public enum RecurrenceRuleFrequency
{
	/// <summary>
	///     The scheduled event repeats yearly.
	/// </summary>
	Yearly = 0,

	/// <summary>
	///     The scheduled event repeats monthly.
	/// </summary>
	Monthly = 1,

	/// <summary>
	///     The scheduled event repeats weekly.
	/// </summary>
	Weekly = 2,

	/// <summary>
	///     The scheduled event repeats daily.
	/// </summary>
	Daily = 3
}
