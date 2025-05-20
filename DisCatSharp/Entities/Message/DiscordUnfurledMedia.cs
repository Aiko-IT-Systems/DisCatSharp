using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a unfurled media.
/// </summary>
public class DiscordUnfurledMedia: ObservableApiObject
{
	/// <summary>
	///     Constructs a new empty <see cref="DiscordUnfurledMedia" />.
	/// </summary>
	[JsonConstructor]
	internal DiscordUnfurledMedia()
	{ }

	/// <summary>
	///     Gets the URL of the unfurled media.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri Url { get; internal set; }

	/// <summary>
	///     Gets the proxied URL of the unfurled media.
	/// </summary>
	[JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri? ProxyUrl { get; internal set; }

	/// <summary>
	///     Gets the media, or MIME, type of the unfurled media.
	/// </summary>
	[JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? ContentType { get; internal set; }

	/// <summary>
	///     Gets the height, if applicable.
	/// </summary>
	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int? Height { get; internal set; }

	/// <summary>
	///     Gets the width, if applicable.
	/// </summary>
	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int? Width { get; internal set; }

	/// <summary>
	///     Gets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public UnfurledMediaFlags Flags { get; internal set; } = UnfurledMediaFlags.None;

	/// <summary>
	///     Gets the placeholder.
	/// </summary>
	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string? Placeholder { get; internal set; }

	/// <summary>
	///     Gets the placeholder version.
	/// </summary>
	[JsonProperty("placeholder_version", NullValueHandling = NullValueHandling.Ignore)]
	public int? PlaceholderVersion { get; internal set; }

	/// <summary>
	///     Gets the loading state.
	/// </summary>
	[JsonProperty("loading_state", NullValueHandling = NullValueHandling.Ignore)]
	public LoadingState LoadingState { get; internal set; } = LoadingState.Unknown;
}
