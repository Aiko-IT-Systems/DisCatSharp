using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents guild inventory settings.
/// </summary>
public sealed class DiscordGuildInventorySettings
{
	/// <summary>
	/// Whether anyone is allowed to use and collect this guild's emojis.
	/// </summary>
	[JsonProperty("is_emoji_pack_collectible")]
	public bool IsEmojiPackCollectible { get; internal set; }
}
