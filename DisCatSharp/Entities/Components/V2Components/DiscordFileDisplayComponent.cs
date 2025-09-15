using System;

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
		this.Id = other.Id;
	}

	/// <summary>
	///     Constructs a new file display component field with the specified options.
	/// </summary>
	/// <param name="url">The file url. Only supports attachments (<c>attachment://</c>).</param>
	/// <param name="spoiler">Whether this file should be marked as spoiler.</param>
	public DiscordFileDisplayComponent(string url, bool? spoiler)
		: this()
	{
		ArgumentException.ThrowIfNullOrEmpty(url);
		if (!url.StartsWith("attachment://", StringComparison.Ordinal))
			throw new ArgumentException("File URL must start with 'attachment://'.", nameof(url));

		this.File = new(url);
		this.Spoiler = spoiler;
	}

	/// <summary>
	///     Gets the file.
	/// </summary>
	[JsonProperty("file")]
	public DiscordUnfurledMediaItem File { get; internal set; }

	/// <summary>
	///     Gets the file size.
	/// </summary>
	[JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
	public int Size { get; internal set; }

	/// <summary>
	///     Gets the file name.
	/// </summary>
	[JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets whether this file should be marked as spoiler.
	/// </summary>
	[JsonProperty("spoiler", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Spoiler { get; internal set; }

	/// <summary>
	///     Assigns a unique id to this component.
	/// </summary>
	/// <param name="id">The id to assign.</param>
	public DiscordFileDisplayComponent WithId(int id)
	{
		this.Id = id;
		return this;
	}
}
