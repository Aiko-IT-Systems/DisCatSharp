using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a thumbnail component.
/// </summary>
public sealed class DiscordThumbnailComponent : DiscordSectionAccessory
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordThumbnailComponent" />.
	/// </summary>
	internal DiscordThumbnailComponent()
	{
		this.Type = ComponentType.Thumbnail;
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordThumbnailComponent" />.
	/// </summary>
	/// <param name="url">The thumbnail url.</param>
	/// <param name="description">The description of the thumbnail.</param>
	/// <param name="spoiler">Whether this thumbnail should be marked as spoiler.</param>
	internal DiscordThumbnailComponent(string url, string? description = null, bool? spoiler = null)
		: this()
	{
		this.Media = new(url);
		this.Description = description;
		this.Spoiler = spoiler;
	}

	/// <summary>
	///     Gets the media item.
	/// </summary>
	[JsonProperty("media")]
	public DiscordUnfurledMediaItem Media { get; internal set; }

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

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordThumbnailComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
