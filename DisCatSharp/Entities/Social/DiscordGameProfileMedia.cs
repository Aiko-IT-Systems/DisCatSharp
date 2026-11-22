using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents an image used in a game-stat profile.</summary>
public sealed class DiscordGameProfileMedia
{
	/// <summary>Gets or sets the publicly reachable image URL.</summary>
	[JsonProperty("url")] public string Url { get; set; } = string.Empty;
}
