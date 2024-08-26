using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an poll answer count object for a <see cref="DiscordPoll" />.
/// </summary>
public sealed class DiscordPollAnswerCount
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPollAnswerCount" /> class.
	/// </summary>
	internal DiscordPollAnswerCount()
	{ }

	/// <summary>
	///     Gets the answer id.
	/// </summary>
	[JsonProperty("id")]
	public int Id { get; internal set; }

	/// <summary>
	///     Gets the number of votes for this answer.
	/// </summary>
	[JsonProperty("count")]
	public int Count { get; internal set; }

	/// <summary>
	///     Gets whether the current user voted for this answer.
	/// </summary>
	[JsonProperty("me_voted")]
	public bool MeVoted { get; internal set; }
}
