using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a poll request for a message.
/// </summary>
internal sealed class DiscordPollRequest
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPollRequest" /> class.
	/// </summary>
	internal DiscordPollRequest(DiscordPollMedia question, List<DiscordPollAnswer> answers, int duration, PollLayoutType layoutType, bool allowMultiselect)
	{
		this.Question = question;
		this.Answers = answers;
		this.Duration = duration;
		this.LayoutType = layoutType;
		this.AllowMultiselect = allowMultiselect;
	}

	/// <summary>
	///     Sets the poll question.
	/// </summary>
	[JsonProperty("question")]
	internal DiscordPollMedia Question { get; set; }

	/// <summary>
	///     Sets the poll answers to choose from.
	/// </summary>
	[JsonProperty("answers")]
	internal List<DiscordPollAnswer> Answers { get; set; }

	/// <summary>
	///     Sets the poll duration in hours.
	/// </summary>
	[JsonProperty("duration")]
	internal int Duration { get; set; }

	/// <summary>
	///     Sets the poll layout type.
	/// </summary>
	[JsonProperty("layout_type")]
	internal PollLayoutType LayoutType { get; set; }

	/// <summary>
	///     Sets whether the poll allows multiselect.
	/// </summary>
	[JsonProperty("allow_multiselect")]
	internal bool AllowMultiselect { get; set; }
}
