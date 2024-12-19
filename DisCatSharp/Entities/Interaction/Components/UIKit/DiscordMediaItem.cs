using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a media item.
/// </summary>
public sealed class DiscordMediaItem
{
	/// <summary>
	/// Constructs a new empty <see cref="DiscordMediaItem"/>.
	/// </summary>
	internal DiscordMediaItem()
	{ }

	/// <summary>
	/// Constructs a new <see cref="DiscordMediaItem"/>.
	/// </summary>
	/// <param name="url">The items url.</param>
	internal DiscordMediaItem(string url)
	{
		this.Url = url;
	}

	/// <summary>
	/// Gets the url.
	/// </summary>
	[JsonProperty("url")]
	public string Url { get; internal set; }
}
