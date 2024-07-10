using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a reaction to a message.
/// </summary>
public class DiscordReaction : ObservableApiObject
{
	/// <summary>
	/// Gets the total number of users who reacted with this emoji.
	/// </summary>
	[JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
	public int Count { get; internal set; }

	/// <summary>
	/// Gets the total number of users who burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_count", NullValueHandling = NullValueHandling.Ignore)]
	public int BurstCount { get; internal set; }

	/// <summary>
	/// Gets whether the current user burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_me", NullValueHandling = NullValueHandling.Ignore)]
	public bool BurstMe { get; internal set; }

	/// <summary>
	/// Gets the ids of users who burst reacted with this emoji.
	/// </summary>
	[JsonProperty("burst_user_ids", NullValueHandling = NullValueHandling.Ignore)]
	public ulong[] BurstUserIds { get; internal set; }

	/// <summary>
	/// Gets the burst colors.
	/// </summary>
	[JsonProperty("burst_colors", NullValueHandling = NullValueHandling.Ignore)]
	public string[] BurstColors { get; internal set; }

	/// <summary>
	/// Gets whether the current user reacted with this emoji.
	/// </summary>
	[JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMe { get; internal set; }

	/// <summary>
	/// Gets the emoji used to react to this message.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji Emoji { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordReaction"/> class.
	/// </summary>
	internal DiscordReaction()
	{ }
}
