using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a thumbnail component.
/// </summary>
public sealed class DiscordThumnailComponent : DiscordSectionAccessory
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordThumnailComponent" />.
	/// </summary>
	internal DiscordThumnailComponent()
	{
		this.Type = ComponentType.Thumbnail;
	}

	/// <summary>
	///     Constructs a new <see cref="DiscordThumnailComponent" />.
	/// </summary>
	/// <param name="url">The thumbnail url.</param>
	/// <param name="description">The description of the thumbnail.</param>
	/// <param name="spoiler">Whether this thumbnail should be marked as spoiler.</param>
	internal DiscordThumnailComponent(string url, string? description = null, bool? spoiler = null)
		: this()
	{
		this.Image = new(url);
		this.Description = description;
		this.Spoiler = spoiler;
	}

	/// <summary>
	///     Gets the media item.
	/// </summary>
	[JsonProperty("image")]
	public DiscordUnfurledMediaItem Image { get; internal set; }

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
