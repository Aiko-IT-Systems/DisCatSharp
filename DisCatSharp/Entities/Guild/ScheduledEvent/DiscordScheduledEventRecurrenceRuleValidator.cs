using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
///     Validator for <see cref="DiscordScheduledEventRecurrenceRule" /> instances.
/// </summary>
public static class DiscordScheduledEventRecurrenceRuleValidator
{
	/// <summary>
	///     A collection of valid weekday sets for events with a daily recurrence frequency.
	/// </summary>
	private static readonly List<List<RecurrenceRuleWeekday>> s_validDailyWeekdaySets = [CreateWeekdaySet(RecurrenceRuleWeekday.Monday, RecurrenceRuleWeekday.Tuesday, RecurrenceRuleWeekday.Wednesday, RecurrenceRuleWeekday.Thursday, RecurrenceRuleWeekday.Friday), CreateWeekdaySet(RecurrenceRuleWeekday.Tuesday, RecurrenceRuleWeekday.Wednesday, RecurrenceRuleWeekday.Thursday, RecurrenceRuleWeekday.Friday, RecurrenceRuleWeekday.Saturday), CreateWeekdaySet(RecurrenceRuleWeekday.Sunday, RecurrenceRuleWeekday.Monday, RecurrenceRuleWeekday.Tuesday, RecurrenceRuleWeekday.Wednesday, RecurrenceRuleWeekday.Thursday), CreateWeekdaySet(RecurrenceRuleWeekday.Friday, RecurrenceRuleWeekday.Saturday), CreateWeekdaySet(RecurrenceRuleWeekday.Saturday, RecurrenceRuleWeekday.Sunday), CreateWeekdaySet(RecurrenceRuleWeekday.Sunday, RecurrenceRuleWeekday.Monday)];

	/// <summary>
	///     Creates a set of weekdays for easier readability.
	/// </summary>
	/// <param name="weekdays">The weekdays to include in the set.</param>
	/// <returns>A list of <see cref="RecurrenceRuleWeekday" /> objects.</returns>
	private static List<RecurrenceRuleWeekday> CreateWeekdaySet(params RecurrenceRuleWeekday[] weekdays)
		=> [.. weekdays];

	/// <summary>
	///     Validates the recurrence rule.
	/// </summary>
	/// <param name="rule">The recurrence rule to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	public static (bool IsValid, string? ErrorMessage) Validate(this DiscordScheduledEventRecurrenceRule rule)
	{
		return rule.ByWeekday is not null && rule.ByNWeekday is not null
			? (false, "by_weekday and by_n_weekday cannot both be set.")
			: rule.ByMonth is not null && rule.ByMonthDay is not null
				? (false, "by_month and by_month_day cannot both be set.")
				: rule.Frequency switch
				{
					RecurrenceRuleFrequency.Daily => rule.ValidateDailyFrequency(),
					RecurrenceRuleFrequency.Weekly => rule.ValidateWeeklyFrequency(),
					RecurrenceRuleFrequency.Monthly => rule.ValidateMonthlyFrequency(),
					RecurrenceRuleFrequency.Yearly => rule.ValidateYearlyFrequency(),
					_ => (false, "Unknown frequency type.")
				};
	}

	/// <summary>
	///     Validates recurrence rules with a daily frequency.
	/// </summary>
	/// <param name="rule">The recurrence rule to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	private static (bool IsValid, string? ErrorMessage) ValidateDailyFrequency(this DiscordScheduledEventRecurrenceRule rule)
		=> rule.ByWeekday is not null && !rule.ByWeekday.IsValidDailyWeekdaySet()
			? ((bool IsValid, string? ErrorMessage))(false, "Invalid by_weekday set for daily frequency.")
			: (true, null);

	/// <summary>
	///     Validates recurrence rules with a weekly frequency.
	/// </summary>
	/// <param name="rule">The recurrence rule to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	private static (bool IsValid, string? ErrorMessage) ValidateWeeklyFrequency(this DiscordScheduledEventRecurrenceRule rule)
		=> rule.ByWeekday?.Count is not 1
			? (false, "Weekly events must have a single day set in by_weekday.")
			: rule.Interval is not 1 and not 2
				? (false, "Weekly events can only have an interval of 1 or 2.")
				: (true, null);

	/// <summary>
	///     Validates recurrence rules with a monthly frequency.
	/// </summary>
	/// <param name="rule">The recurrence rule to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	private static (bool IsValid, string? ErrorMessage) ValidateMonthlyFrequency(this DiscordScheduledEventRecurrenceRule rule)
		=> rule.ByNWeekday?.Count is not 1
			? ((bool IsValid, string? ErrorMessage))(false, "Monthly events must have a single day set in by_n_weekday.")
			: (true, null);

	/// <summary>
	///     Validates recurrence rules with a yearly frequency.
	/// </summary>
	/// <param name="rule">The recurrence rule to validate.</param>
	/// <returns>A tuple containing a boolean indicating validity and an optional error message.</returns>
	private static (bool IsValid, string? ErrorMessage) ValidateYearlyFrequency(this DiscordScheduledEventRecurrenceRule rule)
		=> rule.ByMonth?.Count is not 1 || rule.ByMonthDay?.Count is not 1
			? ((bool IsValid, string? ErrorMessage))(false, "Yearly events must have both by_month and by_month_day set to a single value.")
			: (true, null);

	/// <summary>
	///     Validates if the weekday set is valid for a daily frequency.
	/// </summary>
	/// <param name="byWeekday">The weekday set to validate.</param>
	/// <returns><see langword="true" /> if the set is valid; otherwise, <see langword="false" />.</returns>
	private static bool IsValidDailyWeekdaySet(this IReadOnlyCollection<RecurrenceRuleWeekday> byWeekday)
		=> s_validDailyWeekdaySets.Any(set => set.SequenceEqual(byWeekday));
}
