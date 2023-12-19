using System;
using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a scheduled event edit model.
/// </summary>
public class ScheduledEventEditModel : BaseEditModel
{
	/// <summary>
	/// Gets or sets the channel.
	/// </summary>
	public Optional<DiscordChannel> Channel { get; set; }

	/// <summary>
	/// Gets or sets the location.
	/// </summary>
	public Optional<string> Location { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	public Optional<ScheduledEventEntityType> EntityType { get; set; }

	/// <summary>
	/// Gets or sets the cover image as base64.
	/// </summary>
	public Optional<Stream> CoverImage { get; set; }

	/// <summary>
	/// Gets or sets the status of the scheduled event.
	/// </summary>
	public Optional<ScheduledEventStatus> Status { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScheduledEventEditModel"/> class.
	/// </summary>
	internal ScheduledEventEditModel()
	{ }
}
