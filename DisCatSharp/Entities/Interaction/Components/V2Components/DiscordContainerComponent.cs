using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a container component.
/// </summary>
public sealed class DiscordContainerComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordContainerComponent" />.
	/// </summary>
	internal DiscordContainerComponent()
	{
		this.Type = ComponentType.Container;
	}

	/// <summary>
	///     Constructs a new container component based on another container component.
	/// </summary>
	/// <param name="other">The container component to copy.</param>
	public DiscordContainerComponent(DiscordContainerComponent other)
		: this()
	{
		this.Components = other.Components;
		this.AccentColor = other.AccentColor;
		this.Spoiler = other.Spoiler;
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new container component field with the specified options.
	/// </summary>
	/// <param name="components">The container components. Max of <c>10</c>.</param>
	/// <param name="spoiler">Whether the container should be marked as spoiler.</param>
	/// <param name="accentColor">The accent color for the container.</param>
	public DiscordContainerComponent(IEnumerable<DiscordComponent> components, bool? spoiler = null, DiscordColor? accentColor = null)
		: this()
	{
		var comps = components.ToList();
		if (comps.Count > 10)
			throw new ArgumentException("You can only have up to 10 components in a container.");

		List<ComponentType> allowedTypes = [ComponentType.ActionRow, ComponentType.TextDisplay, ComponentType.Section, ComponentType.MediaGallery, ComponentType.Separator, ComponentType.File];
		if (comps.Any(c => !allowedTypes.Contains(c.Type)))
			throw new ArgumentException("All components must be of type ActionRow, TextDisplay, Section, MediaGallery, Separator, or File.");

		this.Components = [.. comps];
		this.Spoiler = spoiler;
		this.AccentColor = accentColor;
	}

	/// <summary>
	///     Constructs a new container component field with the specified options.
	/// </summary>
	/// <param name="spoiler">Whether the container should be marked as spoiler.</param>
	/// <param name="accentColor">The accent color for the container.</param>
	public DiscordContainerComponent(bool? spoiler = null, DiscordColor? accentColor = null)
		: this()
	{
		this.Spoiler = spoiler;
		this.AccentColor = accentColor;
	}

	/// <summary>
	///     The components for the container.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordComponent> Components { get; internal set; } = [];

	/// <summary>
	///     The accent color for the container.
	/// </summary>
	[JsonIgnore]
	public DiscordColor? AccentColor { get; internal set; }

	/// <summary>
	///     Gets the accent color int for the container.
	/// </summary>
	[JsonProperty("accent_color", NullValueHandling = NullValueHandling.Ignore)]
	internal int? AccentColorInt
		=> this.AccentColor?.Value;

	/// <summary>
	///     Gets whether this container should be marked as spoiler.
	/// </summary>
	[JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Spoiler { get; internal set; }

	/// <summary>
	///     Adds multiple components to the container.
	/// </summary>
	/// <param name="components">The components to add.</param>
	/// <returns>The updated container component.</returns>
	/// <exception cref="ArgumentException">
	///     Thrown if the number of components exceeds 10 or if any of the components are not
	///     of the allowed types.
	/// </exception>
	public DiscordContainerComponent AddComponents(IEnumerable<DiscordComponent> components)
	{
		var comps = components.ToList();
		if (this.Components.Count + comps.Count > 10)
			throw new ArgumentException("You can only have up to 10 components in a container.");

		List<ComponentType> allowedTypes = [ComponentType.ActionRow, ComponentType.TextDisplay, ComponentType.Section, ComponentType.MediaGallery, ComponentType.Separator, ComponentType.File];
		if (comps.Any(c => !allowedTypes.Contains(c.Type)))
			throw new ArgumentException("All components must be of type ActionRow, TextDisplay, Section, MediaGallery, Separator, or File.");

		this.Components = [.. comps];
		return this;
	}

	/// <summary>
	///     Adds a component to the container.
	/// </summary>
	/// <param name="component">The component to add.</param>
	/// <returns>The updated container component.</returns>
	/// <exception cref="ArgumentException">
	///     Thrown if the number of components exceeds 10 or if any of the components are not
	///     of the allowed types.
	/// </exception>
	public DiscordContainerComponent AddComponent(DiscordComponent component)
	{
		if (this.Components.Count > 10)
			throw new ArgumentException("You can only have up to 10 components in a container.");

		List<ComponentType> allowedTypes = [ComponentType.ActionRow, ComponentType.TextDisplay, ComponentType.Section, ComponentType.MediaGallery, ComponentType.Separator, ComponentType.File];
		if (!allowedTypes.Contains(component.Type))
			throw new ArgumentException("All components must be of type ActionRow, TextDisplay, Section, MediaGallery, Separator, or File.");

		this.Components = [.. this.Components, component];
		return this;
	}

	/// <summary>
	///     Assigns a unique id to the components.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordContainerComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}

	/// <inheritdoc/>
	public override IEnumerable<DiscordComponent> GetChildren()
	{
		foreach (var comp in this.Components)
			yield return comp;
	}
}
