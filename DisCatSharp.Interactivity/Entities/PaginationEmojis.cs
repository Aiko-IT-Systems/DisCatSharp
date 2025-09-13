using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity.Entities;

/// <summary>
///     The pagination emojis.
/// </summary>
public sealed class PaginationEmojis
{
	/// <summary>
	///     Gets or sets the left emoji.
	/// </summary>
	public DiscordEmoji Left { get; init; }

	/// <summary>
	///     Gets or sets the right emoji.
	/// </summary>
	public DiscordEmoji Right { get; init; }

	/// <summary>
	///     Gets or sets the skip left emoji.
	/// </summary>
	public DiscordEmoji SkipLeft { get; init; }

	/// <summary>
	///     Gets or sets the skip right emoji.
	/// </summary>
	public DiscordEmoji SkipRight { get; init; }

	/// <summary>
	///     Gets or sets the stop emoji.
	/// </summary>
	public DiscordEmoji Stop { get; init; }

	/// <summary>
	///     Initializes a new instance of the <see cref="PaginationEmojis" /> class.
	/// </summary>
	public PaginationEmojis()
	{
		this.Left = DiscordEmoji.FromUnicode("◀");
		this.Right = DiscordEmoji.FromUnicode("▶");
		this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
		this.SkipRight = DiscordEmoji.FromUnicode("⏭");
		this.Stop = DiscordEmoji.FromUnicode("⏹");
	}
}
