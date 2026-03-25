using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents additional guild feature counts for a powerup.
/// </summary>
public sealed class DiscordStoreGuildFeatureBundle
{
	/// <summary>
	///     Gets the additional emoji slot count.
	/// </summary>
	[JsonProperty("additional_emoji_slots", NullValueHandling = NullValueHandling.Ignore)]
	public int? AdditionalEmojiSlots { get; internal set; }

	/// <summary>
	///     Gets the additional sound slot count.
	/// </summary>
	[JsonProperty("additional_sound_slots", NullValueHandling = NullValueHandling.Ignore)]
	public int? AdditionalSoundSlots { get; internal set; }

	/// <summary>
	///     Gets the additional sticker slot count.
	/// </summary>
	[JsonProperty("additional_sticker_slots", NullValueHandling = NullValueHandling.Ignore)]
	public int? AdditionalStickerSlots { get; internal set; }

	/// <summary>
	///     Gets the guild features unlocked by the powerup.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Features { get; internal set; } = [];
}
