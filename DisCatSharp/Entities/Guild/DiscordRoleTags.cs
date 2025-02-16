using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord role tags object with detailed classification logic.
/// </summary>
public sealed class DiscordRoleTags : ObservableApiObject
{
	/// <summary>
	///     Whether this role is a guild's linked role.
	/// </summary>
	[JsonProperty("guild_connections", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool?> GuildConnections { get; internal set; }

	/// <summary>
	///     Whether this is the guild's booster role.
	/// </summary>
	[JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool?> PremiumSubscriber { get; internal set; }

	/// <summary>
	///     Gets the ID of the bot this role belongs to.
	/// </summary>
	[JsonProperty("bot_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong> BotId { get; internal set; }

	/// <summary>
	///     Gets the ID of the integration this role belongs to.
	/// </summary>
	[JsonProperty("integration_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong> IntegrationId { get; internal set; }

	/// <summary>
	///     Gets the ID of this role's subscription SKU and listing.
	/// </summary>
	[JsonProperty("subscription_listing_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong> SubscriptionListingId { get; internal set; }

	/// <summary>
	///     Whether this role is available for purchase.
	/// </summary>
	[JsonProperty("available_for_purchase", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool?> AvailableForPurchase { get; internal set; }

	/// <summary>
	///     Whether this role is a guild product role.
	/// </summary>
	[JsonProperty("is_guild_product_role", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool> IsGuildProductRole { get; internal set; }

	/// <summary>
	///     Determines the type of role based on its tags.
	/// </summary>
	public RoleType DetermineRoleType()
	{
		if (this.BotId.HasValue)
			return RoleType.Bot;

		if (this.IsGuildProductRole.HasValue)
			return RoleType.ServerProduct;

		if (this.PremiumSubscriber.HasValue)
			return RoleType.Booster;

		if (this.IntegrationId.HasValue && this.SubscriptionListingId.HasValue)
			return !this.AvailableForPurchase.HasValue ? RoleType.PremiumSubscriberTierDraft : RoleType.PremiumSubscriberTier;

		if (this.IntegrationId.HasValue)
			return RoleType.ExternalPlatformOrPremiumSubscriber; // We need to wait for discord to fix this bullshit, to return RoleType.ExternalPlatform or RoleType.PremiumSubscriber.

		if (this.GuildConnections.HasValue)
			return RoleType.RoleConnection;

		return RoleType.Normal;
	}
}
