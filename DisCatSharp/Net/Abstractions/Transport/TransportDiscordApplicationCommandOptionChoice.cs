using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationCommandOptionChoice
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> NameLocalizations { get; set; }

	[JsonProperty("value")]
	public string Value { get; set; }
}
