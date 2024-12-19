using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a text display component that can be submitted. Fires
///     <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated" /> event when submitted.
/// </summary>
public sealed class DiscordTextDisplayComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordTextDisplayComponent" />.
	/// </summary>
	internal DiscordTextDisplayComponent()
	{
		this.Type = ComponentType.TextDisplay;
	}

	/// <summary>
	///     Constructs a new text display component based on another text display component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordTextDisplayComponent(DiscordTextDisplayComponent other)
		: this()
	{
		this.Content = other.Content;
	}

	/// <summary>
	///     Constructs a new text display component field with the specified options.
	/// </summary>
	/// <param name="content">The content for the text display.</param>
	public DiscordTextDisplayComponent(string content)
	{
		this.Content = content;
		this.Type = ComponentType.TextDisplay;
	}

	/// <summary>
	///     The content for the text display.
	/// </summary>
	[JsonProperty("content")]
	public string Content { get; internal set; }
}
