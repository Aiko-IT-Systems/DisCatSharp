using Newtonsoft.Json;

namespace DisCatSharp.Entities;

public class ForumReactionEmoji : ObservableApiObject
{
	/// <summary>
	///     Creates a new forum reaction emoji.
	///     Use either <paramref name="emojiId" /> or <paramref name="unicodeEmojiString" />. Not both.
	/// </summary>
	/// <param name="emojiId">The emoji id. Has to be from the same server.</param>
	/// <param name="unicodeEmojiString">The unicode emoji.</param>
	public ForumReactionEmoji(ulong? emojiId = null, string unicodeEmojiString = null)
	{
		this.EmojiId = emojiId;
		this.EmojiName = unicodeEmojiString;
	}

	/// <summary>
	///     Gets the emoji id of the forum post tag.
	/// </summary>
	// NullValueHandling.Include is intentional: null emoji_id signals a unicode emoji; Discord needs the explicit null
	// to distinguish "unicode (no id)" from "field absent". Mutually exclusive with emoji_name.
	[JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Include)]
	public ulong? EmojiId { get; internal set; }

	/// <summary>
	///     Gets the unicode emoji of the forum post tag.
	/// </summary>
	// NullValueHandling.Include is intentional: null emoji_name signals a custom guild emoji; Discord needs the explicit null
	// to distinguish "custom (no name)" from "field absent". Mutually exclusive with emoji_id.
	[JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Include)]
	public string EmojiName { get; internal set; }

	/// <summary>
	///     Gets the emoji.
	/// </summary>
	public DiscordEmoji GetEmoji(DiscordClient client)
		=> this.EmojiName != null ? DiscordEmoji.FromName(client, $":{this.EmojiName}:", false) : DiscordEmoji.FromGuildEmote(client, this.EmojiId.Value);
}
