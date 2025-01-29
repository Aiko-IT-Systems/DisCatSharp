using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a separator component.
/// </summary>
public sealed class DiscordSeparatorComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordSeparatorComponent" />.
	/// </summary>
	internal DiscordSeparatorComponent()
	{
		this.Type = ComponentType.Separator;
	}

	/// <summary>
	///     Constructs a new separator component based on another separator component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordSeparatorComponent(DiscordSeparatorComponent other)
		: this()
	{
		this.Divider = other.Divider;
		this.Spacing = other.Spacing;
	}

	/// <summary>
	///     Constructs a new separator component field with the specified options.
	/// </summary>
	/// <param name="divider">Whether this is a divider.</param>
	/// <param name="spacing">The spacing size.</param>
	/// <exception cref="ArgumentException">Is thrown when no label is set.</exception>
	public DiscordSeparatorComponent(bool? divider = null, SeparatorSpacingSize? spacing = null)
		: this()
	{
		this.Divider = divider;
		this.Spacing = spacing;
	}

	/// <summary>
	///     The spacing size.
	/// </summary>
	[JsonProperty("spacing", NullValueHandling = NullValueHandling.Ignore)]
	public SeparatorSpacingSize? Spacing { get; set; }

	/// <summary>
	///     Whether this is a divider.
	/// </summary>
	[JsonProperty("divider", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Divider { get; internal set; }
}
