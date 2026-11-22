using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Deserializes Discord's wrapper around an application identity lookup result.</summary>
internal sealed class DiscordApplicationIdentityList : ObservableApiObject
{
	/// <summary>Gets the identity records returned by Discord.</summary>
	[JsonProperty("identities")] public List<DiscordApplicationIdentity> Identities { get; internal set; } = [];
}
