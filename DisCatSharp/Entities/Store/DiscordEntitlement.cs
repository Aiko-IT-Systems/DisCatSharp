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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a <see cref="DiscordEntitlement"/>.
/// </summary>
public sealed class DiscordEntitlement : SnowflakeObject
{
	/// <summary>
	/// Gets this entitlement's bound sku id.
	/// </summary>
	[JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SkuId;

	/// <summary>
	/// Gets this entitlement's bound application id.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId;

	/// <summary>
	/// Gets this entitlement's bound user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId;

	/// <summary>
	/// Gets this entitlement's bound user.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User
		=> this.Discord.GetCachedOrEmptyUserInternal(this.UserId);

	/// <summary>
	/// Gets this entitlement's promotion id.
	/// </summary>
	[JsonProperty("promotion_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? PromotionId;

	/// <summary>
	/// Gets this entitlement's type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public EntitlementType Type;

	/// <summary>
	/// Gets whether this entitlement was deleted.
	/// </summary>
	[JsonProperty("deleted", NullValueHandling = NullValueHandling.Ignore)]
	public bool Deleted;

	/// <summary>
	/// Gets this entitlement's gift code flags.
	/// </summary>
	[JsonProperty("gift_code_flags", NullValueHandling = NullValueHandling.Ignore)]
	public int GiftCodeFlags;

	/// <summary>
	/// Gets whether this entitlement was consumed.
	/// </summary>
	[JsonProperty("consumed", NullValueHandling = NullValueHandling.Ignore)]
	public bool Consumed;

	/// <summary>
	/// Gets when this entitlement starts as raw string.
	/// </summary>
	[JsonProperty("starts_at", NullValueHandling = NullValueHandling.Ignore)]
	public string? StartsAtRaw;

	/// <summary>
	/// Gets when this entitlement starts.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? StartsAt
		=> !string.IsNullOrWhiteSpace(this.StartsAtRaw) && DateTimeOffset.TryParse(this.StartsAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets when this entitlement ends as raw string.
	/// </summary>
	[JsonProperty("ends_at", NullValueHandling = NullValueHandling.Ignore)]
	public string? EndsAtRaw;

	/// <summary>
	/// Gets when this entitlement ends.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EndsAt
		=> !string.IsNullOrWhiteSpace(this.EndsAtRaw) && DateTimeOffset.TryParse(this.EndsAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets this entitlement's bound guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId;

	/// <summary>
	/// Gets this entitlement's bound guild.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild? Guild
		=> this.GuildId.HasValue ? this.Discord.Guilds[this.GuildId.Value] : null;

	/// <summary>
	/// Gets this entitlement's subscription id.
	/// </summary>
	[JsonProperty("subscription_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? SubscriptionId;

	/// <summary>
	/// Gets this entitlement's parent id.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ParentId;

	/// <summary>
	/// Gets this entitlement's gift code batch id.
	/// </summary>
	[JsonProperty("gift_code_batch_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GiftCodeBatchId;

	/// <summary>
	/// Gets this entitlement's branches.
	/// </summary>
	[JsonProperty("branches", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Branches = new();

	/// <summary>
	/// Gets this entitlement's gifter user id.
	/// </summary>
	[JsonProperty("gifter_user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GifterUserId;
}
