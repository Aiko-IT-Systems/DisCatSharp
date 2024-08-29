using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordAutoModerationAction
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodActionType Type { get; internal set; }

	[JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordAutoModerationActionMetadata Metadata { get; internal set; }
}
