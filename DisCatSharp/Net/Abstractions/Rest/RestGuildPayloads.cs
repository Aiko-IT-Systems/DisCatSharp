using System;
using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Net.Abstractions;

/// <summary>
///     The reason action.
/// </summary>
internal interface IReasonAction
{
	/// <summary>
	///     Gets or sets the reason.
	/// </summary>
	string Reason { get; set; }
}

/// <summary>
///     Represents a guild modify payload.
/// </summary>
internal sealed class RestGuildModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public Optional<string> Name { get; set; }

	/// <summary>
	///     Gets or sets the icon base64.
	/// </summary>
	[JsonProperty("icon")]
	public Optional<string> IconBase64 { get; set; }

	/// <summary>
	///     Gets or sets the verification level.
	/// </summary>
	[JsonProperty("verification_level")]
	public Optional<VerificationLevel> VerificationLevel { get; set; }

	/// <summary>
	///     Gets or sets the default message notifications.
	/// </summary>
	[JsonProperty("default_message_notifications")]
	public Optional<DefaultMessageNotifications> DefaultMessageNotifications { get; set; }

	/// <summary>
	///     Gets or sets the splash base64.
	/// </summary>
	[JsonProperty("splash")]
	public Optional<string> SplashBase64 { get; set; }

	/// <summary>
	///     Gets or sets the banner base64.
	/// </summary>
	[JsonProperty("banner")]
	public Optional<string> BannerBase64 { get; set; }

	/// <summary>
	///     Gets or sets the home header base64.
	/// </summary>
	[JsonProperty("home_header")]
	public Optional<string> HomeHeaderBase64 { get; set; }

	/// <summary>
	///     Gets or sets the discovery splash base64.
	/// </summary>
	[JsonProperty("discovery_splash")]
	public Optional<string> DiscoverySplashBase64 { get; set; }

	/// <summary>
	///     Gets or sets the afk channel id.
	/// </summary>
	[JsonProperty("afk_channel_id")]
	public Optional<ulong?> AfkChannelId { get; set; }

	/// <summary>
	///     Gets or sets the afk timeout.
	/// </summary>
	[JsonProperty("afk_timeout")]
	public Optional<int> AfkTimeout { get; set; }

	/// <summary>
	///     Gets or sets the mfa level.
	/// </summary>
	[JsonProperty("mfa_level")]
	public Optional<MfaLevel> MfaLevel { get; set; }

	/// <summary>
	///     Gets or sets the explicit content filter.
	/// </summary>
	[JsonProperty("explicit_content_filter")]
	public Optional<ExplicitContentFilter> ExplicitContentFilter { get; set; }

	/// <summary>
	///     Gets or sets the system channel id.
	/// </summary>
	[JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong?> SystemChannelId { get; set; }

	/// <summary>
	///     Gets or sets the safety alerts channel id.
	/// </summary>
	[JsonProperty("safety_alerts_channel_id", NullValueHandling = NullValueHandling.Include)]
	internal Optional<ulong?> SafetyAlertsChannelId { get; set; }

	/// <summary>
	///     Gets or sets the system channel flags.
	/// </summary>
	[JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<SystemChannelFlags> SystemChannelFlags { get; set; }

	/// <summary>
	///     Gets or sets the rules channel id.
	/// </summary>
	[JsonProperty("rules_channel_id")]
	public Optional<ulong?> RulesChannelId { get; set; }

	/// <summary>
	///     Gets or sets the public updates channel id.
	/// </summary>
	[JsonProperty("public_updates_channel_id")]
	public Optional<ulong?> PublicUpdatesChannelId { get; set; }

	/// <summary>
	///     Gets or sets the preferred locale.
	/// </summary>
	[JsonProperty("preferred_locale")]
	public Optional<string> PreferredLocale { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> Description { get; set; }

	/// <summary>
	///     Gets or sets whether the premium progress bar should be enabled.
	/// </summary>
	[JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> PremiumProgressBarEnabled { get; set; }
}

/// <summary>
///     Represents a guild mfa level modify payload.
/// </summary>
internal sealed class RestGuildMfaLevelModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the mfa level.
	/// </summary>
	[JsonProperty("level")]
	public MfaLevel Level { get; set; }
}

/// <summary>
///     Represents a guild community modify payload.
/// </summary>
internal sealed class RestGuildFeatureModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Features { get; set; }
}

internal sealed class RestGuildSafetyModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the safety alerts channel id.
	/// </summary>
	[JsonProperty("safety_alerts_channel_id", NullValueHandling = NullValueHandling.Include)]
	internal Optional<ulong?> SafetyAlertsChannelId { get; set; }

	/// <summary>
	///     Gets or sets the features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Features { get; set; }
}

