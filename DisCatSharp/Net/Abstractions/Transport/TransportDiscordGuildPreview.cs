using System.Collections.Generic;

using DisCatSharp.Entities;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordGuildPreview : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? Icon { get; internal set; }

	[JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? Splash { get; internal set; }

	[JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? DiscoverySplash { get; internal set; }

	[JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordEmoji>? Emojis { get; internal set; }

	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Features { get; internal set; }

	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ApproximateMemberCount { get; internal set; }

	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ApproximatePresenceCount { get; internal set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }
}
