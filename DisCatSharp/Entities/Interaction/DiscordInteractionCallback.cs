using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a partial interaction in interaction responses
/// </summary>
public sealed class DiscordInteractionCallback : SnowflakeObject
{
	/// <summary>
	///     Gets the interaction type.
	/// </summary>
	[JsonProperty("type")]
	public InteractionType Type { get; internal set; }

	/// <summary>
	///     Gets the instance ID of the Activity if one was launched or joined.
	/// </summary>
	[JsonProperty("activity_instance_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ActivityInstanceId { get; internal set; }

	/// <summary>
	///     Gets the message id of the created response, if any.
	/// </summary>
	[JsonProperty("response_message_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ResponseMessageId { get; internal set; }

	/// <summary>
	///     Gets whether the response message is a loading message (deferred response).
	/// </summary>
	[JsonProperty("response_message_loading", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ResponseMessageLoading { get; internal set; }

	/// <summary>
	///     Gets whether the response message is ephemeral.
	/// </summary>
	[JsonProperty("response_message_ephemeral", NullValueHandling = NullValueHandling.Ignore)]
	public bool? ResponseMessageEphemeral { get; internal set; }
}
