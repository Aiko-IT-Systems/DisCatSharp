using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a seperator component that can be submitted. Fires
///     <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated" /> event when submitted.
/// </summary>
public sealed class DiscordSeperatorComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordSeperatorComponent" />.
	/// </summary>
	internal DiscordSeperatorComponent()
	{
		this.Type = ComponentType.Seperator;
	}

	/// <summary>
	///     Constructs a new seperator component based on another seperator component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordSeperatorComponent(DiscordSeperatorComponent other)
		: this()
	{
		this.Divider = other.Divider;
		this.Spacing = other.Spacing;
	}

	/// <summary>
	///     Constructs a new seperator component field with the specified options.
	/// </summary>
	/// <param name="divider">Whether this is a divider.</param>
	/// <param name="spacing">The spacing size.</param>
	/// <exception cref="ArgumentException">Is thrown when no label is set.</exception>
	public DiscordSeperatorComponent(bool? divider = null, SeperatorSpacingSize? spacing = null)
	{
		this.Divider = divider;
		this.Spacing = spacing;
		this.Type = ComponentType.Seperator;
	}

	/// <summary>
	///     The spacing size.
	/// </summary>
	[JsonProperty("spacing", NullValueHandling = NullValueHandling.Ignore)]
	public SeperatorSpacingSize? Spacing { get; set; }

	/// <summary>
	///     Whether this is a divider.
	/// </summary>
	[JsonProperty("divider", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Divider { get; internal set; }
}
