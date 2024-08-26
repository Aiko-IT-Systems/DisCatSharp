using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a interaction resource.
/// </summary>
public sealed class DiscordInteractionResource : ObservableApiObject
{
	/// <summary>
	///     Gets the resource type.
	/// </summary>
	[JsonProperty("type")]
	public InteractionResponseType Type { get; internal set; }

	/// <summary>
	///     Message created by the interaction..
	///     <para>Only present if <see cref="Type"/> is either <see cref="InteractionResponseType.ChannelMessageWithSource"/> or <see cref="InteractionResponseType.UpdateMessage"/>.</para>
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessage? Message { get; internal set; }

	/// <summary>
	///     Represents the Activity launched by this interaction.
	///     <para>Only present if <see cref="Type"/> is either <see cref="InteractionResponseType.LaunchActivity"/>.</para>
	/// </summary>
	[JsonProperty("activity_instance", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordActivityInstance? ActivityInstance { get; internal set; }
}
