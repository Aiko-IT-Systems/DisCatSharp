using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a media gallery component.
/// </summary>
public sealed class DiscordMediaGalleryComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordMediaGalleryComponent" />.
	/// </summary>
	[JsonConstructor]
	internal DiscordMediaGalleryComponent()
	{
		this.Type = ComponentType.MediaGallery;
	}

	/// <summary>
	///     Constructs a new media gallery component based on another media gallery component.
	/// </summary>
	/// <param name="other">The media gallery component to copy.</param>
	public DiscordMediaGalleryComponent(DiscordMediaGalleryComponent other)
		: this()
	{
		this.Items = other.Items;
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new media gallery component field with the specified options.
	/// </summary>
	/// <param name="items">The media gallery items.</param>
	public DiscordMediaGalleryComponent(IEnumerable<DiscordMediaGalleryItem> items)
		: this()
	{
		var it = items.ToList();
		if (it.Count > 10)
			throw new ArgumentException("You can only have up to 10 items in a media gallery.");

		this.Items = [..it];
	}

	/// <summary>
	///     The content for the media gallery.
	/// </summary>
	[JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordMediaGalleryItem> Items { get; internal set; } = [];

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordMediaGalleryComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
