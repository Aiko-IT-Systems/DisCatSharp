using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordInstallParams
{
	[JsonProperty("scopes", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Scopes { get; internal set; }

	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public string Permissions { get; internal set; }
}
