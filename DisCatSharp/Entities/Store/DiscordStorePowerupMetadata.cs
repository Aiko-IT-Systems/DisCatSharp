using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents guild powerup metadata.
/// </summary>
public sealed class DiscordStorePowerupMetadata
{
	/// <summary>
	///     Gets the animated image url.
	/// </summary>
	[JsonProperty("animated_image_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? AnimatedImageUrl { get; internal set; }

	/// <summary>
	///     Gets the boost price.
	/// </summary>
	[JsonProperty("boost_price", NullValueHandling = NullValueHandling.Ignore)]
	public int? BoostPrice { get; internal set; }

	/// <summary>
	///     Gets the category type.
	/// </summary>
	[JsonProperty("category_type", NullValueHandling = NullValueHandling.Ignore)]
	public string? CategoryType { get; internal set; }

	/// <summary>
	///     Gets the guild feature bundle.
	/// </summary>
	[JsonProperty("guild_features", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordStoreGuildFeatureBundle? GuildFeatures { get; internal set; }

	/// <summary>
	///     Gets the purchase limit.
	/// </summary>
	[JsonProperty("purchase_limit", NullValueHandling = NullValueHandling.Ignore)]
	public int? PurchaseLimit { get; internal set; }

	/// <summary>
	///     Gets the static image url.
	/// </summary>
	[JsonProperty("static_image_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? StaticImageUrl { get; internal set; }
}
