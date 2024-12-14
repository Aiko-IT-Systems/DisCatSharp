using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a media gallery component that can be submitted. Fires
///     <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated" /> event when submitted.
/// </summary>
public sealed class DiscordMediaGalleryComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordMediaGalleryComponent" />.
	/// </summary>
	internal DiscordMediaGalleryComponent()
	{
		this.Type = ComponentType.MediaGallery;
	}

	/// <summary>
	///     Constructs a new media gallery component based on another media gallery component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordMediaGalleryComponent(DiscordMediaGalleryComponent other)
		: this()
	{
		this.Items = other.Items;
	}

	/// <summary>
	///     Constructs a new media gallery component field with the specified options.
	/// </summary>
	/// <param name="items">The media gallery items.</param>
	/// <exception cref="ArgumentException">Is thrown when no label is set.</exception>
	public DiscordMediaGalleryComponent(IEnumerable<DiscordMediaGalleryItem> items)
	{
		this.Items = items.ToList();
		this.Type = ComponentType.MediaGallery;
	}

	/// <summary>
	///     The content for the media gallery.
	/// </summary>
	[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordMediaGalleryItem> Items { get; internal set; } = [];
}