/// <summary>
///     Represents a guild community modify payload.
/// </summary>
internal sealed class RestGuildCommunityModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the verification level.
	/// </summary>
	[JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<VerificationLevel> VerificationLevel { get; set; }

	/// <summary>
	///     Gets or sets the default message notifications.
	/// </summary>
	[JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<DefaultMessageNotifications> DefaultMessageNotifications { get; set; }

	/// <summary>
	///     Gets or sets the explicit content filter.
	/// </summary>
	[JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ExplicitContentFilter> ExplicitContentFilter { get; set; }

	/// <summary>
	///     Gets or sets the rules channel id.
	/// </summary>
	[JsonProperty("rules_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ulong?> RulesChannelId { get; set; }

	/// <summary>
	///     Gets or sets the public updates channel id.
	/// </summary>
	[JsonProperty("public_updates_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ulong?> PublicUpdatesChannelId { get; set; }

	/// <summary>
	///     Gets or sets the preferred locale.
	/// </summary>
	[JsonProperty("preferred_locale")]
	public Optional<string> PreferredLocale { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public Optional<string> Description { get; set; }

	/// <summary>
	///     Gets or sets the features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public List<string> Features { get; set; }
}

/// <summary>
///     Represents guild inventory settings.
/// </summary>
internal sealed class RestGuildInventoryModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets whether anyone is allowed to use and collect this guild's emojis.
	/// </summary>
	[JsonProperty("is_emoji_pack_collectible")]
	public bool IsEmojiPackCollectible { get; set; }
}

/// <summary>
///     Represents guild incidents actions.
/// </summary>
internal sealed class RestGuildIncidentActionsModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets until when invites are disabled.
	/// </summary>
	[JsonProperty("invites_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTimeOffset? InvitesDisabledUntil { get; set; }

	/// <summary>
	///     Gets until when direct messages are disabled.
	/// </summary>
	[JsonProperty("dms_disabled_until", NullValueHandling = NullValueHandling.Include)]
	public DateTimeOffset? DmsDisabledUntil { get; set; }
}

/// <summary>
///     Represents guild onboarding settings.
/// </summary>
internal sealed class RestGuildOnboardingModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the prompts shown during onboarding and in customize community.
	/// </summary>
	[JsonProperty("prompts", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<DiscordOnboardingPrompt>> Prompts { get; set; }

	/// <summary>
	///     Gets or sets the channel ids that members get opted into automatically.
	/// </summary>
	[JsonProperty("default_channel_ids", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<ulong>> DefaultChannelIds { get; set; }

	/// <summary>
	///     Gets or sets whether onboarding is enabled in the guild.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	/// <summary>
	///     Gets or sets the mode of onboarding.
	/// </summary>
	[JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<OnboardingMode> Mode { get; set; }
}

/// <summary>
///     Represents guild server guide settings.
/// </summary>
internal sealed class RestGuildServerGuideModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets whether server guide is enabled in the guild.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	/// <summary>
	///     Gets or sets the server guide welcome message.
	/// </summary>
	[JsonProperty("welcome_message", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<WelcomeMessage> WelcomeMessage { get; set; }

	/// <summary>
	///     Gets or sets the new member actions.
	/// </summary>
	[JsonProperty("new_member_actions", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<NewMemberAction>> NewMemberActions { get; set; }

	/// <summary>
	///     Gets or sets the resource channels.
	/// </summary>
	[JsonProperty("resource_channels", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<List<ResourceChannel>> ResourceChannels { get; set; }
}

/// <summary>
///     Represents a guild member add payload.
/// </summary>
internal sealed class RestGuildMemberAddPayload : IOAuth2Payload
{
	/// <summary>
	///     Gets or sets the nickname.
	/// </summary>
	[JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
	public string? Nickname { get; set; }

	/// <summary>
	///     Gets or sets the roles.
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public IEnumerable<DiscordRole>? Roles { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether mute.
	/// </summary>
	[JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Mute { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether deaf.
	/// </summary>
	[JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Deaf { get; set; }

	/// <summary>
	///     Gets or sets the access token.
	/// </summary>
	[JsonProperty("access_token")]
	public string AccessToken { get; set; }
}

/// <summary>
///     Represents a guild channel reorder payload.
/// </summary>
internal sealed class RestGuildChannelReorderPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; set; }
}

/// <summary>
///     Represents a guild channel new parent payload.
/// </summary>
internal sealed class RestGuildChannelNewParentPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; set; }

	/// <summary>
	///     Gets or sets the parent id.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<ulong?> ParentId { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether lock permissions.
	/// </summary>
	[JsonProperty("lock_permissions", NullValueHandling = NullValueHandling.Ignore)]
	public bool? LockPermissions { get; set; }
}

/// <summary>
///     Represents a guild channel no parent payload.
/// </summary>
internal sealed class RestGuildChannelNoParentPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; set; }

	/// <summary>
	///     Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; set; }

	/// <summary>
	///     Gets or sets the parent id.
	/// </summary>
	[JsonProperty("parent_id", NullValueHandling = NullValueHandling.Include)]
	public Optional<ulong?> ParentId { get; set; }
}

/// <summary>
///     Represents a guild role reorder payload.
/// </summary>
internal sealed class RestGuildRoleReorderPayload
{
	/// <summary>
	///     Gets or sets the role id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong RoleId { get; set; }

	/// <summary>
	///     Gets or sets the position.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; set; }
}

/// <summary>
///     Represents a guild member modify payload.
/// </summary>
internal sealed class RestGuildMemberModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the nickname.
	/// </summary>
	[JsonProperty("nick")]
	public Optional<string> Nickname { get; set; }

	/// <summary>
	///     Gets or sets the role ids.
	/// </summary>
	[JsonProperty("roles")]
	public Optional<IEnumerable<ulong>> RoleIds { get; set; }

	/// <summary>
	///     Gets or sets the mute.
	/// </summary>
	[JsonProperty("mute")]
	public Optional<bool> Mute { get; set; }

	/// <summary>
	///     Gets or sets the deafen.
	/// </summary>
	[JsonProperty("deaf")]
	public Optional<bool> Deafen { get; set; }

	/// <summary>
	///     Gets or sets the voice channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public Optional<ulong?> VoiceChannelId { get; set; }

	[JsonProperty("flags")]
	public Optional<MemberFlags> Flags { get; set; }
}

/// <summary>
///     Represents a guild member timeout modify payload.
/// </summary>
internal sealed class RestGuildMemberTimeoutModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the date until the member can communicate again.
	/// </summary>
	[JsonProperty("communication_disabled_until")]
	public DateTimeOffset? CommunicationDisabledUntil { get; internal set; }
}

/// <summary>
///     Represents a guild role payload.
/// </summary>
internal sealed class RestGuildRolePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the permissions.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? Permissions { get; set; }

	/// <summary>
	///     Gets or sets the color.
	/// </summary>
	[JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
	public int? Color { get; set; }

	/// <summary>
	///     Gets or sets the colors.
	/// </summary>
	[JsonProperty("colors", NullValueHandling = NullValueHandling.Ignore)]
	public RoleColors? Colors { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether hoist.
	/// </summary>
	[JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Hoist { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether mentionable.
	/// </summary>
	[JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Mentionable { get; set; }

	/// <summary>
	///     Gets or sets the icon base64.
	/// </summary>
	[JsonProperty("icon")]
	public Optional<string> IconBase64 { get; set; }

	/// <summary>
	///     Gets or sets the icon base64.
	/// </summary>
	[JsonProperty("unicode_emoji")]
	public Optional<string> UnicodeEmoji { get; set; }

	/// <summary>
	///     Role colors.
	/// </summary>
	public sealed class RoleColors
	{
		/// <summary>
		///     Gets the primary color. Is the same as <see cref="DiscordRole.Color" />.
		/// </summary>
		[JsonProperty("primary_color", NullValueHandling = NullValueHandling.Ignore)]
		public int PrimaryColor { get; set; }

		/// <summary>
		///     Gets the secondary color. Uses for gradient style.
		/// </summary>
		[JsonProperty("secondary_color", NullValueHandling = NullValueHandling.Include)]
		public int? SecondaryColor { get; set; }

		/// <summary>
		///     Gets the tertiary color. This only applies to the <c>holographic</c> role style and must have the value
		///     <c>16761760</c>.
		/// </summary>
		[JsonProperty("tertiary_color", NullValueHandling = NullValueHandling.Include)]
		public int? TertiaryColor { get; set; }
	}
}

/// <summary>
///     Represents a guild prune result payload.
/// </summary>
internal sealed class RestGuildPruneResultPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the pruned.
	/// </summary>
	[JsonProperty("pruned", NullValueHandling = NullValueHandling.Ignore)]
	public int? Pruned { get; set; }
}

/// <summary>
///     Represents a guild integration attach payload.
/// </summary>
internal sealed class RestGuildIntegrationAttachPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the type.
	/// </summary>
	[JsonProperty("type")]
	public string Type { get; set; }

	/// <summary>
	///     Gets or sets the id.
	/// </summary>
	[JsonProperty("id")]
	public ulong Id { get; set; }
}

/// <summary>
///     Represents a guild join request update payload.
/// </summary>
internal sealed class RestGuildJoinRequestUpdatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the action.
	/// </summary>
	[JsonProperty("action")]
	public string Action { get; set; }

	/// <summary>
	///     Gets or sets the reason.
	/// </summary>
	[JsonProperty("rejection_reason", NullValueHandling = NullValueHandling.Ignore)]
	public string? RejectionReason { get; set; }
}

/// <summary>
///     Represents a guild integration modify payload.
/// </summary>
internal sealed class RestGuildIntegrationModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the expire behavior.
	/// </summary>
	[JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
	public int? ExpireBehavior { get; set; }

	/// <summary>
	///     Gets or sets the expire grace period.
	/// </summary>
	[JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
	public int? ExpireGracePeriod { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether enable emoticons.
	/// </summary>
	[JsonProperty("enable_emoticons", NullValueHandling = NullValueHandling.Ignore)]
	public bool? EnableEmoticons { get; set; }
}

/// <summary>
///     Represents a guild emoji modify payload.
/// </summary>
internal class RestGuildEmojiModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	///     Gets or sets the roles.
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public ulong[] Roles { get; set; }
}

/// <summary>
///     Represents a guild emoji create payload.
/// </summary>
internal class RestGuildEmojiCreatePayload : RestGuildEmojiModifyPayload
{
	/// <summary>
	///     Gets or sets the image b64.
	/// </summary>
	[JsonProperty("image")]
	public string ImageB64 { get; set; }
}

/// <summary>
///     Represents a guild widget settings payload.
/// </summary>
internal class RestGuildWidgetSettingsPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets a value indicating whether enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Enabled { get; set; }

	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ChannelId { get; set; }
}

/// <summary>
///     Represents a guild template create or modify payload.
/// </summary>
internal class RestGuildTemplateCreateOrModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public required string Name { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
	public string? Description { get; set; }
}

/// <summary>
///     Represents a guild membership screening form modify payload.
/// </summary>
internal class RestGuildMembershipScreeningFormModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	/// <summary>
	///     Gets or sets the fields.
	/// </summary>
	[JsonProperty("form_fields", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<DiscordGuildMembershipScreeningField[]> Fields { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Description { get; set; }
}

/// <summary>
///     Represents a guild welcome screen modify payload.
/// </summary>
internal class RestGuildWelcomeScreenModifyPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the enabled.
	/// </summary>
	[JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<bool> Enabled { get; set; }

	/// <summary>
	///     Gets or sets the welcome channels.
	/// </summary>
	[JsonProperty("welcome_channels", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<IEnumerable<DiscordGuildWelcomeScreenChannel>> WelcomeChannels { get; set; }

	/// <summary>
	///     Gets or sets the description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<string> Description { get; set; }
}

/// <summary>
///     Represents a guild update current user voice state payload.
/// </summary>
internal class RestGuildUpdateCurrentUserVoiceStatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether suppress.
	/// </summary>
	[JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Suppress { get; set; }

	/// <summary>
	///     Gets or sets the request to speak timestamp.
	/// </summary>
	[JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? RequestToSpeakTimestamp { get; set; }
}

/// <summary>
///     Represents a guild update user voice state payload.
/// </summary>
internal class RestGuildUpdateUserVoiceStatePayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the channel id.
	/// </summary>
	[JsonProperty("channel_id")]
	public ulong ChannelId { get; set; }

	/// <summary>
	///     Gets or sets a value indicating whether suppress.
	/// </summary>
	[JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Suppress { get; set; }
}

/// <summary>
///     Represents a guild ban payload.
/// </summary>
internal sealed class RestGuildBanPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the delete message seconds.
	/// </summary>
	[JsonProperty("delete_message_seconds", NullValueHandling = NullValueHandling.Include)]
	public int DeleteMessageSeconds { get; set; }
}

/// <summary>
///     Represents a guild bulk ban payload.
/// </summary>
internal sealed class RestGuildBulkBanPayload : ObservableApiObject
{
	/// <summary>
	///     Gets or sets the user ids.
	/// </summary>
	[JsonProperty("user_ids", NullValueHandling = NullValueHandling.Ignore)]
	public List<ulong> UserIds { get; set; }

	/// <summary>
	///     Gets or sets the delete message seconds.
	/// </summary>
	[JsonProperty("delete_message_seconds", NullValueHandling = NullValueHandling.Include)]
	public int DeleteMessageSeconds { get; set; }
}
