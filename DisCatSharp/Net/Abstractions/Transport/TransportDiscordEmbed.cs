using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordEmbed
{
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string? Title { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string? Type { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
	public string? Url { get; internal set; }

	[JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? Timestamp { get; internal set; }

	[JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
	public int? Color { get; internal set; }

	[JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedFooter? Footer { get; internal set; }

	[JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedImage? Image { get; internal set; }

	[JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedThumbnail? Thumbnail { get; internal set; }

	[JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedVideo? Video { get; internal set; }

	[JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedProvider? Provider { get; internal set; }

	[JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordEmbedAuthor? Author { get; internal set; }

	[JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordEmbedField>? Fields { get; internal set; }
}
