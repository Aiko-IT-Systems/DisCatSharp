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
	public int Type { get; internal set; }

	/// <summary>
	/// Only not-null if <see cref="Type"/> is <c>4</c>.
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordMessage? Message { get; internal set; }
}
