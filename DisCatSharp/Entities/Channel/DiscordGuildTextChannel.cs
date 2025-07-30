using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a base class for all guild text-based channels.
/// </summary>
public abstract class DiscordGuildTextChannel : DiscordGuildChannel
{
	/// <summary>
	/// Gets the channel topic.
	/// </summary>
	[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
	public string Topic { get; internal set; }

	/// <summary>
	/// Gets the last message id in the channel.
	/// </summary>
	[JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LastMessageId { get; internal set; }

	/// <summary>
	/// Gets the rate limit per user (slowmode).
	/// </summary>
	[JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
	public int? RateLimitPerUser { get; internal set; }

	/// <summary>
	/// Gets the nsfw status.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsNsfw { get; internal set; }
}
