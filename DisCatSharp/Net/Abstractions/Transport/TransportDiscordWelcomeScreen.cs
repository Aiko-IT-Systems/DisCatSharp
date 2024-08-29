using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordWelcomeScreen
{
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	[JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
	public List<TransportDiscordWelcomeScreenChannel>? WelcomeChannels { get; internal set; }
}
