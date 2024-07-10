using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Additional data used when an action is executed.
/// Different fields are relevant based on the action type.
/// </summary>
public class AutomodActionMetadata : ObservableApiObject
{
	/// <summary>
	/// The channel to which user content should be logged.
	/// Only works with SendAlertMessage.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; internal set; }

	/// <summary>
	/// The timeout duration in seconds.
	/// Maximum of 2419200 seconds (4 weeks).
	/// </summary>
	[JsonProperty("duration_seconds", NullValueHandling = NullValueHandling.Ignore)]
	public int? Duration { get; internal set; }

	[JsonProperty("custom_message", NullValueHandling = NullValueHandling.Ignore)]
	public string? CustomMessage { get; set; }
}
