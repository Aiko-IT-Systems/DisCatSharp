using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a footer in an embed.
/// </summary>
public sealed class DiscordEmbedFooter : ObservableApiObject
{
	/// <summary>
	/// Gets the footer's text.
	/// </summary>
	[JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
	public string Text { get; internal set; }

	/// <summary>
	/// Gets the url of the footer's icon.
	/// </summary>
	[JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri IconUrl { get; internal set; }

	/// <summary>
	/// Gets the proxied url of the footer's icon.
	/// </summary>
	[JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri ProxyIconUrl { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEmbedFooter"/> class.
	/// </summary>
	internal DiscordEmbedFooter()
	{ }
}
