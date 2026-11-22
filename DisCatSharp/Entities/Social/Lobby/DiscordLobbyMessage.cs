using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>Represents a message in a Discord lobby.</summary>
public sealed class DiscordLobbyMessage : ObservableApiObject
{
	/// <summary>Gets the message ID.</summary>
	[JsonProperty("id")] public ulong Id { get; internal set; }
	/// <summary>Gets the message type.</summary>
	[JsonProperty("type")] public MessageType Type { get; internal set; }
	/// <summary>Gets the message content.</summary>
	[JsonProperty("content")] public string Content { get; internal set; } = string.Empty;
	/// <summary>Gets the ID of the lobby containing the message.</summary>
	[JsonProperty("lobby_id")] public ulong LobbyId { get; internal set; }
	/// <summary>Gets the compatibility channel ID.</summary>
	[JsonProperty("channel_id")] public ulong ChannelId { get; internal set; }
	/// <summary>Gets the message author.</summary>
	[JsonProperty("author")] public DiscordUser Author { get; internal set; } = null!;
	/// <summary>Gets the author's lobby details captured when the message was sent.</summary>
	[JsonProperty("lobby_member")] public DiscordLobbyMessageMember? LobbyMember { get; internal set; }
	/// <summary>Gets dispatch-only metadata, when present.</summary>
	[JsonProperty("metadata")] public IReadOnlyDictionary<string, string>? Metadata { get; internal set; }
	/// <summary>Gets moderation metadata, when present.</summary>
	[JsonProperty("moderation_metadata")] public IReadOnlyDictionary<string, string>? ModerationMetadata { get; internal set; }
	/// <summary>Gets the message flags.</summary>
	[JsonProperty("flags")] public MessageFlags Flags { get; internal set; }
	/// <summary>Gets the application ID associated with the message.</summary>
	[JsonProperty("application_id")] public ulong ApplicationId { get; internal set; }
}
