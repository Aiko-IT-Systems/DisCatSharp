using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Describes a lobby member to add, update, or remove.</summary>
public sealed class DiscordLobbyMemberUpdate
{
	/// <summary>Gets or sets the user ID.</summary>
	[JsonProperty("id")] public ulong Id { get; set; }
	/// <summary>Gets or sets metadata to replace the member's metadata, when specified.</summary>
	[JsonProperty("metadata")] public Optional<Dictionary<string, string>?> Metadata { get; set; }
	/// <summary>Gets or sets the member's flags, when specified.</summary>
	[JsonProperty("flags")] public Optional<DiscordLobbyMemberFlags> Flags { get; set; }
	/// <summary>Gets or sets the additional display name; an explicit null clears it.</summary>
	[JsonProperty("additional_name")] public Optional<string?> AdditionalName { get; set; }
	/// <summary>Gets or sets whether a bulk request removes this member.</summary>
	[JsonProperty("remove_member", NullValueHandling = NullValueHandling.Ignore)] public bool? RemoveMember { get; set; }
}
