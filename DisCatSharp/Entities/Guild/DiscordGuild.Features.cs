// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the guild features.
/// </summary>
public class GuildFeatures
{
	/// <summary>
	/// Guild has access to set an animated guild icon.
	/// </summary>
	public bool CanSetAnimatedIcon { get; }

	/// <summary>
	/// Guild has access to set a guild banner image.
	/// </summary>
	public bool CanSetBanner { get; }

	/// <summary>
	/// Guild has access to use commerce features (i.e. create store channels)
	/// </summary>
	public bool CanCreateStoreChannels { get; }

	/// <summary>
	/// Guild can enable Welcome Screen, Membership Screening, Stage Channels, News Channels and receives community updates.
	/// Furthermore the guild can apply as a partner and for the discovery (if the prerequisites are given).
	/// <see cref="ChannelType.Stage"/> and <see cref="ChannelType.News"/> is usable.
	/// </summary>
	public bool HasCommunityEnabled { get; }

	/// <summary>
	/// Guild is able to be discovered in the discovery.
	/// </summary>
	public bool IsDiscoverable { get; }

	/// <summary>
	/// Guild is able to be featured in the discovery.
	/// </summary>
	public bool IsFeatureable { get; }

	/// <summary>
	/// Guild has access to set an invite splash background.
	/// </summary>
	public bool CanSetInviteSplash { get; }

	/// <summary>
	/// Guild has enabled Membership Screening.
	/// </summary>
	public bool HasMembershipScreeningEnabled { get; }

	/// <summary>
	/// Guild has access to create news channels.
	/// <see cref="ChannelType.News"/> is usable.
	/// </summary>
	public bool CanCreateNewsChannels { get; }

	/// <summary>
	/// Guild is partnered.
	/// </summary>
	public bool IsPartnered { get; }

	/// <summary>
	/// Guild has increased custom emoji slots.
	/// </summary>
	public bool CanUploadMoreEmojis { get; }

	/// <summary>
	/// Guild can be previewed before joining via Membership Screening or the discovery.
	/// </summary>
	public bool HasPreviewEnabled { get; }

	/// <summary>
	/// Guild has access to set a vanity URL.
	/// </summary>
	public bool CanSetVanityUrl { get; }

	/// <summary>
	/// Guild is verified.
	/// </summary>
	public bool IsVerified { get; }

	/// <summary>
	/// Guild has access to set 384kbps bitrate in voice (previously VIP voice servers).
	/// </summary>
	public bool CanAccessVipRegions { get; }

	/// <summary>
	/// Guild has enabled the welcome screen.
	/// </summary>
	public bool HasWelcomeScreenEnabled { get; }

	/// <summary>
	/// Guild has enabled ticketed events.
	/// </summary>
	public bool HasTicketedEventsEnabled { get; }

	/// <summary>
	/// Guild has enabled monetization.
	/// </summary>
	public bool HasMonetizationEnabled { get; }

	/// <summary>
	/// Guild has increased custom sticker slots.
	/// </summary>
	public bool CanUploadMoreStickers { get; }

	/// <summary>
	/// Guild has access to the three day archive time for threads.
	/// Needs Premium Tier 1 (<see cref="PremiumTier.TierOne"/>).
	/// </summary>
	public bool CanSetThreadArchiveDurationThreeDays { get; }

	/// <summary>
	/// Guild has access to the seven day archive time for threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	public bool CanSetThreadArchiveDurationSevenDays { get; }

	/// <summary>
	/// Guild has access to create private threads.
	/// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
	/// </summary>
	public bool CanCreatePrivateThreads { get; }

	/// <summary>
	/// Guild is a hub.
	/// <see cref="ChannelType.GuildDirectory"/> is usable.
	/// </summary>
	public bool IsHub { get; }

	/// <summary>
	/// Guild is in a hub.
	/// https://github.com/discord/discord-api-docs/pull/3757/commits/4932d92c9d0c783861bc715bf7ebbabb15114e34
	/// </summary>
	public bool HasDirectoryEntry { get; }

	/// <summary>
	/// Guild is linked to a hub.
	/// </summary>
	public bool IsLinkedToHub { get; }

	/// <summary>
	/// Guild has full access to threads.
	/// Old Feature.
	/// </summary>
	public bool HasThreadTestingEnabled { get; }

	/// <summary>
	/// Guild has access to threads.
	/// </summary>
	public bool HasThreadsEnabled { get; }

	/// <summary>
	/// Guild can set role icons.
	/// </summary>
	public bool CanSetRoleIcons { get; }

	/// <summary>
	/// Guild has the new thread permissions.
	/// Old Feature.
	/// </summary>
	public bool HasNewThreadPermissions { get; }

	/// <summary>
	/// Guild can set thread default auto archive duration.
	/// Old Feature.
	/// </summary>
	public bool CanSetThreadDefaultAutoArchiveDuration { get; }

	/// <summary>
	/// Guild has enabled role subscriptions.
	/// </summary>
	public bool HasRoleSubscriptionsEnabled { get; }

	/// <summary>
	/// Guild role subscriptions as purchaseable.
	/// </summary>
	public bool RoleSubscriptionsIsAvailableForPurchase { get; }

	/// <summary>
	/// Guild has premium tier 3 override.
	/// </summary>
	public bool PremiumTierThreeOverride { get; }

	/// <summary>
	/// Guild has access to text in voice.
	/// Restricted to <see cref="IsStaffOnly"/>.
	/// </summary>
	public bool TextInVoiceEnabled { get; }

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	public bool CanSetAnimatedBanner { get; }

