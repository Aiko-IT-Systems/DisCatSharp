using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord role tags.
/// </summary>
public class DiscordRoleTags : ObservableApiObject
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
