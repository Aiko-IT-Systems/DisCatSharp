using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a user's collectibles.
/// </summary>
public sealed class DiscordCollectibles
{
	/// <summary>
	///     Gets the user's nameplate, if applicable.
	/// </summary>
	[JsonProperty("nameplate", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordNameplate? Nameplate { get; internal set; }
}
