using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a interaction resource.
/// </summary>
public sealed class DiscordInteractionResource
{
	/// <summary>
	/// Gets the resource type.
	/// </summary>
	[JsonProperty("type")]
	public InteractionResponseType Type { get; internal set; }

	/// <summary>
	/// Gets the created message, if applicable.
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessage? Message { get; internal set; }
}
