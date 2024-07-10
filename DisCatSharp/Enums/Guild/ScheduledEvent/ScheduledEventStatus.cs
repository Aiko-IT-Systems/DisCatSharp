namespace DisCatSharp.Enums;

/// <summary>
/// Represents the status for a scheduled event.
/// </summary>
public enum ScheduledEventStatus
{
	/// <summary>
	/// Indicates that the event is scheduled.
	/// </summary>
	Scheduled = 1,

	/// <summary>
	/// Indicates that the event is active.
	/// </summary>
	Active = 2,

	/// <summary>
	/// Indicates that the event is completed.
	/// </summary>
	Completed = 3,

	/// <summary>
	/// Indicates that the event is canceled.
	/// </summary>
	Canceled = 4
}
