using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a followed channel.
/// </summary>
public class DiscordFollowedChannel : ObservableApiObject
{
	/// <summary>
	/// Gets the id of the channel following the announcement channel.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the id of the webhook that posts crossposted messages to the channel.
	/// </summary>
	[JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong WebhookId { get; internal set; }
}
