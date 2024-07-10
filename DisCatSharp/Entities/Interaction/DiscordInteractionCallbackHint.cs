using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a callback hint for an interaction.
/// </summary>
public sealed class DiscordInteractionCallbackHint
{
	/// <summary>
	/// The type of callback that will be used for this response.
	/// </summary>
	[JsonProperty("allowed_callback_type", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionResponseType AllowedCallbackType { get; internal set; }

	/// <summary>
	/// Whether this response can be or should be ephemeral.
	/// </summary>
	[JsonProperty("ephemerality", NullValueHandling = NullValueHandling.Ignore)]
	public InteractionCallbackEphemerality Ephemerality { get; internal set; }

	/// <summary>
	/// The permissions that the bot requires to use this callback.
	/// </summary>
	[JsonProperty("required_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? RequiredPermissions { get; internal set; }
}
