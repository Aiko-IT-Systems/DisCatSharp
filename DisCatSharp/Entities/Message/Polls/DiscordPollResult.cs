using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an poll result object for a <see cref="DiscordPoll"/>.
/// </summary>
public sealed class DiscordPollResult : ObservableApiObject
{
	/// <summary>
	/// Gets the answer counts.
	/// </summary>
	[JsonProperty("answer_counts")]
	public List<DiscordPollAnswerCount> AnswerCounts { get; internal set; } = [];

	/// <summary>
	/// Gets whether this poll is finalized (closed).
	/// </summary>
	[JsonProperty("is_finalized")]
	public bool IsFinalized { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordPollResult"/> class.
	/// </summary>
	internal DiscordPollResult()
	{ }
}
