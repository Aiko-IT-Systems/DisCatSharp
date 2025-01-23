using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents an image in an embed.
/// </summary>
public sealed class DiscordEmbedImage : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEmbedImage" /> class.
	/// </summary>
	internal DiscordEmbedImage()
	{ }

	/// <summary>
	///     Gets the source url of the image.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri Url { get; internal set; }

	/// <summary>
	///     Gets a proxied url of the image.
	/// </summary>
	[JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri ProxyUrl { get; internal set; }

	/// <summary>
	///     Gets the height of the image.
	/// </summary>
	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int Height { get; internal set; }

	/// <summary>
	///     Gets the width of the image.
	/// </summary>
	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int Width { get; internal set; }

	/// <summary>
	///     Gets the flags of the image.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public EmbedMediaFlags Flags { get; internal set; } = EmbedMediaFlags.None;
}
