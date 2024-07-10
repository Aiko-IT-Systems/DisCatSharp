using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an embed provider.
/// </summary>
public sealed class DiscordEmbedProvider : ObservableApiObject
{
	/// <summary>
	/// Gets the name of the provider.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the url of the provider.
	/// </summary>
	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public Uri Url { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEmbedProvider"/> class.
	/// </summary>
	internal DiscordEmbedProvider()
	{ }
}
