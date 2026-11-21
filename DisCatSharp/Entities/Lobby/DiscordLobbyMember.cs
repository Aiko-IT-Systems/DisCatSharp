using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents a lobby member.</summary>
public sealed class DiscordLobbyMember : ObservableApiObject
{
	/// <summary>Gets the member's user ID.</summary>
	[JsonProperty("id")] public ulong Id { get; internal set; }
	/// <summary>Gets the member's metadata.</summary>
	[JsonProperty("metadata")] public IReadOnlyDictionary<string, string>? Metadata { get; internal set; }
	/// <summary>Gets the member's lobby permissions.</summary>
	[JsonProperty("flags")] public DiscordLobbyMemberFlags Flags { get; internal set; }
	/// <summary>Gets the member's additional display name, when set.</summary>
	[JsonProperty("additional_name")] public string? AdditionalName { get; internal set; }
}
