using System.Collections.Generic;

using DisCatSharp.Attributes;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     The transport application.
/// </summary>
internal sealed class TransportApplication : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="TransportApplication" /> class.
	/// </summary>
	internal TransportApplication()
	{ }

	/// <summary>
	///     Gets or sets the id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
	public ulong Id { get; set; }

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the icon hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Include)]
	public string IconHash { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public string Description { get; set; }

	/// <summary>
	///     Gets or sets the summary.
	/// </summary>
	[JsonProperty("summary", NullValueHandling = NullValueHandling.Include), DiscordDeprecated("Empty string, will be removed in API v11")]
	public string Summary { get; set; }

	/// <summary>
	///     Gets the bot user.
	/// </summary>
	[JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
	public TransportUser Bot { get; set; }

	/// <summary>
	///     Whether the bot is public.
	/// </summary>
	[JsonProperty("bot_public", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool?> IsPublicBot { get; set; }

	/// <summary>
	///     Gets or sets the flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Include)]
	public ApplicationFlags Flags { get; set; }

	/// <summary>
	///     Gets or sets the terms of service url.
	/// </summary>
	[JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Include)]
	public string TermsOfServiceUrl { get; set; }

	/// <summary>
	///     Gets or sets the privacy policy url.
	/// </summary>
	[JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Include)]
	public string PrivacyPolicyUrl { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether the bot requires code grant.
	/// </summary>
	[JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Include)]
	public Optional<bool?> BotRequiresCodeGrant { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether the bot is a hook.
	/// </summary>
	[JsonProperty("hook", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsHook { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether the bot requires code grant.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public string Type { get; set; }

	/// <summary>
	///     Gets or sets the rpc origins.
	/// </summary>
	[JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> RpcOrigins { get; set; } = [];

	/// <summary>
	///     Gets or sets the owner.
	/// </summary>
	[JsonProperty("owner", NullValueHandling = NullValueHandling.Include)]
	public TransportUser Owner { get; set; }

	/// <summary>
	///     Gets or sets the team.
	/// </summary>
	[JsonProperty("team", NullValueHandling = NullValueHandling.Include)]
	public TransportTeam? Team { get; set; }

	/// <summary>
	///     Gets or sets the verify key.
	/// </summary>
	[JsonProperty("verify_key", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> VerifyKey { get; set; }

	/// <summary>
	///     Gets or sets the guild id.
	/// </summary>
	[JsonProperty("guild_id")]
	public Optional<ulong> GuildId { get; set; }

	/// <summary>
	///     Gets or sets the partial guild.
	/// </summary>
	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<DiscordGuild> Guild { get; set; }

	/// <summary>
	///     Gets or sets the primary sku id.
	/// </summary>
	[JsonProperty("primary_sku_id", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ulong> PrimarySkuId { get; set; }

	/// <summary>
	///     Gets or sets the slug.
	/// </summary>
	[JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Slug { get; set; }

	/// <summary>
	///     Gets or sets the cover image hash.
	/// </summary>
	[JsonProperty("cover_image", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> CoverImageHash { get; set; }

	/// <summary>
	///     Gets or sets the custom install url.
	/// </summary>
	[JsonProperty("custom_install_url", NullValueHandling = NullValueHandling.Include)]
	public string CustomInstallUrl { get; set; }

	/// <summary>
	///     Gets or sets the install params.
	/// </summary>
	[JsonProperty("install_params", NullValueHandling = NullValueHandling.Include)]
	public DiscordApplicationInstallParams InstallParams { get; set; }

	/// <summary>
	///     Gets or sets the role connection verification entry point.
	/// </summary>
	[JsonProperty("role_connections_verification_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> RoleConnectionsVerificationUrl { get; set; }

	/// <summary>
	///     Gets or sets the tags.
	/// </summary>
	[JsonProperty("tags", NullValueHandling = NullValueHandling.Include)]
	public List<string> Tags { get; set; } = [];

	/// <summary>
	///     Gets or sets the approximate guild count.
	/// </summary>
	[JsonProperty("approximate_guild_count", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int> ApproximateGuildCount { get; set; }

	/// <summary>
	///     Gets or sets the approximate user install count.
	/// </summary>
	[JsonProperty("approximate_user_install_count", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<int> ApproximateUserInstallCount { get; set; }

	/// <summary>
	///     Gets or sets the interactions endpoint url.
	/// </summary>
	[JsonProperty("interactions_endpoint_url", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> InteractionsEndpointUrl { get; set; }

	/// <summary>
	///     Gets or sets the rpc origins.
	/// </summary>
	[JsonProperty("redirect_uris", NullValueHandling = NullValueHandling.Include)]
	public Optional<List<string>> RedirectUris { get; set; }

	/// <summary>
	///     Gets or sets the integration types config.
	/// </summary>
	[JsonProperty("integration_types_config", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordIntegrationTypesConfig? IntegrationTypesConfig { get; set; }

	/// <summary>
	///     Gets or sets whether the application is monetized.
	/// </summary>
	[JsonProperty("is_monetized", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMonetized { get; set; }

	/// <summary>
	///     Gets or sets whether the application is verified.
	/// </summary>
	[JsonProperty("is_verified", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsVerified { get; set; }

	/// <summary>
	///     Gets or sets whether the storefront is available.
	/// </summary>
	[JsonProperty("storefront_available", NullValueHandling = NullValueHandling.Ignore)]
	public bool StorefrontAvailable { get; set; }

	/// <summary>
	///     Gets or sets the interaction event types.
	/// </summary>
	[JsonProperty("interactions_event_types", NullValueHandling = NullValueHandling.Include)]
	public List<string> InteractionsEventTypes { get; set; } = [];

	/// <summary>
	///     Gets or sets the interactions version.
	/// </summary>
	[JsonProperty("interactions_version", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationInteractionsVersion InteractionsVersion { get; set; }

	/// <summary>
	///     Gets or sets the explicit content filter level.
	/// </summary>
	[JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
	public ExplicitContentFilterLevel ExplicitContentFilter { get; set; }

	/// <summary>
	///     Gets or sets the RPC application state.
	/// </summary>
	[JsonProperty("rpc_application_state", NullValueHandling = NullValueHandling.Ignore)]
	public RpcApplicationState RpcApplicationState { get; set; }

	/// <summary>
	///     Gets or sets the store application state.
	/// </summary>
	[JsonProperty("store_application_state", NullValueHandling = NullValueHandling.Ignore)]
	public StoreApplicationState StoreApplicationState { get; set; }

	/// <summary>
	///     Gets or sets the verification state.
	/// </summary>
	[JsonProperty("verification_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationVerificationState VerificationState { get; set; }

	/// <summary>
	///     Gets or sets whether the integration is public.
	/// </summary>
	[JsonProperty("integration_public", NullValueHandling = NullValueHandling.Ignore)]
	public bool IntegrationPublic { get; set; }

	/// <summary>
	///     Gets or sets whether the integration requires code grant.
	/// </summary>
	[JsonProperty("integration_require_code_grant", NullValueHandling = NullValueHandling.Ignore)]
	public bool IntegrationRequireCodeGrant { get; set; }

	/// <summary>
	///     Gets or sets the discoverability state.
	/// </summary>
	[JsonProperty("discoverability_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationDiscoverabilityState DiscoverabilityState { get; set; }

	/// <summary>
	///     Gets or sets the discovery eligibility flags.
	/// </summary>
	[JsonProperty("discovery_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationDiscoveryEligibilityFlags DiscoveryEligibilityFlags { get; set; }

	/// <summary>
	///     Gets or sets the monetization state.
	/// </summary>
	[JsonProperty("monetization_state", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationMonetizationState MonetizationState { get; set; }

	/// <summary>
	///     Gets or sets the verification eligibility flags.
	/// </summary>
	[JsonProperty("verification_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public int VerificationEligibilityFlags { get; set; }

	/// <summary>
	///     Gets or sets the monetization eligibility flags.
	/// </summary>
	[JsonProperty("monetization_eligibility_flags", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationMonetizationEligibilityFlags MonetizationEligibilityFlags { get; set; }

	/// <summary>
	///     Gets or sets the internal guild restriction level.
	/// </summary>
	[JsonProperty("internal_guild_restriction", NullValueHandling = NullValueHandling.Ignore)]
	public int InternalGuildRestriction { get; set; }
}
