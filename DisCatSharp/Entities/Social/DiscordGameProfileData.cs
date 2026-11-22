using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>Contains game stats displayed by Discord game-stat widgets.</summary>
/// <remarks>When supplied in a profile update, this whole object replaces the existing profile data.</remarks>
public sealed class DiscordGameProfileData
{
	/// <summary>Gets or sets the optional primary stats.</summary>
	[JsonProperty("primary", NullValueHandling = NullValueHandling.Ignore)] public DiscordGamePrimaryStats? Primary { get; set; }
	/// <summary>Gets or sets the optional custom stat fields; Discord accepts at most 30.</summary>
	[JsonProperty("dynamic", NullValueHandling = NullValueHandling.Ignore)] public List<DiscordGameDynamicField>? Dynamic { get; set; }
	/// <summary>Preserves fields Discord may add to the data object in the future.</summary>
	[JsonExtensionData] public IDictionary<string, JToken>? AdditionalData { get; set; }
}
