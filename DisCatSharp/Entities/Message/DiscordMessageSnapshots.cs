using DisCatSharp.Attributes;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a message snapshot.
/// </summary>
[DiscordInExperiment, Experimental]
public class DiscordMessageSnapshot : ObservableApiObject
{
	/// <summary>
	///     Gets the forwarded message.
	/// </summary>
	[JsonProperty("message")]
	public DiscordForwardedMessage Message { get; internal set; }
}
