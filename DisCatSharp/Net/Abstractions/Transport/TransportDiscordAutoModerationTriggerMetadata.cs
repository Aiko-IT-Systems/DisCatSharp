using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAutoModerationTriggerMetadata
{
	[JsonProperty("keyword_filter", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> KeywordFilter { get; internal set; }

	[JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
	public List<int> Presets { get; internal set; }

	[JsonProperty("allow_list", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> AllowList { get; internal set; }

	[JsonProperty("mention_total_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? MentionTotalLimit { get; internal set; }
}
