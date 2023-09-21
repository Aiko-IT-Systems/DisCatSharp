using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a stage instance create payload.
/// </summary>
internal sealed class RestStageInstanceCreatePayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; set; }

	/// <summary>
	/// Gets or sets the associated scheduled event id.
	/// </summary>
	[JsonProperty("guild_scheduled_event_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ScheduledEventId { get; set; }

	/// <summary>
	/// Whether everyone should be notified about the start.
	/// </summary>
	[JsonProperty("send_start_notification", NullValueHandling = NullValueHandling.Ignore)]
	public bool SendStartNotification { get; set; }
}

/// <summary>
/// Represents a stage instance modify payload.
/// </summary>
internal sealed class RestStageInstanceModifyPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the topic.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Topic { get; set; }
}
