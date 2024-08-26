using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a partial interaction in interaction responses
/// </summary>
public sealed class DiscordInteractionResponseInteraction : SnowflakeObject
{
	/// <summary>
	///     Gets the interaction type.
	/// </summary>
	[JsonProperty("type")]
	public InteractionType Type { get; internal set; }

	/// <summary>
	///     Gets the message id of the created response, if any.
	/// </summary>
	[JsonProperty("response_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ResponseMessageId { get; internal set; }

	/// <summary>
	///     Gets whether the response message is a loading message (deferred response).
	/// </summary>
	[JsonProperty("response_message_loading")]
	public bool ResponseMessageLoading { get; internal set; } = false;

	/// <summary>
	///     Gets whether the response message is ephemeral.
	/// </summary>
	[JsonProperty("response_message_ephemeral")]
	public bool ResponseMessageEphemeral { get; internal set; } = false;
}
