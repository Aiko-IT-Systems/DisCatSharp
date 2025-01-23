using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a <see cref="DiscordSubscription" />.
/// </summary>
public sealed class DiscordSubscription : SnowflakeObject
{
	/// <summary>
	///     Gets the id of the user who subscribed.
	/// </summary>
	[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong UserId { get; internal set; }

	/// <summary>
	///     Gets the user who subscribed, if they're in the cache.
	/// </summary>
	[JsonIgnore]
	public DiscordUser? User
		=> this.Discord.UserCache.GetValueOrDefault(this.UserId);

	/// <summary>
	///     Gets the current status of the subscription.
	/// </summary>
	[JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
	public SubscriptionStatus Status { get; internal set; }

	/// <summary>
	///     Gets the list of SKUs the user is subscribed to.
	/// </summary>
	[JsonProperty("sku_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> SkuIds { get; internal set; } = [];

	/// <summary>
	///     Gets the list of SKUs that this user will be subscribed to at renewal.
	/// </summary>
	[JsonProperty("renewal_sku_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> RenewalSkuIds { get; internal set; } = [];

	/// <summary>
	///     Gets the list of entitlements granted for this subscription.
	/// </summary>
	[JsonProperty("entitlement_ids", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> EntitlementIds { get; internal set; } = [];

	/// <summary>
	///     Gets the start of the current subscription period.
	/// </summary>
	[JsonProperty("current_period_start", NullValueHandling = NullValueHandling.Ignore)]
	internal string? CurrentPeriodStartRaw { get; set; }

	/// <summary>
	///     Gets the start of the current subscription period.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CurrentPeriodStartsAt
		=> !string.IsNullOrWhiteSpace(this.CurrentPeriodStartRaw) && DateTimeOffset.TryParse(this.CurrentPeriodStartRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets the end of the current subscription period.
	/// </summary>
	[JsonProperty("current_period_end", NullValueHandling = NullValueHandling.Ignore)]
	internal string? CurrentPeriodEndRaw{ get; set; }

	/// <summary>
	///     Gets the end of the current subscription period.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CurrentPeriodEndsAt
		=> !string.IsNullOrWhiteSpace(this.CurrentPeriodEndRaw) && DateTimeOffset.TryParse(this.CurrentPeriodEndRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     Gets when the subscription was canceled.
	/// </summary>
	[JsonProperty("canceled_at", NullValueHandling = NullValueHandling.Ignore)]
	internal string? CanceledAtRaw { get; set; }

	/// <summary>
	///     Gets when the subscription was canceled.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? CanceledAt
		=> !string.IsNullOrWhiteSpace(this.CanceledAtRaw) && DateTimeOffset.TryParse(this.CanceledAtRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	///     <para>Gets the ISO3166-1 alpha-2 country code of the payment source used to purchase the subscription.</para>
	///     <para>Missing unless queried with a private OAuth scope.</para>
	/// </summary>
	[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
	public string? Country { get; internal set; }
}
