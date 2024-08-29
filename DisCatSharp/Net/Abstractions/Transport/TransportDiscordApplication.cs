using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

internal sealed class TransportDiscordApplication : SnowflakeObject
{
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? Icon { get; set; }

	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; set; }

	[JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
	public string? Summary { get; set; }

	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser Bot { get; set; }

	[JsonProperty("bot_public", NullValueHandling = NullValueHandling.Ignore)]
	public bool? BotPublic { get; set; }

	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationFlags Flags { get; set; }

	[JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? TermsOfServiceUrl { get; set; }

	[JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? PrivacyPolicyUrl { get; set; }

	[JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Ignore)]
	public bool? BotRequiresCodeGrant { get; set; }

	[JsonProperty("hook", NullValueHandling = NullValueHandling.Ignore)]
	public bool Hook { get; set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; set; }

	[JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> RpcOrigins { get; set; } = [];

	[JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordUser Owner { get; set; }

	[JsonProperty("team", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordTeam? Team { get; set; }

	[JsonProperty("verify_key", NullValueHandling = NullValueHandling.Ignore)]
	public string? VerifyKey { get; set; }

	[JsonProperty("guild_id")]
	public ulong? GuildId { get; set; }

	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordGuild? Guild { get; set; }

	[JsonProperty("primary_sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? PrimarySkuId { get; set; }

	[JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
	public string? Slug { get; set; }

	[JsonProperty("cover_image", NullValueHandling = NullValueHandling.Ignore)]
	public string? CoverImage { get; set; }

	[JsonProperty("custom_install_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? CustomInstallUrl { get; set; }

	[JsonProperty("install_params", NullValueHandling = NullValueHandling.Ignore)]
	public TransportDiscordInstallParams? InstallParams { get; set; }

	[JsonProperty("role_connections_verification_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? RoleConnectionsVerificationUrl { get; set; }

	[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Tags { get; set; } = [];

	[JsonProperty("approximate_guild_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ApproximateGuildCount { get; set; }

	[JsonProperty("approximate_user_install_count", NullValueHandling = NullValueHandling.Ignore)]
	public int ApproximateUserInstallCount { get; set; }

	[JsonProperty("interactions_endpoint_url", NullValueHandling = NullValueHandling.Ignore)]
	public string? InteractionsEndpointUrl { get; set; }

	[JsonProperty("redirect_uris", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> RedirectUris { get; set; }

	[JsonProperty("integration_types_config", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordIntegrationTypesConfig? IntegrationTypesConfig { get; set; }

	[JsonProperty("is_monetized", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMonetized { get; set; }

	[JsonProperty("is_verified", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsVerified { get; set; }

	[JsonProperty("storefront_available", NullValueHandling = NullValueHandling.Ignore)]
	public bool StorefrontAvailable { get; set; }

	[JsonProperty("interactions_event_types", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> InteractionsEventTypes { get; set; } = [];

	[JsonProperty("interactions_version", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationInteractionsVersion InteractionsVersion { get; set; }

	[JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationExplicitContentFilter ExplicitContentFilter { get; set; }

	[JsonProperty("rpc_application_state", NullValueHandling = NullValueHandling.Ignore)]
	public RpcApplicationState RpcApplicationState { get; set; }

	[JsonProperty("store_application_state", NullValueHandling = NullValueHandling.Ignore)]
	public StoreApplicationState StoreApplicationState { get; set; }

	[JsonProperty("verification_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationVerificationState VerificationState { get; set; }

	[JsonProperty("integration_public", NullValueHandling = NullValueHandling.Ignore)]
	public bool IntegrationPublic { get; set; }

	[JsonProperty("integration_require_code_grant", NullValueHandling = NullValueHandling.Ignore)]
	public bool IntegrationRequireCodeGrant { get; set; }

	[JsonProperty("discoverability_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationDiscoverabilityState DiscoverabilityState { get; set; }

	[JsonProperty("discovery_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationDiscoveryEligibilityFlags DiscoveryEligibilityFlags { get; set; }

	[JsonProperty("monetization_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationMonetizationState MonetizationState { get; set; }

	[JsonProperty("verification_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationVerificationEligibilityFlags VerificationEligibilityFlags { get; set; }

	[JsonProperty("monetization_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationMonetizationEligibilityFlags MonetizationEligibilityFlags { get; set; }

	[JsonProperty("internal_guild_restriction", NullValueHandling = NullValueHandling.Ignore)]
	public int InternalGuildRestriction { get; set; }
}
