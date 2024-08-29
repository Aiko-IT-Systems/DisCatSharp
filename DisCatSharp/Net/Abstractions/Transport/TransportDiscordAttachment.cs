using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAttachment : SnowflakeObject
{
	[JsonProperty("filename")]
	public string Filename { get; internal set; }

	[JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? ContentType { get; internal set; }

	[JsonProperty("size")]
	public int Size { get; internal set; }

	[JsonProperty("url")]
	public string Url { get; internal set; }

	[JsonProperty("proxy_url")]
	public string ProxyUrl { get; internal set; }

	[JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
	public int? Height { get; internal set; }

	[JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
	public int? Width { get; internal set; }

	[JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Ephemeral { get; internal set; }

	[JsonProperty("duration_secs", NullValueHandling = NullValueHandling.Ignore)]
	public float? DurationSecs { get; internal set; }

	[JsonProperty("waveform", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(WaveformConverter))]
	public byte[]? Waveform { get; internal set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public int? Flags { get; internal set; }

	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string? Title { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }
}
