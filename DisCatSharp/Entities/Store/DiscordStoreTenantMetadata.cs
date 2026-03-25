using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents tenant-scoped store metadata.
/// </summary>
public sealed class DiscordStoreTenantMetadata
{
	/// <summary>
	///     Gets guild monetization metadata.
	/// </summary>
	[JsonProperty("guild_monetization", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordStoreGuildMonetization? GuildMonetization { get; internal set; }
}
