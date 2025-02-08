using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a <see cref="DiscordEntitlement" />.
/// </summary>
public sealed class DiscordEntitlement : SnowflakeObject
{
	/// <summary>
	///     Gets this entitlement's bound application id.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ApplicationId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's branches.
	/// </summary>
	[JsonProperty("branches", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> Branches { get; internal set; } = [];

	/// <summary>
	///     Gets whether this entitlement was consumed.
	/// </summary>
	[JsonProperty("consumed", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Consumed { get; internal set; }

	/// <summary>
	///     Gets whether this entitlement was deleted.
	/// </summary>
	[JsonProperty("deleted", NullValueHandling = NullValueHandling.Ignore)]
	public bool Deleted { get; internal set; }

	/// <summary>
	///     Gets when this entitlement ends as raw string.
	/// </summary>
	[JsonProperty("ends_at", NullValueHandling = NullValueHandling.Ignore)]
	public string? EndsAtRaw { get; internal set; }

	/// <summary>
	///     Gets this entitlement's gift code batch id.
	/// </summary>
	[JsonProperty("gift_code_batch_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GiftCodeBatchId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's gift code flags.
	/// </summary>
	[JsonProperty("gift_code_flags", NullValueHandling = NullValueHandling.Ignore)]
	public int GiftCodeFlags { get; internal set; }

	/// <summary>
	///     Gets this entitlement's gifter user id.
	/// </summary>
	[JsonProperty("gifter_user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GifterUserId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's bound guild id.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's parent id.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ParentId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's promotion id.
	/// </summary>
	[JsonProperty("promotion_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? PromotionId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's bound sku id.
	/// </summary>
	[JsonProperty("sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong SkuId { get; internal set; }

	/// <summary>
	///     Gets when this entitlement starts as raw string.
	/// </summary>
	[JsonProperty("starts_at", NullValueHandling = NullValueHandling.Ignore)]
	public string? StartsAtRaw { get; internal set; }

	/// <summary>
	///     Gets this entitlement's subscription id.
	/// </summary>
	[JsonProperty("subscription_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? SubscriptionId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public EntitlementType Type { get; internal set; }

	/// <summary>
	///     Gets this entitlement's bound user id.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; internal set; }

	/// <summary>
	///     Gets this entitlement's bound user.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User
		=> this.Discord.GetCachedOrEmptyUserInternal(this.UserId);

	/// <summary>
	///     Gets when this entitlement starts.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? StartsAt
		=> !string.IsNullOrWhiteSpace(this.StartsAtRaw) && DateTimeOffset.TryParse(this.StartsAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets when this entitlement ends.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? EndsAt
		=> !string.IsNullOrWhiteSpace(this.EndsAtRaw) && DateTimeOffset.TryParse(this.EndsAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets this entitlement's bound guild.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild? Guild
		=> this.GuildId.HasValue ? this.Discord.Guilds[this.GuildId.Value] : null;

	/// <summary>
	///     Deletes this entitlement if it's a test entitlement.
	/// </summary>
	public async Task DeleteTestEntitlementsAsync()
		=> await (this.Discord as DiscordClient)!.DeleteTestEntitlementAsync(this.Id);

	/// <summary>
	///    Consumes this entitlement if it's consumable.
	/// </summary>
	/// <returns>Whether the entitlement was consumed.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the entitlement is not consumable.</exception>
	public async Task<bool> ConsumeAsync()
		=> this.Consumed is null
			? throw new InvalidOperationException("You can only consume consumable entitlements.")
			: (this.Consumed.HasValue && this.Consumed.Value) || await (this.Discord as DiscordClient)!.ConsumeEntitlementAsync(this.Id);
}
