using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a sku flag extensions.
/// </summary>
public static class SkuFlagExtensions
{
	/// <summary>
	///     Calculates whether these sku flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasSkuFlag(this SkuFlags baseFlags, SkuFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents sku flags.
/// </summary>
[Flags]
public enum SkuFlags : long
{
	/// <summary>
	///     Whether the SKU is a premium purchase.
	/// </summary>
	PremiumPurchase = 1L << 0,

	/// <summary>
	///     Whether the SKU is free premium content.
	/// </summary>
	HasFreePremiumContent = 1L << 1,

	/// <summary>
	///     Whether the SKU is available for purchase.
	/// </summary>
	Available = 1L << 2,

	/// <summary>
	///     Whether the SKU is a premium or distribution product.
	/// </summary>
	PremiumAndDistribution = 1L << 3,

	/// <summary>
	///     Whether the SKU is a premium sticker pack.
	/// </summary>
	StickerPack = 1L << 4,

	/// <summary>
	///     Whether the SKU is a guild role subscription. These are subscriptions made to guilds for premium perks.
	/// </summary>
	GuildRoleSubscription = 1L << 5,

	/// <summary>
	///     Whether the SKU is a Discord premium subscription or related first-party product.
	///     These are subscriptions like Nitro and Server Boosts. These are the only giftable subscriptions.
	/// </summary>
	PremiumSubscription = 1L << 6,

	/// <summary>
	///     Whether the SKU is a application subscription. These are subscriptions made to applications for premium perks bound
	///     to a guild.
	/// </summary>
	ApplicationGuildSubscription = 1L << 7,

	/// <summary>
	///     Whether the SKU is a application subscription. These are subscriptions made to applications for premium perks bound
	///     to a user.
	/// </summary>
	ApplicationUserSubscription = 1L << 8,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
