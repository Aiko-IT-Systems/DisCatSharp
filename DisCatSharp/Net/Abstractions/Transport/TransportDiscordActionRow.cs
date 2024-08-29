using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordActionRow : TransportDiscordMessageComponent
{
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordMessageComponent> Components { get; internal set; }
}
