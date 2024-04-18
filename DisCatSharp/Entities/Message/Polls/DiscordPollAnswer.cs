using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an poll answer for a <see cref="DiscordPoll"/>.
/// </summary>
public sealed class DiscordPollAnswer
{
	/// <summary>
	/// Gets the id of the answer.
	/// <para>Only sent as part of responses from Discord's API/Gateway.</para>
	/// </summary>
	[JsonProperty("answer_id")]
	public int? AnswerId { get; internal set; }

	/// <summary>
	/// Gets the poll media.
	/// </summary>
	[JsonProperty("poll_media")]
	public DiscordPollMedia PollMedia { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordPollAnswer"/> class.
	/// </summary>
	public DiscordPollAnswer(string text, PartialEmoji? partialEmoji = null)
	{
		this.PollMedia = new(text, partialEmoji);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordPollAnswer"/> class.
	/// </summary>
	internal DiscordPollAnswer()
	{ }
}
