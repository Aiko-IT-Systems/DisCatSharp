using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the interaction metadata.
/// </summary>
public sealed class DiscordInteractionMetadata : SnowflakeObject
{
	/// <summary>
	/// Type of the interaction.
	/// </summary>
	[JsonProperty("type")]
	public InteractionType Type { get; internal set; }

	/// <summary>
	/// The transport user who triggered the interaction.
	/// </summary>
	[JsonProperty("user")]
	internal TransportUser TransportUser { get; set; }

	/// <summary>
	/// Ther user who triggered the interaction.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User
		=> new(this.TransportUser)
		{
			Discord = this.Discord
		};

	/// <summary>
	/// This is the same field on interaction.
	/// </summary>
	[JsonProperty("authorizing_integration_owners")]
	public AuthorizingIntegrationOwners AuthorizingIntegrationOwners { get; internal set; } = new();

	/// <summary>
	/// <para>On followup messages only, this will be the ID of the original response message.</para>
	/// <para>On original response messages this will not be present.</para>
	/// </summary>
	[JsonProperty("original_response_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OriginalResponseMessageId { get; internal set; }

	/// <summary>
	/// <para>The name of the application command.</para>
	/// <note type="note"><para>For messages created from application command interactions.</para>
	/// <para>Check <see cref="Type"/> to determine if this field is represented.</para></note>
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string? Name { get; internal set; }

	/// <summary>
	/// <para>The ID of the message that was interacted with to trigger the interaction.</para>
	/// <note type="note"><para>For messages created from message component interactions.</para>
	/// <para>Check <see cref="Type"/> to determine if this field is represented.</para></note>
	/// </summary>
	[JsonProperty("interacted_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? InteractedMessageId { get; internal set; }

	/// <summary>
	/// <para>The interaction metadata of the interaction that responded with the modal.</para>
	/// <note type="note"><para>For messages created from a modal submit interaction.</para>
	/// <para>Check <see cref="Type"/> to determine if this field is represented.</para></note>
	/// </summary>
	[JsonProperty("triggering_interaction_metadata", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionMetadata? TriggeringInteractionMetadata { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="DiscordInteractionMetadata"/> object.
	/// </summary>
	internal DiscordInteractionMetadata()
	{ }
}
