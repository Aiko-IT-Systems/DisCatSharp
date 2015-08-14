using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// The rest guild scheduled event create payload.
/// </summary>
internal sealed class RestGuildScheduledEventCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the entity metadata.
	/// </summary>
	[JsonProperty("entity_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordScheduledEventEntityMetadata EntityMetadata { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	[JsonProperty("scheduled_start_time")]
	public DateTimeOffset ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	[JsonProperty("scheduled_end_time", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	[JsonProperty("entity_type")]
	public ScheduledEventEntityType EntityType { get; set; }

	/// <summary>
	/// Gets or sets the image as base64.
	/// </summary>
	[JsonProperty("image", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> CoverBase64 { get; set; }

	/// <summary>
	/// Gets or sets the privacy level of the scheduled event.
	/// </summary>
	[JsonProperty("privacy_level")]
	public int PrivacyLevel { get; } = 2;
}

/// <summary>
/// The rest guild scheduled event modify payload.
/// </summary>
internal sealed class RestGuildScheduledEventModifyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public Optional<ulong?> ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the entity metadata.
	/// </summary>
	[JsonProperty("entity_metadata")]
	public Optional<DiscordScheduledEventEntityMetadata> EntityMetadata { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[JsonProperty("description")]
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	[JsonProperty("scheduled_start_time")]
	public Optional<DateTimeOffset> ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	[JsonProperty("scheduled_end_time")]
	public Optional<DateTimeOffset> ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	[JsonProperty("entity_type")]
	public Optional<ScheduledEventEntityType> EntityType { get; set; }

	/// <summary>
	/// Gets or sets the cover image as base64.
	/// </summary>
	[JsonProperty("image")]
	public Optional<string> CoverBase64 { get; set; }

	/// <summary>
	/// Gets or sets the status of the scheduled event.
	/// </summary>
	[JsonProperty("status")]
	public Optional<ScheduledEventStatus> Status { get; set; }

	/// <summary>
	/// Gets or sets the privacy level of the scheduled event.
	/// </summary>
	[JsonProperty("privacy_level")]
	public int PrivacyLevel { get; } = 2;
}
