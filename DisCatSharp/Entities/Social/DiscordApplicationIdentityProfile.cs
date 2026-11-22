using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>Represents the game-stat profile attached to an application identity.</summary>
public sealed class DiscordApplicationIdentityProfile : ObservableApiObject
{
	/// <summary>Gets the user's external username.</summary>
	[JsonProperty("username")] public string? Username { get; internal set; }
	/// <summary>Gets arbitrary application metadata, which Discord does not use for widgets.</summary>
	[JsonProperty("metadata")] public JObject? Metadata { get; internal set; }
	/// <summary>Gets the current game-stat data.</summary>
	[JsonProperty("data")] public DiscordGameProfileData? Data { get; internal set; }
}
