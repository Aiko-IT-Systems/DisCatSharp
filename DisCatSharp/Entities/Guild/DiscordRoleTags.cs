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

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord role tags.
/// </summary>
public class DiscordRoleTags
{
	/// <summary>
	/// Gets the id of the bot this role belongs to.
	/// </summary>
	[JsonProperty("bot_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? BotId { get; internal set; }

	/// <summary>
	/// Gets the id of the integration this role belongs to.
	/// </summary>
	[JsonProperty("integration_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? IntegrationId { get; internal set; }

	/// <summary>
	/// Gets whether this is the guild's booster role.
	/// </summary>
	[JsonIgnore]
	public bool IsPremiumSubscriber
		=> this.PremiumSubscriber.HasValue && this.PremiumSubscriber.Value;
	[JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Include)]
	internal bool? PremiumSubscriber = false;

	/// <summary>
	/// The id of this role's subscription sku and listing.
	/// </summary>
	[JsonProperty("subscription_listing_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? SubscriptionListingId { get; internal set; }

	/// <summary>
	/// Whether this role is available for purchase.
	/// </summary>
	[JsonProperty("available_for_purchase", NullValueHandling = NullValueHandling.Ignore)]
	public bool? AvailableForPurchase { get; internal set; }

	/// <summary>
	/// Gets whether this is the guild's premium subscriber role.
	/// </summary>
	[JsonIgnore]
	public bool IsLinkedRole
		=> this.GuildConnection.HasValue && this.GuildConnection.Value;

	[JsonProperty("guild_connections", NullValueHandling = NullValueHandling.Include)]
	internal bool? GuildConnection = false;
}
