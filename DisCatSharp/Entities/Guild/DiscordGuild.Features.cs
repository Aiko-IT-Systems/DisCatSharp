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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the guild features.
/// </summary>
public class GuildFeatures
{
	/// <summary>
	/// List of all guild features.
	/// </summary>
	public List<GuildFeaturesEnum> Features { get; }

	/// <summary>
	/// Checks the guild features and constructs a new <see cref="GuildFeatures"/> object.
	/// </summary>
	/// <param name="guild">Guild to check</param>
	public GuildFeatures(DiscordGuild guild)
	{
		this.Features = new List<GuildFeaturesEnum>();

		if (guild.RawFeatures.Contains("APPLICATION_COMMAND_PERMISSIONS_V2")) this.Features.Add(GuildFeaturesEnum.UsesApplicationCommandsPermissionsV2);
		if (guild.RawFeatures.Contains("RAID_ALERTS_ENABLED")) this.Features.Add(GuildFeaturesEnum.RaidAlertsEnabled);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZABLE_RESTRICTED")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizableRestricted);
		if (guild.RawFeatures.Contains("VOICE_IN_THREADS")) this.Features.Add(GuildFeaturesEnum.VoiceInThreadsEnabled);
		if (guild.RawFeatures.Contains("CHANNEL_HIGHLIGHTS_DISABLED")) this.Features.Add(GuildFeaturesEnum.ChannelHighlightsDisabled);
		if (guild.RawFeatures.Contains("CHANNEL_HIGHLIGHTS")) this.Features.Add(GuildFeaturesEnum.ChannelHighlights);
		if (guild.RawFeatures.Contains("GUILD_ONBOARDING_EVER_ENABLED")) this.Features.Add(GuildFeaturesEnum.HadGuildOnBoardingEverEnabled);
		if (guild.RawFeatures.Contains("BURST_REACTIONS")) this.Features.Add(GuildFeaturesEnum.CanUseBurstReactions);
		if (guild.RawFeatures.Contains("CREATOR_STORE_PAGE")) this.Features.Add(GuildFeaturesEnum.CanUseCreatorStorePage);

		if (guild.RawFeatures.Contains("ANIMATED_ICON")) this.Features.Add(GuildFeaturesEnum.CanSetAnimatedIcon);
		if (guild.RawFeatures.Contains("ANIMATED_BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetAnimatedBanner);
		if (guild.RawFeatures.Contains("BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetBanner);
		if (guild.RawFeatures.Contains("COMMUNITY")) this.Features.Add(GuildFeaturesEnum.HasCommunityEnabled);
		if (!guild.RawFeatures.Contains("DISCOVERABLE_DISABLED") && guild.RawFeatures.Contains("DISCOVERABLE")) this.Features.Add(GuildFeaturesEnum.IsDiscoverable);
		if (guild.RawFeatures.Contains("FEATUREABLE")) this.Features.Add(GuildFeaturesEnum.IsFeatureable);
		if (guild.RawFeatures.Contains("INVITE_SPLASH")) this.Features.Add(GuildFeaturesEnum.CanSetInviteSplash);
		if (guild.RawFeatures.Contains("MEMBER_VERIFICATION_GATE_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasMembershipScreeningEnabled);
		if (guild.RawFeatures.Contains("NEWS")) this.Features.Add(GuildFeaturesEnum.CanCreateNewsChannels);
		if (guild.RawFeatures.Contains("PARTNERED")) this.Features.Add(GuildFeaturesEnum.IsPartnered);
		if (guild.RawFeatures.Contains("MORE_EMOJI")) this.Features.Add(GuildFeaturesEnum.CanUploadMoreEmojis);
		if (guild.RawFeatures.Contains("PREVIEW_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasPreviewEnabled);
		if (guild.RawFeatures.Contains("VANITY_URL")) this.Features.Add(GuildFeaturesEnum.CanSetVanityUrl);
		if (guild.RawFeatures.Contains("VERIFIED")) this.Features.Add(GuildFeaturesEnum.IsVerified);
		if (guild.RawFeatures.Contains("VIP_REGIONS")) this.Features.Add(GuildFeaturesEnum.CanAccessVipRegions);
		if (guild.RawFeatures.Contains("WELCOME_SCREEN_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasWelcomeScreenEnabled);
		if (guild.RawFeatures.Contains("TICKETED_EVENTS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasTicketedEventsEnabled);
		if (guild.RawFeatures.Contains("MONETIZATION_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasMonetizationEnabled);
		if (guild.RawFeatures.Contains("MORE_STICKERS")) this.Features.Add(GuildFeaturesEnum.CanUploadMoreStickers);
		if (guild.RawFeatures.Contains("HUB")) this.Features.Add(GuildFeaturesEnum.IsHub);
		if (guild.RawFeatures.Contains("THREADS_ENABLED_TESTING")) this.Features.Add(GuildFeaturesEnum.HasThreadTestingEnabled);
		if (guild.RawFeatures.Contains("THREADS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasThreadsEnabled);
		if (guild.RawFeatures.Contains("ROLE_ICONS")) this.Features.Add(GuildFeaturesEnum.CanSetRoleIcons);
		if (guild.RawFeatures.Contains("NEW_THREAD_PERMISSIONS")) this.Features.Add(GuildFeaturesEnum.HasNewThreadPermissions);
		if (guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_ENABLED")) this.Features.Add(GuildFeaturesEnum.HasRoleSubscriptionsEnabled);
		if (guild.RawFeatures.Contains("PREMIUM_TIER_3_OVERRIDE")) this.Features.Add(GuildFeaturesEnum.PremiumTierThreeOverride);
		if (guild.RawFeatures.Contains("THREAD_DEFAULT_AUTO_ARCHIVE_DURATION")) this.Features.Add(GuildFeaturesEnum.CanSetThreadDefaultAutoArchiveDuration);
		if (guild.RawFeatures.Contains("TEXT_IN_VOICE_ENABLED")) this.Features.Add(GuildFeaturesEnum.TextInVoiceEnabled);
		if (guild.RawFeatures.Contains("HAS_DIRECTORY_ENTRY")) this.Features.Add(GuildFeaturesEnum.HasDirectoryEntry);
		if (guild.RawFeatures.Contains("LINKED_TO_HUB")) this.Features.Add(GuildFeaturesEnum.IsLinkedToHub);
		if (guild.RawFeatures.Contains("MEMBER_PROFILES")) this.Features.Add(GuildFeaturesEnum.HasMemberProfiles);
		if (guild.RawFeatures.Contains("INTERNAL_EMPLOYEE_ONLY")) this.Features.Add(GuildFeaturesEnum.IsStaffOnly);
		if (guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE")) this.Features.Add(GuildFeaturesEnum.RoleSubscriptionsIsAvailableForPurchase);
		if (guild.RawFeatures.Contains("AUTO_MODERATION")) this.Features.Add(GuildFeaturesEnum.CanSetupAutoModeration);
		if (guild.RawFeatures.Contains("GUILD_HOME_TEST")) this.Features.Add(GuildFeaturesEnum.GuildHomeTest);
		if (guild.RawFeatures.Contains("INVITES_DISABLED")) this.Features.Add(GuildFeaturesEnum.InvitesDisabled);
		if (guild.RawFeatures.Contains("ACTIVITIES_ALPHA")) this.Features.Add(GuildFeaturesEnum.ActivitiesAlpha);
		if (guild.RawFeatures.Contains("ACTIVITIES_EMPLOYEE")) this.Features.Add(GuildFeaturesEnum.ActivitiesEmployee);
		if (guild.RawFeatures.Contains("ACTIVITIES_INTERNAL_DEV")) this.Features.Add(GuildFeaturesEnum.ActivitiesInternalDev);
		if (guild.RawFeatures.Contains("AUTOMOD_TRIGGER_KEYWORD_FILTER")) this.Features.Add(GuildFeaturesEnum.AutomodTriggerKeywordFilter);
		if (guild.RawFeatures.Contains("AUTOMOD_TRIGGER_ML_SPAM_FILTER")) this.Features.Add(GuildFeaturesEnum.AutomodTriggerMlSpamFilter);
		if (guild.RawFeatures.Contains("AUTOMOD_TRIGGER_SPAM_LINK_FILTERGuild")) this.Features.Add(GuildFeaturesEnum.AutomodTriggerSpamLinkFilterGuild);
		if (guild.RawFeatures.Contains("AUTOMOD_DEFAULT_LIST")) this.Features.Add(GuildFeaturesEnum.AutomodDefaultList);
		if (guild.RawFeatures.Contains("BFG")) this.Features.Add(GuildFeaturesEnum.Bfg);
		if (guild.RawFeatures.Contains("BOOSTING_TIERS_EXPERIMENT_MEDIUM_GUILD")) this.Features.Add(GuildFeaturesEnum.BoostingTiersExperimentMediumGuild);
		if (guild.RawFeatures.Contains("BOOSTING_TIERS_EXPERIMENT_SMALL_GUILD")) this.Features.Add(GuildFeaturesEnum.BoostingTiersExperimentSmallGuild);
		if (guild.RawFeatures.Contains("BOT_DEVELOPER_EARLY_ACCESS")) this.Features.Add(GuildFeaturesEnum.BotDeveloperEarlyAccess);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZABLE")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizable);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZABLE_DISABLED")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizableDisabled);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZABLE_PROVISIONAL")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizableProvisional);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZABLE_WHITEGLOVE")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizableWhiteGlove);
		if (guild.RawFeatures.Contains("CREATOR_MONETIZATION_APPLICATION_ALLOWLIST")) this.Features.Add(GuildFeaturesEnum.CreatorMonetizationApplicationAllowlist);
		if (guild.RawFeatures.Contains("DEVELOPER_SUPPORT_SERVER")) this.Features.Add(GuildFeaturesEnum.DeveloperSupportServer);
		if (guild.RawFeatures.Contains("EXPOSED_TO_ACTIVITIES_WTP_EXPERIMENT")) this.Features.Add(GuildFeaturesEnum.ExposedToActivitiesWtpExperiment);
		if (guild.RawFeatures.Contains("GUILD_COMMUNICATION_DISABLED_GUILDS")) this.Features.Add(GuildFeaturesEnum.GuildCommunicationDisabledGuilds);
		if (guild.RawFeatures.Contains("DISABLE_GUILD_COMMUNICATION")) this.Features.Add(GuildFeaturesEnum.DisableGuildCommunication);
		if (guild.RawFeatures.Contains("GUILD_HOME_OVERRIDE")) this.Features.Add(GuildFeaturesEnum.GuildHomeOverride);
		if (guild.RawFeatures.Contains("GUILD_AUTOMOD_DEFAULT_LIST")) this.Features.Add(GuildFeaturesEnum.GuildAutomodDefaultList);
		if (guild.RawFeatures.Contains("GUILD_MEMBER_VERIFICATION_EXPERIMENT")) this.Features.Add(GuildFeaturesEnum.GuildMemberVerificationExperiment);
		if (guild.RawFeatures.Contains("GUILD_ROLE_SUBSCRIPTION_PURCHASE_FEEDBACK_LOOP")) this.Features.Add(GuildFeaturesEnum.GuildRoleSubscriptionPurchaseFeedbackLoop);
		if (guild.RawFeatures.Contains("GUILD_ROLE_SUBSCRIPTION_TRIALS")) this.Features.Add(GuildFeaturesEnum.GuildRoleSubscriptionTrials);
		if (guild.RawFeatures.Contains("HAD_EARLY_ACTIVITIES_ACCESS")) this.Features.Add(GuildFeaturesEnum.HadEarlyActivitiesAccess);
		if (guild.RawFeatures.Contains("INCREASED_THREAD_LIMIT")) this.Features.Add(GuildFeaturesEnum.IncreasedThreadLimit);
		if (guild.RawFeatures.Contains("MOBILE_WEB_ROLE_SUBSCRIPTION_PURCHASE_PAGE")) this.Features.Add(GuildFeaturesEnum.MobileWebRoleSubscriptionPurchasePage);
		if (guild.RawFeatures.Contains("RELAY_ENABLED")) this.Features.Add(GuildFeaturesEnum.RelayEnabled);
		if (guild.RawFeatures.Contains("RESTRICT_SPAM_RISK_GUILDS")) this.Features.Add(GuildFeaturesEnum.RestrictSpamRiskGuilds);
		if (guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE")) this.Features.Add(GuildFeaturesEnum.RoleSubscriptionsAvailableForPurchase);
		if (guild.RawFeatures.Contains("THREADS_ENABLED_TESTING")) this.Features.Add(GuildFeaturesEnum.ThreadsEnabledTesting);
		if (guild.RawFeatures.Contains("VOICE_CHANNEL_EFFECTS")) this.Features.Add(GuildFeaturesEnum.VoiceChannelEffects);
		if (guild.RawFeatures.Contains("SOUNDBOARD")) this.Features.Add(GuildFeaturesEnum.Soundboard);

		if (guild.RawFeatures.Contains("COMMERCE")) this.Features.Add(GuildFeaturesEnum.Commerce);
		if (guild.RawFeatures.Contains("EXPOSED_TO_BOOSTING_TIERS_EXPERIMENT")) this.Features.Add(GuildFeaturesEnum.ExposedToBoostingTiersExperiment);
		if (guild.RawFeatures.Contains("PUBLIC_DISABLED")) this.Features.Add(GuildFeaturesEnum.PublicDisabled);
		if (guild.RawFeatures.Contains("PUBLIC")) this.Features.Add(GuildFeaturesEnum.Public);
		if (guild.RawFeatures.Contains("SEVEN_DAY_THREAD_ARCHIVE")) this.Features.Add(GuildFeaturesEnum.SevenDayThreadArchive);
		if (guild.RawFeatures.Contains("THREE_DAY_THREAD_ARCHIVE")) this.Features.Add(GuildFeaturesEnum.ThreeDayThreadArchive);
		if (guild.RawFeatures.Contains("FEATURABLE")) this.Features.Add(GuildFeaturesEnum.Featurable);
		if (guild.RawFeatures.Contains("FORCE_RELAY")) this.Features.Add(GuildFeaturesEnum.ForceRelay);
		if (guild.RawFeatures.Contains("LURKABLE")) this.Features.Add(GuildFeaturesEnum.Lurkable);
		if (guild.RawFeatures.Contains("MEMBER_LIST_DISABLED")) this.Features.Add(GuildFeaturesEnum.MemberListDisabled);
		if (guild.RawFeatures.Contains("CHANNEL_BANNER")) this.Features.Add(GuildFeaturesEnum.CanSetChannelBanner);
		if (guild.RawFeatures.Contains("PRIVATE_THREADS")) this.Features.Add(GuildFeaturesEnum.CanCreatePrivateThreads);
	}

	/// <summary>
	/// Checks whether the guild has a feature enabled.
	/// </summary>
	/// <param name="flag">The feature you'd like to check for.</param>
	/// <returns>Whether the guild has the requested feature.</returns>
	public bool HasFeature(GuildFeaturesEnum flag)
		=> this.Features.Contains(flag);

	public string ToString(string separator, bool humanReadable)
	{
		if (!humanReadable) return string.Join(separator, this.Features);

		else
		{
			var humanReadableFeatures = this.Features.Select(x => AddSpacesToWord(x.ToString()));

			return string.Join(separator, humanReadableFeatures);
		}
	}

	/// <summary>
	/// Converts a string of characters (here: enum) into a string of characters separated by spaces after a capital letter.
	/// </summary>
	/// <param name="text">String of text to convert</param>
	/// <returns>String separated by a space after every capital letter.</returns>
	private static string AddSpacesToWord(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return "";
		var newText = new StringBuilder(text.Length * 2);
		newText.Append(text[0]);
		for (var i = 1; i < text.Length; i++)
		{
			if (char.IsUpper(text[i]) && text[i - 1] != ' ')
				newText.Append(' ');
			newText.Append(text[i]);
		}
		return newText.ToString();
	}
}

/// <summary>
/// Represents the guild features.
/// </summary>
public enum GuildFeaturesEnum
{
	/// <summary>
	/// Guild has access to set an animated guild icon.
	/// </summary>
	CanSetAnimatedIcon,

	/// <summary>
	/// Guild has access to set a guild banner image.
	/// </summary>
	CanSetBanner,

	/// <summary>
	/// Guild has access to use commerce features (i.e. create store channels)
	/// </summary>
	[Obsolete("Store applications are EOL.")]
	CanCreateStoreChannels,

	/// <summary>
	/// Guild can enable Welcome Screen, Membership Screening, Stage Channels, News Channels and receives community updates.
	/// Furthermore the guild can apply as a partner and for the discovery (if the prerequisites are given).
	/// <see cref="ChannelType.Stage"/> and <see cref="ChannelType.News"/> is usable.
	/// </summary>
	HasCommunityEnabled,

	/// <summary>
	/// Guild is able to be discovered in the discovery.
	/// </summary>
	IsDiscoverable,

	/// <summary>
	/// Guild is able to be featured in the discovery.
	/// </summary>
	IsFeatureable,

	/// <summary>
	/// Guild has access to set an invite splash background.
	/// </summary>
	CanSetInviteSplash,

	/// <summary>
	/// Guild has enabled Membership Screening.
	/// </summary>
	HasMembershipScreeningEnabled,

	/// <summary>
	/// Guild has access to create news channels.
	/// <see cref="ChannelType.News"/> is usable.
	/// </summary>
	CanCreateNewsChannels,

	/// <summary>
	/// Guild is partnered.
	/// </summary>
	IsPartnered,

	/// <summary>
	/// Guild has increased custom emoji slots.
	/// </summary>
	CanUploadMoreEmojis,

	/// <summary>
	/// Guild can be previewed before joining via Membership Screening or the discovery.
	/// </summary>
	HasPreviewEnabled,

	/// <summary>
	/// Guild has access to set a vanity URL.
	/// </summary>
	CanSetVanityUrl,

	/// <summary>
	/// Guild is verified.
	/// </summary>
	IsVerified,

	/// <summary>
	/// Guild has access to set 384kbps bitrate in voice (previously VIP voice servers).
	/// </summary>
	CanAccessVipRegions,

	/// <summary>
	/// Guild has enabled the welcome screen.
	/// </summary>
	HasWelcomeScreenEnabled,

	/// <summary>
	/// Guild has enabled ticketed events.
	/// </summary>
	HasTicketedEventsEnabled,

	/// <summary>
	/// Guild has enabled monetization.
	/// </summary>
	HasMonetizationEnabled,

	/// <summary>
	/// Guild has increased custom sticker slots.
	/// </summary>
	CanUploadMoreStickers,

	/// <summary>
	/// Guild has access to the three day archive time for threads.
	/// Needs Premium Tier 1 (<see cref="PremiumTier.TierOne"/>).
	/// </summary>
	[DiscordDeprecated("Auto archive duration isn't locked to boosts anymore."), Obsolete]
	CanSetThreadArchiveDurationThreeDays,

	/// <summary>
	/// Guild has access to the seven day archive time for threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	[DiscordDeprecated("Auto archive duration isn't locked to boosts anymore."), Obsolete]
	CanSetThreadArchiveDurationSevenDays,

	/// <summary>
	/// Guild has access to create private threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	[DiscordDeprecated("Private threads aren't bound to the server boost level anymore and can be used by everyone."), Obsolete]
	CanCreatePrivateThreads,

	/// <summary>
	/// Guild is a hub.
	/// <see cref="ChannelType.GuildDirectory"/> is usable.
	/// </summary>
	IsHub,

	/// <summary>
	/// Guild is in a hub.
	/// https://github.com/discord/discord-api-docs/pull/3757/commits/4932d92c9d0c783861bc715bf7ebbabb15114e34
	/// </summary>
	HasDirectoryEntry,

	/// <summary>
	/// Guild is linked to a hub.
	/// </summary>
	IsLinkedToHub,

	/// <summary>
	/// Guild has full access to threads.
	/// Old Feature.
	/// </summary>
	HasThreadTestingEnabled,

	/// <summary>
	/// Guild has access to threads.
	/// </summary>
	HasThreadsEnabled,

	/// <summary>
	/// Guild can set role icons.
	/// </summary>
	CanSetRoleIcons,

	/// <summary>
	/// Guild has the new thread permissions.
	/// Old Feature.
	/// </summary>
	HasNewThreadPermissions,

	/// <summary>
	/// Guild can set thread default auto archive duration.
	/// Old Feature.
	/// </summary>
	CanSetThreadDefaultAutoArchiveDuration,

	/// <summary>
	/// Guild has enabled role subscriptions.
	/// </summary>
	HasRoleSubscriptionsEnabled,

	/// <summary>
	/// Guild role subscriptions as purchaseable.
	/// </summary>
	RoleSubscriptionsIsAvailableForPurchase,

	/// <summary>
	/// Guild has premium tier 3 override.
	/// </summary>
	PremiumTierThreeOverride,

	/// <summary>
	/// Guild has access to text in voice.
	/// Restricted to <see cref="IsStaffOnly"/>.
	/// </summary>
	TextInVoiceEnabled,

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	CanSetAnimatedBanner,

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	[DiscordDeprecated("Feature was removed"), Obsolete]
	CanSetChannelBanner,

	/// <summary>
	/// Allows members to customize their avatar, banner and bio for that server.
	/// </summary>
	HasMemberProfiles,

	/// <summary>
	/// Guild is restricted to users with the <see cref="UserFlags.Staff"/> badge.
	/// </summary>
	IsStaffOnly,

	/// <summary>
	/// Guild can use and setup the experimental auto moderation feature.
	/// </summary>
	CanSetupAutoModeration,

	/// <summary>
	/// Guild has access to home.
	/// </summary>
	GuildHomeTest,

	/// <summary>
	/// Guild has disabled invites.
	/// </summary>
	InvitesDisabled,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	ActivitiesAlpha,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	ActivitiesEmployee,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	ActivitiesInternalDev,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	AutomodTriggerKeywordFilter,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	AutomodTriggerMlSpamFilter,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	AutomodTriggerSpamLinkFilterGuild,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	AutomodDefaultList,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	Bfg,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	BoostingTiersExperimentMediumGuild,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	BoostingTiersExperimentSmallGuild,

	/// <summary>
	/// Guild has early access features for bot and library developers.
	/// </summary>
	BotDeveloperEarlyAccess,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	CreatorMonetizable,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	CreatorMonetizableDisabled,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	CreatorMonetizableProvisional,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	CreatorMonetizableWhiteGlove,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	CreatorMonetizationApplicationAllowlist,

	/// <summary>
	/// Guild is set as a support server for an app in App Directory.
	/// </summary>
	DeveloperSupportServer,

	/// <summary>
	/// Guild was previously in the 2021-11_activities_baseline_engagement_bundle experiment.
	/// </summary>
	ExposedToActivitiesWtpExperiment,

	/// <summary>
	/// Guild had early access to the user timeouts.
	/// </summary>
	GuildCommunicationDisabledGuilds,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	DisableGuildCommunication,

	/// <summary>
	/// Guild has access to the Home feature.
	/// </summary>
	GuildHomeOverride,

	/// <summary>
	/// Guild had early access to the Automod Default List.
	/// </summary>
	GuildAutomodDefaultList,

	/// <summary>
	/// Guild had early access to approving membership manually.
	/// </summary>
	GuildMemberVerificationExperiment,

	/// <summary>
	/// Guilds was previously in the 2022-05_mobile_web_role_subscription_purchase_page experiment.
	/// </summary>
	GuildRoleSubscriptionPurchaseFeedbackLoop,

	/// <summary>
	/// Guild was previously in the 2022-01_guild_role_subscription_trials experiment.
	/// </summary>
	GuildRoleSubscriptionTrials,

	/// <summary>
	/// Guild previously had access to voice channel activities and can bypass the boost level requirement.
	/// </summary>
	HadEarlyActivitiesAccess,

	/// <summary>
	/// Allows the guild to have 1,000+ active threads.
	/// </summary>
	IncreasedThreadLimit,

	/// <summary>
	/// Guild was previously in the 2022-05_mobile_web_role_subscription_purchase_page experiment.
	/// </summary>
	MobileWebRoleSubscriptionPurchasePage,

	/// <summary>
	/// Shards connections to the guild to different nodes that relay information between each other.
	/// </summary>
	RelayEnabled,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	RestrictSpamRiskGuilds,

	/// <summary>
	/// Allows guild's members to purchase role subscriptions.
	/// </summary>
	RoleSubscriptionsAvailableForPurchase,

	/// <summary>
	/// Used by bot developers to test their bots with threads in guilds with 5 or less members and a bot.
	/// </summary>
	ThreadsEnabledTesting,

	/// <summary>
	/// Guild had early access to the voice channel effects.
	/// </summary>
	VoiceChannelEffects,

	/// <summary>
	/// Guild had early access to the soundboard feature.
	/// </summary>
	Soundboard,

	/// <summary>
	/// Ability to create and use store channels.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	Commerce,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	ExposedToBoostingTiersExperiment,

	/// <summary>
	/// Deprecated in favor of Community.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	PublicDisabled,

	/// <summary>
	/// Deprecated in favor of Community.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	Public,

	/// <summary>
	/// The guild can use the seven-day archive time for threads.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	SevenDayThreadArchive,

	/// <summary>
	/// The guild can use the three-day archive time for threads.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	ThreeDayThreadArchive,

	/// <summary>
	/// Previously used to control which servers were displayed under the "Featured" category in Discovery.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	Featurable,

	/// <summary>
	/// Shards connections to the guild to different nodes that relay information between each other.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	ForceRelay,

	/// <summary>
	/// Currently unknown.
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	Lurkable,

	/// <summary>
	/// Created for the Fortnite server blackout event on Oct 13, 2019, when viewing the member list it would show "There's nothing to see here.".
	/// </summary>
	[DiscordDeprecated("This feature is depcreated"), Obsolete]
	MemberListDisabled,

	[DiscordInExperiment]
	CanUseCreatorStorePage,

	[DiscordInExperiment]
	CanUseBurstReactions,

	[DiscordInExperiment]
	HadGuildOnBoardingEverEnabled,

	[DiscordInExperiment]
	ChannelHighlightsDisabled,

	[DiscordInExperiment]
	ChannelHighlights,

	[DiscordInExperiment]
	CreatorMonetizableRestricted,

	[DiscordUnreleased]
	VoiceInThreadsEnabled,

	[DiscordInExperiment("Feature related to automod.")]
	RaidAlertsEnabled,

	[DiscordInExperiment("Was recently added to determine whether guilds uses the newer permission system for application commands.")]
	UsesApplicationCommandsPermissionsV2,
}
