using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents audio inside an embed.
/// </summary>
public sealed class DiscordEmbedAudio : DiscordUnfurledMedia
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordEmbedAudio" /> class.
	/// </summary>
	internal DiscordEmbedAudio()
	{ }

	/// <summary>
	///     Gets the description of the audio.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }
}
