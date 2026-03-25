using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents guild monetization metadata.
/// </summary>
public sealed class DiscordStoreGuildMonetization
{
	/// <summary>
	///     Gets guild powerup metadata.
	/// </summary>
	[JsonProperty("powerup", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordStorePowerupMetadata? Powerup { get; internal set; }
}
