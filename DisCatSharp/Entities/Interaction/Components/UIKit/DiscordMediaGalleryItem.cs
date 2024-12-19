using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a media gallery item.
/// </summary>
public sealed class DiscordMediaGalleryItem
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordMediaGalleryItem" />.
	/// </summary>
	internal DiscordMediaGalleryItem()
	{ }

	/// <summary>
	///     Constructs a new <see cref="DiscordMediaGalleryItem" />.
	/// </summary>
	/// <param name="url">The url.</param>
	/// <param name="description">The description.</param>
	/// <param name="spoiler">Whether this item should be marked as spoiler.</param>
	public DiscordMediaGalleryItem(string url, string? description, bool? spoiler)
	{
		this.Media = new(url);
		this.Description = description;
		this.Spoiler = spoiler;
	}

	/// <summary>
	///     Gets the media item.
	/// </summary>
	[JsonProperty("media")]
	public DiscordMediaItem Media { get; internal set; }

	/// <summary>
	///     Gets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Gets whether this gallery item should be marked as spoiler.
	/// </summary>
	[JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Spoiler { get; internal set; }
}
