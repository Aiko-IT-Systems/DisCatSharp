using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordInteractionMetadata : NullableSnowflakeObject
{
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionType Type { get; internal set; }

	[JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser? User { get; internal set; }

	[JsonProperty("authorizing_integration_owners", NullValueHandling = NullValueHandling.Ignore)]
	public AuthorizingIntegrationOwners? AuthorizingIntegrationOwners { get; internal set; }

	[JsonProperty("original_response_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OriginalResponseMessageId { get; internal set; }

	[JsonProperty("interacted_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? InteractedMessageId { get; internal set; }

	[JsonProperty("triggering_interaction_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordInteractionMetadata? TriggeringInteractionMetadata { get; internal set; }
}
