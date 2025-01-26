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
	public bool? GuildConnections { get; internal set; }

	/// <summary>
	///     Whether this is the guild's booster role.
	/// </summary>
	[JsonProperty("premium_subscriber", NullValueHandling = NullValueHandling.Include)]
	public bool? PremiumSubscriber { get; internal set; }

	/// <summary>
	///     Gets the ID of the bot this role belongs to.
	/// </summary>
	[JsonProperty("bot_id", NullValueHandling = NullValueHandling.Include)]
	public ulong? BotId { get; internal set; }

	/// <summary>
	///     Gets the ID of the integration this role belongs to.
	/// </summary>
	[JsonProperty("integration_id", NullValueHandling = NullValueHandling.Include)]
	public ulong? IntegrationId { get; internal set; }

	/// <summary>
	///     Gets the ID of this role's subscription SKU and listing.
	/// </summary>
	[JsonProperty("subscription_listing_id", NullValueHandling = NullValueHandling.Include)]
	public ulong? SubscriptionListingId { get; internal set; }

	/// <summary>
	///     Whether this role is available for purchase.
	/// </summary>
	[JsonProperty("available_for_purchase", NullValueHandling = NullValueHandling.Include)]
	public bool? AvailableForPurchase { get; internal set; }

	/// <summary>
	///     Determines the type of role based on its tags.
	/// </summary>
	public RoleType DetermineRoleType()
	{
		if (this.BotId is not null)
			return RoleType.Bot;

		switch (this.GuildConnections)
		{
			case false when this.PremiumSubscriber is null:
				return RoleType.Booster;
			case false when this.PremiumSubscriber is false:
				return RoleType.ServerProduct;
			case false when this.IntegrationId is not null && this.PremiumSubscriber is false:
				return RoleType.PremiumSubscriber;
			case false when this.IntegrationId is not null && this.SubscriptionListingId is not null:
				return RoleType.PremiumSubscriberTier;
		}

		if (this.IntegrationId is not null && this.BotId is null && this.PremiumSubscriber is null && this.SubscriptionListingId is null)
			return RoleType.ExternalPlatform;

		return this.GuildConnections is null
			? RoleType.RoleConnection
			: RoleType.Normal;
	}
}
