using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a interaction response.
/// </summary>
public sealed class DiscordInteractionResponse : ObservableApiObject
{
	/// <summary>
	/// Gets the created interaction.
	/// </summary>
	[JsonProperty("interaction")]
	public DiscordInteractionResponseInteraction InteractionResponse { get; internal set; }

	/// <summary>
	/// Gets the interaction resource. If it's type is <c>4</c>, it will have a message attached.
	/// </summary>
	[JsonProperty("resource")]
	public DiscordInteractionResource Resource { get; internal set; }

	/// <summary>
	/// Gets the created message from the <see cref="Resource"/>.
	/// </summary>
	[JsonIgnore]
	public DiscordMessage? Message
		=> this.Resource.Message;
}
