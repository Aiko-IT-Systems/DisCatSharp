using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a media item.
/// </summary>
public sealed class DiscordUnfurledMediaItem
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordUnfurledMediaItem" />.
	/// </summary>
	internal DiscordUnfurledMediaItem()
	{ }

	/// <summary>
	///     Constructs a new <see cref="DiscordUnfurledMediaItem" />.
	/// </summary>
	/// <param name="url">The items url.</param>
	internal DiscordUnfurledMediaItem(string url)
	{
		this.Url = url;
	}

	/// <summary>
	///     Gets the url.
	/// </summary>
	[JsonProperty("url")]
	public string Url { get; internal set; }
}
