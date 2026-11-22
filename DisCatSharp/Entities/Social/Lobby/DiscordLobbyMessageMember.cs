using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents the lobby-member information captured on a lobby message.</summary>
public sealed class DiscordLobbyMessageMember
{
	/// <summary>Gets the member's additional display name.</summary>
	[JsonProperty("additional_name")] public string? AdditionalName { get; internal set; }
}
