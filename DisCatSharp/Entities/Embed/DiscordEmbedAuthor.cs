using System;

using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Gets the author of a discord embed.
/// </summary>
public sealed class DiscordEmbedAuthor : ObservableApiObject
{
	/// <summary>
	/// Gets the name of the author.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	/// Gets the url of the author.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri Url { get; set; }

	/// <summary>
	/// Gets the url of the author's icon.
	/// </summary>
	[JsonProperty("icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri IconUrl { get; set; }

	/// <summary>
	/// Gets the proxied url of the author's icon.
	/// </summary>
	[JsonProperty("proxy_icon_url", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUri ProxyIconUrl { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEmbedAuthor"/> class.
	/// </summary>
	internal DiscordEmbedAuthor()
	{ }
}
