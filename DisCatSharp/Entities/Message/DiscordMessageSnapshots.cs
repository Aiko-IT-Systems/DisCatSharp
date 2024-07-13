using DisCatSharp.Attributes;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

[DiscordInExperiment, Experimental]
public class DiscordMessageSnapshot : ObservableApiObject
{
	[JsonProperty("message")]
	public DiscordForwardedMessage Message { get; internal set; }
}