	/// <summary>
	/// Guild can set an animated banner.
	/// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
	/// </summary>
	public bool CanSetChannelBanner { get; }

	/// <summary>
	/// Allows members to customize their avatar, banner and bio for that server.
	/// </summary>
	public bool HasMemberProfiles { get; }

	/// <summary>
	/// Guild is restricted to users with the <see cref="UserFlags.Staff"/> badge.
	/// </summary>
	public bool IsStaffOnly { get; }

	/// <summary>
	/// Guild can use and setup the experimental auto moderation feature.
	/// </summary>
	public bool CanSetupAutoModeration { get; }

	/// <summary>
	/// String of guild features.
	/// </summary>
	public string FeatureString { get; }

	/// <summary>
	/// Checks the guild features and constructs a new <see cref="GuildFeatures"/> object.
	/// </summary>
	/// <param name="guild">Guild to check</param>
	public GuildFeatures(DiscordGuild guild)
	{
		this.CanSetAnimatedIcon = guild.RawFeatures.Contains("ANIMATED_ICON");
		this.CanSetAnimatedBanner = guild.RawFeatures.Contains("ANIMATED_BANNER");
		this.CanSetBanner = guild.RawFeatures.Contains("BANNER");
		this.CanSetChannelBanner = guild.RawFeatures.Contains("CHANNEL_BANNER");
		this.CanCreateStoreChannels = guild.RawFeatures.Contains("COMMERCE");
		this.HasCommunityEnabled = guild.RawFeatures.Contains("COMMUNITY");
		this.IsDiscoverable = !guild.RawFeatures.Contains("DISCOVERABLE_DISABLED") && guild.RawFeatures.Contains("DISCOVERABLE");
		this.IsFeatureable = guild.RawFeatures.Contains("FEATUREABLE");
		this.CanSetInviteSplash = guild.RawFeatures.Contains("INVITE_SPLASH");
		this.HasMembershipScreeningEnabled = guild.RawFeatures.Contains("MEMBER_VERIFICATION_GATE_ENABLED");
		this.CanCreateNewsChannels = guild.RawFeatures.Contains("NEWS");
		this.IsPartnered = guild.RawFeatures.Contains("PARTNERED");
		this.CanUploadMoreEmojis = guild.RawFeatures.Contains("MORE_EMOJI");
		this.HasPreviewEnabled = guild.RawFeatures.Contains("PREVIEW_ENABLED");
		this.CanSetVanityUrl = guild.RawFeatures.Contains("VANITY_URL");
		this.IsVerified = guild.RawFeatures.Contains("VERIFIED");
		this.CanAccessVipRegions = guild.RawFeatures.Contains("VIP_REGIONS");
		this.HasWelcomeScreenEnabled = guild.RawFeatures.Contains("WELCOME_SCREEN_ENABLED");
		this.HasTicketedEventsEnabled = guild.RawFeatures.Contains("TICKETED_EVENTS_ENABLED");
		this.HasMonetizationEnabled = guild.RawFeatures.Contains("MONETIZATION_ENABLED");
		this.CanUploadMoreStickers = guild.RawFeatures.Contains("MORE_STICKERS");
		this.CanSetThreadArchiveDurationThreeDays = guild.RawFeatures.Contains("THREE_DAY_THREAD_ARCHIVE");
		this.CanSetThreadArchiveDurationSevenDays = guild.RawFeatures.Contains("SEVEN_DAY_THREAD_ARCHIVE");
		this.CanCreatePrivateThreads = guild.RawFeatures.Contains("PRIVATE_THREADS");
		this.IsHub = guild.RawFeatures.Contains("HUB");
		this.HasThreadTestingEnabled = guild.RawFeatures.Contains("THREADS_ENABLED_TESTING");
		this.HasThreadsEnabled = guild.RawFeatures.Contains("THREADS_ENABLED");
		this.CanSetRoleIcons = guild.RawFeatures.Contains("ROLE_ICONS");
		this.HasNewThreadPermissions = guild.RawFeatures.Contains("NEW_THREAD_PERMISSIONS");
		this.HasRoleSubscriptionsEnabled = guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_ENABLED");
		this.PremiumTierThreeOverride = guild.RawFeatures.Contains("PREMIUM_TIER_3_OVERRIDE");
		this.CanSetThreadDefaultAutoArchiveDuration = guild.RawFeatures.Contains("THREAD_DEFAULT_AUTO_ARCHIVE_DURATION");
		this.TextInVoiceEnabled = guild.RawFeatures.Contains("TEXT_IN_VOICE_ENABLED");
		this.HasDirectoryEntry = guild.RawFeatures.Contains("HAS_DIRECTORY_ENTRY");
		this.IsLinkedToHub = guild.RawFeatures.Contains("LINKED_TO_HUB");
		this.HasMemberProfiles = guild.RawFeatures.Contains("MEMBER_PROFILES");
		this.IsStaffOnly = guild.RawFeatures.Contains("INTERNAL_EMPLOYEE_ONLY");
		this.RoleSubscriptionsIsAvailableForPurchase = guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE");
		this.CanSetupAutoModeration = guild.RawFeatures.Contains("AUTO_MODERATION");

		var features = guild.RawFeatures.Any() ? "" : "None";
		foreach (var feature in guild.RawFeatures)
		{
			features += feature + " ";
		}
		this.FeatureString = features;

	}
}
