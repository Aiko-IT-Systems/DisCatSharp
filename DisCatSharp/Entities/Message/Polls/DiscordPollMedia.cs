using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an poll media object for a <see cref="DiscordPoll" />.
/// </summary>
public sealed class DiscordPollMedia
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPollMedia" /> class.
	/// </summary>
	/// <param name="text">The text of the field.</param>
	/// <param name="partialEmoji">The emoji of the field.</param>
	internal DiscordPollMedia(string text, PartialEmoji? partialEmoji = null)
	{
		this.Text = text;
		this.Emoji = partialEmoji;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordPollMedia" /> class.
	/// </summary>
	internal DiscordPollMedia()
	{ }

	/// <summary>
	///     Gets the text.
	/// </summary>
	[JsonProperty("text")]
	public string Text { get; internal set; }

	/// <summary>
	///     Gets the partial emoji. If you need the full emoji, get it from <see cref="DiscordEmoji.FromName" /> or
	///     <see cref="DiscordEmoji.FromGuildEmote" />.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public PartialEmoji? Emoji { get; internal set; }
}
