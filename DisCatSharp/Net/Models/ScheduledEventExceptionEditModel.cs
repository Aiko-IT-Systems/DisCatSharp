using System;

using DisCatSharp.Entities;

namespace DisCatSharp.Net.Models;

/// <summary>
///     Represents a creation model for a scheduled event exception.
/// </summary>
public sealed class ScheduledEventExceptionCreateModel : BaseEditModel
{
	/// <summary>
	///     Gets or sets the original scheduled start time for the recurrence being changed.
	/// </summary>
	public DateTimeOffset OriginalScheduledStartTime { get; set; }

	/// <summary>
	///     Gets or sets the new scheduled start time for the exception.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledStartTime { get; set; }

	/// <summary>
	///     Gets or sets the new scheduled end time for the exception.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledEndTime { get; set; }

	/// <summary>
	///     Gets or sets whether the recurrence should be canceled.
	/// </summary>
	public Optional<bool> IsCanceled { get; set; }
}

/// <summary>
///     Represents an edit model for a scheduled event exception.
/// </summary>
public sealed class ScheduledEventExceptionEditModel : BaseEditModel
{
	/// <summary>
	///     Gets or sets the new scheduled start time for the exception.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledStartTime { get; set; }

	/// <summary>
	///     Gets or sets the new scheduled end time for the exception.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledEndTime { get; set; }

	/// <summary>
	///     Gets or sets whether the recurrence should be canceled.
	/// </summary>
	public Optional<bool> IsCanceled { get; set; }
}
