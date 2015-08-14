using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
/// Represents a followed channel add payload.
/// </summary>
internal sealed class FollowedChannelAddPayload : ObservableApiObject
{
	/// <summary>
	/// Gets or sets the webhook channel id.
	/// </summary>
	[JsonProperty("webhook_channel_id")]
	public ulong WebhookChannelId { get; set; }
}
