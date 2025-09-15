using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a text display component.
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
	/// <param name="other">The text display component to copy.</param>
	public DiscordTextDisplayComponent(DiscordTextDisplayComponent other)
		: this()
	{
		this.Content = other.Content;
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new text display component field with the specified options.
	/// </summary>
	/// <param name="content">The content for the text display.</param>
	public DiscordTextDisplayComponent(string content)
		: this()
	{
		this.Content = content;
	}

	/// <summary>
	///     The content for the text display.
	/// </summary>
	[JsonProperty("content")]
	public string Content { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordTextDisplayComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
