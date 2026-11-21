using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents a single-use invite to a lobby's linked channel.</summary>
public sealed class DiscordLobbyInvite : ObservableApiObject
{
	/// <summary>Gets the invite code.</summary>
	[JsonProperty("code")] public string Code { get; internal set; } = string.Empty;
}
