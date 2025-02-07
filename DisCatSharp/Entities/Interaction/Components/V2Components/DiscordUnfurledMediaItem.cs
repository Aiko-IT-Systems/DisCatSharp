namespace DisCatSharp.Entities;

/// <summary>
///     Represents a unfurled media item.
/// </summary>
public sealed class DiscordUnfurledMediaItem : DiscordUnfurledMedia
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordUnfurledMediaItem" />.
	/// </summary>
	internal DiscordUnfurledMediaItem()
	{ }

	/// <summary>
	///     Constructs a new <see cref="DiscordUnfurledMediaItem" />.
	/// </summary>
	/// <param name="url">The unfurled media item url.</param>
	internal DiscordUnfurledMediaItem(string url)
	{
		this.Url = new(url);
	}
}
