namespace DisCatSharp.Enums;

/// <summary>
/// Represents the auto-archive duration for a thread.
/// </summary>
public enum ThreadAutoArchiveDuration
{
	/// <summary>
	/// Indicates that the thread will be auto archived after one hour.
	/// </summary>
	OneHour = 60,

	/// <summary>
	/// Indicates that the thread will be auto archived after one day / 24 hours.
	/// </summary>
	OneDay = 1440,

	/// <summary>
	/// Indicates that the thread will be auto archived after three days.
	/// </summary>
	ThreeDays = 4320,

	/// <summary>
	/// Indicates that the thread will be auto archived after a week.
	/// </summary>
	OneWeek = 10080
}
