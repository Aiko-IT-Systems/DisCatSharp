using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a section component.
/// </summary>
public sealed class DiscordSectionComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordSectionComponent" />.
	/// </summary>
	internal DiscordSectionComponent()
	{
		this.Type = ComponentType.Section;
	}

	/// <summary>
	///     Constructs a new section component based on another section component.
	/// </summary>
	/// <param name="other">The section component to copy.</param>
	public DiscordSectionComponent(DiscordSectionComponent other)
		: this()
	{
		this.Components = other.Components;
		this.Accessory = other.Accessory;
	}

	/// <summary>
	///     Constructs a new section component field with the specified options.
	/// </summary>
	/// <param name="components">The section components.</param>
	public DiscordSectionComponent(IEnumerable<DiscordTextDisplayComponent> components)
		: this()
	{
		this.Components = components.ToList();
	}

	/// <summary>
	///     The content for the section.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordTextDisplayComponent> Components { get; internal set; } = [];

	/// <summary>
	///     The accessory for the section.
	///     Can be <see cref="DiscordThumnailComponent" /> at the moment, but might include buttons later.
	/// </summary>
	[JsonProperty("accessory", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordSectionAccessory Accessory { get; internal set; }

	/// <summary>
	///     Adds a thumbnail component to the section.
	/// </summary>
	/// <param name="url">The thumbnail url.</param>
	/// <param name="description">The description of the thumbnail.</param>
	/// <param name="spoiler">Whether this thumbnail should be marked as spoiler.</param
	/// <returns>The current <see cref="DiscordSectionComponent" />.</returns>
	public DiscordSectionComponent WithThumbnailComponent(string url, string? description = null, bool? spoiler = null)
	{
		this.Accessory = new DiscordThumnailComponent(url, description, spoiler);
		return this;
	}
}
