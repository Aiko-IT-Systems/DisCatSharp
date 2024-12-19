using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a file display component.
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
	/// <param name="other">The file display component to copy.</param>
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
		: this()
	{
		this.File = new(url);
		this.Spoiler = spoiler;
	}

	/// <summary>
	///     Gets the file.
	/// </summary>
	[JsonProperty("media")]
	public DiscordUnfurledMediaItem File { get; internal set; }

	/// <summary>
	///     Gets whether this file should be marked as spoiler.
	/// </summary>
	[JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Spoiler { get; internal set; }
}
