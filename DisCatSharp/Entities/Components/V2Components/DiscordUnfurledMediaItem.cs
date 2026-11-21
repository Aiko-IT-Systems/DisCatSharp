using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a unfurled media item.
/// </summary>
public sealed class DiscordUnfurledMediaItem : DiscordUnfurledMedia
{
	/// <summary>
	///     Gets Discord's scan result, which may appear in a later message update.
	/// </summary>
	[JsonProperty("content_scan_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordContentScanMetadata? ContentScanMetadata { get; internal set; }

	/// <summary>
	///     Constructs a new empty <see cref="DiscordUnfurledMediaItem" />.
	/// </summary>
	[JsonConstructor]
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
