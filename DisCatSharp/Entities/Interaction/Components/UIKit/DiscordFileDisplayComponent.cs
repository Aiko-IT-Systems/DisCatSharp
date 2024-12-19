using System;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a file display component that can be submitted. Fires
///     <see cref="DisCatSharp.DiscordClient.ComponentInteractionCreated" /> event when submitted.
/// </summary>
public sealed class DiscordFileDisplayComponent : DiscordComponent
{
	/// <summary>
	///     Constructs a new <see cref="DiscordFileDisplayComponent" />.
	/// </summary>
	internal DiscordFileDisplayComponent()
	{
		this.Type = ComponentType.File;
	}

	/// <summary>
	///     Constructs a new file display component based on another file display component.
	/// </summary>
	/// <param name="other">The button to copy.</param>
	public DiscordFileDisplayComponent(DiscordFileDisplayComponent other)
		: this()
	{
		this.File = other.File;
		this.Spoiler = other.Spoiler;
	}

	/// <summary>
	///     Constructs a new file display component field with the specified options.
	/// </summary>
	/// <param name="url">The file url.</param>
	/// <param name="spoiler">Whether this file should be marked as spoiler.</param>
	public DiscordFileDisplayComponent(string url, bool? spoiler)
	{
		this.File = new(url);
		this.Spoiler = spoiler;
		this.Type = ComponentType.File;
	}

	/// <summary>
	///     Gets the file.
	/// </summary>
	[JsonProperty("media")]
	public DiscordMediaItem File { get; internal set; }

	/// <summary>
	///     Gets whether this file should be marked as spoiler.
	/// </summary>
	[JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Spoiler { get; internal set; }
}
