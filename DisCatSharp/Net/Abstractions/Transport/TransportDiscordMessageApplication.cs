using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordMessageApplication : SnowflakeObject
{
	[JsonProperty("cover_image", NullValueHandling = NullValueHandling.Ignore)]
	public string? CoverImage { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? Icon { get; internal set; }

	[JsonProperty("name")]
	public string Name { get; internal set; }
}
