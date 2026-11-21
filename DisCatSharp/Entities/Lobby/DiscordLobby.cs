using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents a Discord application lobby.</summary>
public sealed class DiscordLobby : ObservableApiObject
{
	/// <summary>Gets the lobby ID.</summary>
	[JsonProperty("id")] public ulong Id { get; internal set; }
	/// <summary>Gets the ID of the application that created the lobby.</summary>
	[JsonProperty("application_id")] public ulong ApplicationId { get; internal set; }
	/// <summary>Gets the lobby metadata, if any.</summary>
	[JsonProperty("metadata")] public IReadOnlyDictionary<string, string>? Metadata { get; internal set; }
	/// <summary>Gets the members returned with the lobby.</summary>
	[JsonProperty("members")] public IReadOnlyList<DiscordLobbyMember> Members { get; internal set; } = [];
	/// <summary>Gets the linked guild channel, if one is present.</summary>
	[JsonProperty("linked_channel")] public DiscordChannel? LinkedChannel { get; internal set; }
}
