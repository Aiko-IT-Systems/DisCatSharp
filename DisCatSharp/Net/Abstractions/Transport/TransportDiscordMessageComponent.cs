using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public abstract class TransportDiscordMessageComponent
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType Type { get; internal set; }
}
