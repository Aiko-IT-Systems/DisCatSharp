using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

public class TransportDiscordRoleSubscriptionData
{
	[JsonProperty("role_subscription_listing_id")]
	public ulong RoleSubscriptionListingId { get; internal set; }

	[JsonProperty("tier_name")]
	public string TierName { get; internal set; }

	[JsonProperty("total_months_subscribed")]
	public int TotalMonthsSubscribed { get; internal set; }

	[JsonProperty("is_renewal")]
	public bool IsRenewal { get; internal set; }
}
