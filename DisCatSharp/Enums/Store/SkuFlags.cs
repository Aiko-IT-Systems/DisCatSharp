// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DisCatSharp.Enums;

/// <summary>
/// Represents sku flags.
/// </summary>
public enum SkuFlags : long
{

	/// <summary>
	/// Whether the SKU is a premium purchase.
	/// </summary>
	PremiumPurchase = 1<<0,

	/// <summary>
	/// Whether the SKU is free premium content.
	/// </summary>
	HasFreePremiumContent = 1<<1,

	/// <summary>
	/// Whether the SKU is available for purchase.
	/// </summary>
	Available = 1<<2,

	/// <summary>
	/// Whether the SKU is a premium or distribution product.
	/// </summary>
	PremiumAndDistribution = 1<<3,

	/// <summary>
	/// Whether the SKU is a premium sticker pack.
	/// </summary>
	StickerPack = 1<<4,

	/// <summary>
	/// Whether the SKU is a guild role subscription. These are subscriptions made to guilds for premium perks.
	/// </summary>
	GuildRoleSubscription = 1<<5,

	/// <summary>
	/// Whether the SKU is a Discord premium subscription or related first-party product.
	/// These are subscriptions like Nitro and Server Boosts. These are the only giftable subscriptions.
	/// </summary>
	PremiumSubscription = 1<<6,

	/// <summary>
	/// Whether the SKU is a application subscription. These are subscriptions made to applications for premium perks bound to a guild.
	/// </summary>
	ApplicationGuildSubscription = 1<<7,

	/// <summary>
	/// Whether the SKU is a application subscription. These are subscriptions made to applications for premium perks bound to a user.
	/// </summary>
	ApplicationUserSubscription = 1<<8
}
