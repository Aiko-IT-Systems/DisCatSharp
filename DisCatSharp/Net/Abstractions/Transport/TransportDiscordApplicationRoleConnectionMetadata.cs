using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordApplicationRoleConnectionMetadata
{
	[JsonProperty("type")]
	public ApplicationRoleConnectionMetadataType Type { get; set; }

	[JsonProperty("key")]
	public string Key { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> NameLocalizations { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }

	[JsonProperty("description_localizations", NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, string> DescriptionLocalizations { get; set; }
}
